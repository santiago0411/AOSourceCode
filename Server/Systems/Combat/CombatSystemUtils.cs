using AO.Core;
using AO.Core.Utils;
using AO.Npcs;
using AO.Players;
using AO.World;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Combat
{
    public static class CombatSystemUtils
    {
        public static bool ClassWarriorOrPaladin(ClassType classType) => classType is ClassType.Paladin or ClassType.Warrior;

        /// <summary>Returns whether both players are in zone where fighting is allowed.</summary>
        public static bool BothInFightingZone(Player attacker, Player target)
        {
            return attacker.Flags.ZoneType != ZoneType.SafeZone && target.Flags.ZoneType != ZoneType.SafeZone;
        }

        /// <summary>Returns whether both players are in a arena.</summary>
        public static bool BothInArena(Player attacker, Player target)
        {
            return attacker.Flags.ZoneType == ZoneType.Arena && target.Flags.ZoneType == ZoneType.Arena;
        }

        public static bool CanPlayerAttackPlayer(Player attacker, Player target)
        {
            //if (target.IsGameMaster)
               // return false;

            if (attacker.Flags.IsDead)
            {
                PacketSender.SendMultiMessage(attacker.Id, MultiMessage.YouAreDead);
                return false;
            }

            if (target.Flags.IsDead)
            {
                PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CantAttackSpirit);
                return false;
            }

            if (BothInArena(attacker, target))
                return true;

            if (!BothInFightingZone(attacker, target))
            {
                PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CantAttackInSafeZone);
                return false;
            }
            
            if (Party.ArePlayersInSameParty(attacker, target)) // TODO || SameClan()
                return false;

            switch (attacker.Faction)
            {
                case Faction.Imperial when (target.Faction & Constants.CITIZEN_IMPERIAL) == target.Faction:
                    PacketSender.SendMultiMessage(attacker.Id, MultiMessage.ImperialsCantAttackCitizens);
                    return false;
                case Faction.Citizen when (target.Faction & Constants.CITIZEN_IMPERIAL) == target.Faction && attacker.Flags.SafeToggleOn:
                    PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CantAttackCitizenWithSafeOn);
                    return false;
                default:
                    return true;
            }
        }

        public static bool CanPlayerAttackNpc(Player attacker, Npc npc, bool paralyze = false)
        {
            if (attacker.Flags.IsDead)
            {
                PacketSender.SendMultiMessage(attacker.Id, MultiMessage.YouAreDead);
                return false;
            }

            if (!npc.Attackable)
            {
                PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CantAttackThatNpc);
                return false;
            }

            if (!npc.IsHostile)
                return CanAttackNonHostileNpc(attacker, npc);

            if (npc.IsPet)
                return CanAttackPet(attacker, npc.PetOwner);

            //If the npc already has an owner
            if (npc.Flags.CombatOwner is not null)
                return CanAttackNpcInCombat(attacker, npc);

            //If the player is criminal they cannot own an npc
            if (attacker.Faction != Faction.Criminal) 
            {
                //Can only appropriate hostile npcs that are neutral (non factional npcs)
                if (!paralyze && npc.Info.NpcFaction == NpcFaction.Neutral)
                    PlayerMethods.AppropriatedNpc(attacker, npc);
            }

            return true;
        }

        private static bool CanAttackNonHostileNpc(Player attacker, Npc npc)
        {
            switch (npc.Info.NpcFaction)
            {
                case NpcFaction.Neutral:
                    return false;
                case NpcFaction.Chaos when attacker.Faction == Faction.Chaos:
                    PacketSender.SendMultiMessage(attacker.Id, MultiMessage.ChaosCantAttackChaosNpc);
                    return false;
                case NpcFaction.Imperial:
                    switch (attacker.Faction)
                    {
                        case Faction.Imperial:
                            PacketSender.SendMultiMessage(attacker.Id, MultiMessage.ImperialCantAttackImperialNpc);
                            return false;
                        case Faction.Citizen when attacker.Flags.SafeToggleOn:
                            PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CitizenSafeOnCantAttackImperialNpc);
                            return false;
                        case Faction.Citizen:
                            PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CitizenAttackedImperialNpc);
                            PlayerMethods.ChangePlayerFaction(attacker, Faction.Criminal);
                            break;
                    }

                    break;
            }

            return true;           
        }

        private static bool CanAttackPet(Player attacker, Player petOwner)
        {   
            if (attacker == petOwner)
            {
                PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CantAttackOwnPet);
                return false;
            }

            if (petOwner.Faction is Faction.Criminal or Faction.Chaos)
            {
                if (attacker.Faction is Faction.Criminal or Faction.Chaos)
                    return !Party.ArePlayersInSameParty(attacker, petOwner); //TODO && !InSameClan()
            }

            switch (attacker.Faction)
            {
                case Faction.Imperial:
                    PacketSender.SendMultiMessage(attacker.Id, MultiMessage.ImperialCantAttackCitizenPet);
                    return false;
                case Faction.Citizen when attacker.Flags.SafeToggleOn:
                    PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CitizenSafeOnCantAttackCitizenPet);
                    return false;
                case Faction.Citizen:
                    PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CitizenAttackedCitizenPet);
                    PlayerMethods.ChangePlayerFaction(attacker, Faction.Criminal);
                    break;
            }

            return true;
        }

        private static bool CanAttackNpcInCombat(Player attacker, Npc npc)
        {
            Player owner = npc.Flags.CombatOwner;

            //Can attack his own npc
            if (owner == attacker)
            {
                Timers.PlayerLostNpcInterval(attacker, true);
                return true;
            }

            if (Party.ArePlayersInSameParty(attacker, owner)) // TODO && SameClan()
                return true;
            
            if (Timers.PlayerLostNpcInterval(owner))
            {
                PlayerMethods.LostNpc(owner);
                PlayerMethods.AppropriatedNpc(attacker, npc);
                return true;
            }

            if (attacker.Faction == Faction.Criminal || owner.Faction == Faction.Criminal)
                return true;

            switch (attacker.Faction)
            {
                case Faction.Chaos when owner.Faction == Faction.Chaos:
                    PacketSender.SendMultiMessage(attacker.Id, MultiMessage.ChaosCantAttackChaosNpc);
                    return false;
                case Faction.Imperial when (owner.Faction & Constants.CITIZEN_IMPERIAL) == owner.Faction:
                    PacketSender.SendMultiMessage(attacker.Id, MultiMessage.ImperialCantAttackNpcFightingCitizen);
                    return false;
                case Faction.Citizen when (owner.Faction & Constants.CITIZEN_IMPERIAL) == owner.Faction:
                {
                    if (attacker.Flags.SafeToggleOn)
                    {
                        PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CitizenSafeOnCantAttackNpcFightingCitizen);
                        return false;
                    }

                    PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CitizenAttackedNpcFightingCitizen);
                    PlayerMethods.ChangePlayerFaction(attacker, Faction.Criminal);
                    break;
                }
            }

            return true;
        }
    }
}
