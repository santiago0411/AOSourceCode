using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Players;
using UnityEngine;
using Attribute = AO.Players.Attribute;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems
{
    public class Party
    {
        public class PartyMember
        {
            public readonly Player Player;
            public float ExperiencePercentage = 1f;

            public PartyMember(Player player)
            {
                Player = player;
            }

            public static explicit operator Player(PartyMember member)
            {
                return member.Player;
            }

            public byte ExperienceAsPercentage()
            {
                return (byte)Mathf.CeilToInt(ExperiencePercentage * 100);
            }
        }
        
        public Player Leader { get; private set; }

        private bool CanEditPercentages => Leader.Attributes[Attribute.Charisma] >= 19 && members.Count <= 2;
        private bool customPercentagesActive;
        
        private readonly List<PartyMember> members = new(Constants.MAX_PARTY_MEMBERS);
        
        public Party(Player leader)
        {
            Leader = leader;
            members.Add(new PartyMember(leader));
            PacketSender.OnYouJoinedParty(leader.Id, leader.Id, CanEditPercentages, members);
        }

        public void InvitePlayer(Player invitee)
        {
            if (Leader.Id == invitee.Id)
                return;
            
            if (members.Count >= Constants.MAX_PARTY_MEMBERS)
            {
                PacketSender.SendMultiMessage(Leader.Id, MultiMessage.PartyIsFull);
                return;
            }

            if (invitee.Party is not null)
            {
                PacketSender.SendMultiMessage(Leader.Id, MultiMessage.PlayerAlreadyInParty);
                return;
            }

            bool leaderIsCitizenOrImperial = Leader.Faction == Faction.Citizen || Leader.Faction == Faction.Imperial;
            
            if (leaderIsCitizenOrImperial)
            {
                bool inviteeIsCitizenOrImperial = invitee.Faction == Faction.Citizen || invitee.Faction == Faction.Imperial;
                if (inviteeIsCitizenOrImperial)
                {
                    SendInvite(invitee);
                    return;
                }
            }
            else // Leader is criminal or chaos
            {
                bool inviteeIsCriminalOrChaos = invitee.Faction == Faction.Criminal || invitee.Faction == Faction.Chaos;
                if (inviteeIsCriminalOrChaos)
                {
                    SendInvite(invitee);
                    return;
                }
            }

            PacketSender.SendMultiMessage(Leader.Id, MultiMessage.PlayerDifferentFaction);
        }

        public void AddPlayer(Player joiner)
        {
            joiner.Party = this;
            joiner.Flags.PendingPartyInvite = null;
            members.Add(new PartyMember(joiner));
            PacketSender.OnYouJoinedParty(joiner.Id, Leader.Id, false, members);
            PacketSender.OnPlayerJoinedParty(joiner.Id, members);
            PacketSender.OnCanEditPercentagesChanged(Leader.Id, CanEditPercentages);
            RecalculatePercentages();
        }

        public void RemoveMember(ClientId playerClientId, bool kicked)
        {
            var member = members.FirstOrDefault(m => m.Player.Id == playerClientId);
            if (member is null)
                return; // TODO maybe ban? Tried to kick a player that isn't in their party
            
            RemoveMember(member, kicked);
        }

        private void RemoveMember(PartyMember member, bool kicked)
        {
            var player = member.Player; 
            PacketSender.OnPlayerLeftParty(player.Id, kicked,members);
            
            members.Remove(member);
            player.Party = null;
            
            if (members.Count == 0)
                return;
            
            RecalculatePercentages();
            
            if (Leader.Id == player.Id && members.Count >= 1)
                SetNewLeader();
        }

        private void SendInvite(Player player)
        {
            player.Flags.PendingPartyInvite = this;
            PacketSender.SendMultiMessage(player.Id, MultiMessage.PlayerInvitedToParty,  stackalloc[] {Leader.Id.AsPrimitiveType()});
            PacketSender.SendMultiMessage(Leader.Id, MultiMessage.YouInvitedPlayerToParty,  stackalloc[] {player.Id.AsPrimitiveType()});
        }

        private void SetNewLeader()
        {
            Leader = members.First(m => m.Player.Id != Leader.Id).Player;
            PacketSender.OnCanEditPercentagesChanged(Leader.Id, CanEditPercentages);
            PacketSender.OnPartyLeaderChanged(Leader.Id, members);
        }
        
        public void TryChangePercentages(Dictionary<ClientId, byte> playerPercentages)
        {
            // TODO ban for invalid cases
            if (playerPercentages.Count != members.Count)
                return;

            if (members.Count == 1)
                return;
            
            foreach (var member in members)
                if (!playerPercentages.ContainsKey(member.Player.Id.AsPrimitiveType()))
                    return;

            if (playerPercentages.Values.Sum(x => x) != 100)
                return;

            if (playerPercentages.Values.Any(x => x > 90))
                return;
            
            foreach (var member in members)
            {
                var playerId = member.Player.Id;
                member.ExperiencePercentage = playerPercentages[playerId] / 100f;
            }

            customPercentagesActive = true;
            PacketSender.OnExperiencePercentageChanged(0, members);
        }

        private void RecalculatePercentages()
        {
            customPercentagesActive = false;
            
            foreach (var member in members)
                member.ExperiencePercentage = 1f / members.Count;

            float bonus = GetBonusMultiplier(members.Count, IsClassesBonusValid(members));
            // PercentageBonus is the value to be displayed on the client interface
            // Works as follows:
            // When the bonus is 1.8 (five players on the party)
            // 1.8 * 100 = 180 -> 180 - 100 = 80
            // Every member in the party gets 80% more experience
            var percentageBonus = (byte)Mathf.CeilToInt((bonus * 100) - 100);
            PacketSender.OnExperiencePercentageChanged(percentageBonus, members);
        }

        public void DistributeNpcExperience(Vector2 npcPosition, uint xp)
        {
            var playersInRange = new List<PartyMember>(Constants.MAX_PARTY_MEMBERS);

            // Find the party members that are in range of npc that yielded the xp
            foreach (var member in members)
                if ((member.Player.CurrentTile.Position - npcPosition).magnitude <= Constants.XP_DISTRIBUTION_RANGE)
                    playersInRange.Add(member);
            
            if (customPercentagesActive)
                DistributeExperienceCustom(playersInRange, xp);
            else
                DistributeExperienceEqually(playersInRange, xp);
        }

        private void DistributeExperienceCustom(List<PartyMember> playersInRange, uint xp)
        {
            foreach (var member in playersInRange)
            {
                var finalXp = (uint)(xp * member.ExperiencePercentage);
                PlayerMethods.AddExperience(member.Player, finalXp);
                PacketSender.OnPartyMemberGainedExperience(finalXp, member.Player.Id, members);
            }
        }

        private void DistributeExperienceEqually(List<PartyMember> playersInRange, uint xp)
        {
            // Calculate the bonus according to the players in range
            float bonus = GetBonusMultiplier(playersInRange.Count, IsClassesBonusValid(playersInRange));
            
            // Calculate the corresponding experience for each player
            var finalXp = (uint)(xp * bonus / playersInRange.Count);
            foreach (var member in playersInRange)
                PlayerMethods.AddExperience(member.Player, finalXp);
            
            PacketSender.OnPartyGainedExperience(finalXp, playersInRange, members);
        }

        private static float GetBonusMultiplier(int playersCount, bool magicalAndNoMagicalBonusValid)
        {
            return playersCount switch
            {
                2 => Constants.TWO_PLAYER_PARTY_BONUS,
                3 => Constants.THREE_PLAYER_PARTY_BONUS,
                4 => magicalAndNoMagicalBonusValid
                    ? Constants.FOUR_PLAYER_PARTY_BONUS + Constants.MAGICAL_NOMAGICAL_PARTY_BONUS
                    : Constants.FOUR_PLAYER_PARTY_BONUS,
                5 => magicalAndNoMagicalBonusValid
                    ? Constants.FIVE_PLAYER_PARTY_BONUS + Constants.MAGICAL_NOMAGICAL_PARTY_BONUS
                    : Constants.FIVE_PLAYER_PARTY_BONUS,
                _ => 1f
            };
        }

        private static bool IsClassesBonusValid(List<PartyMember> players)
        {
            if (players.Count < 4)
                return false;

            bool magicalFound = false, notMagicalFound = false;
            
            foreach (var member in players)
            {
                magicalFound |= member.Player.Class.IsMagical;
                notMagicalFound |= !member.Player.Class.IsMagical;
            }

            return magicalFound && notMagicalFound;
        }

        public static bool ArePlayersInSameParty(Player p1, Player p2)
        {
            return p1.Party is not null && p1.Party == p2.Party;
        }
    }
}