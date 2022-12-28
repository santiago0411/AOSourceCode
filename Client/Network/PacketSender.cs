using System.Collections.Generic;
using AO.Core.Ids;
using AOClient.Core.Utils;
using AOClient.Player;
using AOClient.UI.Main.Talents;
using UnityEngine;

namespace AOClient.Network
{
    /// <summary>Contains all the methods to send packets to the server.</summary>
    public static class PacketSender
    {
        /// <summary>Sends a packet to the server via TCP.</summary>
        /// <param name="packet">The packet to send to the sever.</param>
        private static void SendTcpData(Packet packet)
        {
            packet.WriteLength();
            Client.Instance.Tcp.SendData(packet);
        }

        /// <summary>Sends a packet to the server via UDP.</summary>
        /// <param name="packet">The packet to send to the sever.</param>
        private static void SendUdpData(Packet packet)
        {
            packet.WriteLength();
            Client.Instance.Udp.SendData(packet);
        }

        #region Packets
        public static void WelcomeReceived()
        {
            using var packet = new Packet(ClientPackets.WelcomeReceived);
            packet.Write(Client.Instance.MyId.AsPrimitiveType());
            SendTcpData(packet);
        }

        public static void Login(string accountName, string passwordHash)
        {
            using var packet = new Packet(ClientPackets.Login);
            packet.Write(accountName);
            packet.Write(passwordHash);
            SendTcpData(packet);
        }

        public static void RegisterAccount(string accountName, string passwordHash, string email)
        {
            using var packet = new Packet(ClientPackets.RegisterAccount);
            packet.Write(accountName);
            packet.Write(passwordHash);
            packet.Write(email);
            SendTcpData(packet);
        }

        public static void GetRacesAttributes()
        {
            using var packet = new Packet(ClientPackets.GetRacesAttributes);
            SendTcpData(packet);
        }

        public static void GetCharacters()
        {
            using var packet = new Packet(ClientPackets.GetCharacters);
            SendTcpData(packet);
        }

        public static void CreateCharacter(bool isTemplate, string name, byte @class, byte race, byte headId, byte gender, Dictionary<Skill, UI.Main.SkillUI> skills) //Add city
        {
            using var packet = new Packet(ClientPackets.CreateCharacter);
            packet.Write(isTemplate);
            packet.Write(name);
            packet.Write(@class);
            packet.Write(race);
            packet.Write(headId);
            packet.Write(gender);

            packet.Write((byte)skills.Count);
            foreach (var (skill, skillUI) in skills)
            {
                packet.Write((byte)skill);
                packet.Write(skillUI.Value);
            }

            SendTcpData(packet);
        }

        public static void BeginEnterWorld(CharacterId charId)
        {
            using var packet = new Packet(ClientPackets.EnterWorld);
            packet.Write(charId.AsPrimitiveType());
            SendTcpData(packet);
        }

        public static void PlayerChat(string message)
        {
            using var packet = new Packet(ClientPackets.PlayerChat);
            packet.Write(message);
            SendTcpData(packet);
        }

        public static void PlayerMovementInputs(PlayerMovementInputs inputs)
        {
            using var packet = new Packet(ClientPackets.PlayerMovementInputs);
            packet.Write((byte)inputs);
            SendUdpData(packet);
        }

        public static void PlayerInput(PlayerInput playerInput)
        {
            using var packet = new Packet(ClientPackets.PlayerInput);
            packet.Write((byte)playerInput);
            SendTcpData(packet);
        }

        public static void PlayerItemAction(byte itemSlot, bool used)
        {
            using var packet = new Packet(ClientPackets.PlayerItemAction);
            packet.Write(itemSlot);
            packet.Write(used);
            SendTcpData(packet);
        }

        public static void PlayerDropItem(byte itemSlot, ushort quantity, bool draggedAndDropped, Vector2 position = default)
        {
            using var packet = new Packet(ClientPackets.PlayerDropItem);
            packet.Write(itemSlot);
            packet.Write(quantity);
            packet.Write(draggedAndDropped);

            if (draggedAndDropped)
                packet.Write(position);

            SendTcpData(packet);
        }

        public static void PlayerLeftClick(Vector2 clickPosition, bool doubleClick)
        {
            using var packet = new Packet(ClientPackets.PlayerLeftClick);
            packet.Write(clickPosition);
            packet.Write(doubleClick);
            SendTcpData(packet);
        }


        public static void PlayerSwappedItemSlot(byte originalSlot, byte newSlot)
        {
            using var packet = new Packet(ClientPackets.PlayerSwappedItemSlot);
            packet.Write(originalSlot);
            packet.Write(newSlot);
            SendTcpData(packet);
        }

        public static void NpcTrade(bool buying, byte slot, ushort quantity)
        {
            using var packet = new Packet(ClientPackets.NpcTrade);
            packet.Write(buying);
            packet.Write(slot);
            packet.Write(quantity);
            SendTcpData(packet);
        }

        public static void EndNpcTrade()
        {
            using var packet = new Packet(ClientPackets.EndNpcTrade);
            SendTcpData(packet);
        }

        public static void PlayerSelectedSpell(byte spellSlot)
        {
            using var packet = new Packet(ClientPackets.PlayerSelectedSpell);
            packet.Write(spellSlot);
            SendTcpData(packet);
        }

        public static void PlayerLeftClickRequest(Vector2 position, ClickRequest request)
        {
            using var packet = new Packet(ClientPackets.PlayerLeftClickRequest);
            packet.Write(position);
            packet.Write((byte)request);
            SendTcpData(packet);
        }

        public static void MovePlayerSpell(byte slot, bool up)
        {
            using var packet = new Packet(ClientPackets.MovePlayerSpell);
            packet.Write(slot);
            packet.Write(up);
            SendTcpData(packet);
        }

        public static void SkillsChanged(Dictionary<Skill, byte> skillsChanged)
        {
            using var packet = new Packet(ClientPackets.SkillsChanged);
            packet.Write((byte)skillsChanged.Count);

            foreach (var entry in skillsChanged)
            {
                packet.Write((byte)entry.Key);
                packet.Write(entry.Value);
            }

            SendTcpData(packet);
        }

        public static void DropGold(uint amount)
        {
            using var packet = new Packet(ClientPackets.DropGold);
            packet.Write(amount);
            SendTcpData(packet);
        }

        public static void CraftItem(CraftingProfession profession, ItemId itemId, ushort quantity)
        {
            using var packet = new Packet(ClientPackets.CraftItem);
            packet.Write((byte)profession);
            packet.Write(itemId.AsPrimitiveType());
            packet.Write(quantity);
            SendTcpData(packet);
        }

        public static void CloseCraftingWindow()
        {
            using var packet = new Packet(ClientPackets.CloseCraftingWindow);
            SendTcpData(packet);
        }

        public static void SelectQuest(QuestId questId)
        {
            using var packet = new Packet(ClientPackets.SelectQuest);
            packet.Write(questId.AsPrimitiveType());
            SendTcpData(packet);
        }
    
        public static void SelectQuestItemReward(ItemId itemId)
        {
            using var packet = new Packet(ClientPackets.SelectQuestItemReward);
            packet.Write(itemId.AsPrimitiveType());
            SendTcpData(packet);
        }

        public static void AcceptQuest(QuestId questId)
        {
            using var packet = new Packet(ClientPackets.AcceptQuest);
            packet.Write(questId.AsPrimitiveType());
            SendTcpData(packet);
        }
    
        public static void CompleteQuest(QuestId questId)
        {
            using var packet = new Packet(ClientPackets.CompleteQuest);
            packet.Write(questId.AsPrimitiveType());
            SendTcpData(packet);
        }
    
        public static void CanSkillUpTalent(Profession profession, byte nodeId)
        {
            using var packet = new Packet(ClientPackets.CanSkillUpTalent);
            packet.Write((byte)profession);
            packet.Write(nodeId);
            SendTcpData(packet);
        }

        public static void SkillUpTalents(HashSet<TalentNodeUIBase> nodesToSkillUp)
        {
            using var packet = new Packet(ClientPackets.SkillUpTalents);
            packet.Write((byte)nodesToSkillUp.Count);
            foreach (var node in nodesToSkillUp)
                node.WriteSkillUpTalentToPacket(packet);
            SendTcpData(packet);
        }

        public static void ChangePartyPercentages(List<(ClientId, byte)> playerPercentages)
        {
            using var packet = new Packet(ClientPackets.ChangePartyPercentages);
            packet.Write((byte)playerPercentages.Count);
            foreach (var (playerId, percentage) in playerPercentages)
            {
                packet.Write(playerId.AsPrimitiveType());
                packet.Write(percentage);
            }
            SendTcpData(packet);
        }

        public static void KickPartyMember(ClientId playerClientId)
        {
            using var packet = new Packet(ClientPackets.KickPartyMember);
            packet.Write(playerClientId.AsPrimitiveType());
            SendTcpData(packet);
        }

        public static void SendMail(string recipient, string subject, string body, uint gold, byte[] slotsToSend)
        {
            using var packet = new Packet(ClientPackets.SendMail);
            packet.Write(recipient);
            packet.Write(subject);
            packet.Write(body);
            packet.Write(gold);
            packet.Write((byte)slotsToSend.Length);
            foreach (var slotId in slotsToSend)
                packet.Write(slotId);
            SendTcpData(packet);
        }

        public static void FetchMails()
        {
            using var packet = new Packet(ClientPackets.FetchMails);
            SendTcpData(packet);
        }
        
        public static void CollectMailItem(uint mailId, ItemId itemId)
        {
            using var packet = new Packet(ClientPackets.CollectMailItem);
            packet.Write(mailId);
            packet.Write(itemId.AsPrimitiveType());
            SendTcpData(packet);
        }
        
        public static void DeleteMail(uint mailId)
        {
            using var packet = new Packet(ClientPackets.DeleteMail);
            packet.Write(mailId);
            SendTcpData(packet);
        }
        #endregion
    }
}
