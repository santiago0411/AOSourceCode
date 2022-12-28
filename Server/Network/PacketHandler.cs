using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using AO.Core;
using AO.Core.Console;
using AO.Core.Database;
using AO.Core.Ids;
using AO.Core.Logging;
using AO.Players;
using AO.Players.Utils;
using AO.Systems.Mailing;
using AO.Systems.Professions;
using AO.Systems.Questing;
using Client = AO.Network.Server.Client;

namespace AO.Network
{
    /// <summary>Contains all the methods to callback when a client packet is received.</summary>
    public static class PacketHandler
    {
        private static readonly LoggerAdapter log = new(typeof(PacketHandler));

        private static bool CheckPlayerIsValid(Client client, [CallerMemberName] string callerName = "")
        {
            if (client.ClientGameData.Player)
                return true;
            
            log.Warn("Client {0} sent {1} without being logged into a character. Disconnecting.", client.Tcp.RemoteIPEndPoint.ToString(), callerName);
            client.Disconnect();
            return false;
        }
        
        public static void WelcomeReceived(Client fromClient, Packet packet)
        {
            ClientId clientIdCheck;

            try
            {
                clientIdCheck = packet.ReadClientId();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(WelcomeReceived), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            if (fromClient.Id != clientIdCheck)
            {
                log.Warn("ID: {0} has assumed the wrong client ID {1}!", fromClient.Id, clientIdCheck);
                fromClient.Disconnect();
                return;
            }

            fromClient.Authenticated = true;
            log.Debug("{0} connected successfully and is now player {1}.", fromClient.Tcp.RemoteIPEndPoint.ToString(), fromClient.Id);
        }

        public static async void Login(Client fromClient, Packet packet)
        {
            if (fromClient.RunningTask)
                return;

            string accountName, passwordHash;

            try
            {
                accountName = packet.ReadString();
                passwordHash = packet.ReadString();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(Login), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            await TasksManager.LoginTask(fromClient, accountName, passwordHash);
        }

        public static async void RegisterAccount(Client fromClient, Packet packet)
        {
            if (fromClient.RunningTask)
                return;

            string accountName, passwordHash, email;
            try
            {
                accountName = packet.ReadString();
                passwordHash = packet.ReadString();
                email = packet.ReadString();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(RegisterAccount), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            await TasksManager.RegisterAccountTask(fromClient, accountName.ToLower(), passwordHash, email.ToLower());
        }

        public static async void GetCharacters(Client fromClient, Packet _)
        {
            if (fromClient.RunningTask)
                return;

            uint accountId = fromClient.ClientGameData.AccountId;

            if (accountId == 0)
            {
                log.Warn($"Client {fromClient.Tcp.RemoteIPEndPoint} sent {nameof(GetCharacters)} without having logged into an account. Disconnecting.");
                fromClient.Disconnect();
                return;
            }
            
            // Check if this account's characters have already been fetched from db
            if (fromClient.ClientGameData.AccountCharacters.Count == 0)
            {
                // If the haven't fetch them
                fromClient.RunningTask = true;
                var (status, charList) = await DatabaseOperations.FetchCharacters(accountId);
                fromClient.RunningTask = false;
                
                if (status != DatabaseResultStatus.Ok)
                {
                    fromClient.Disconnect();
                    return;
                }
                
                // Avoid allocating new memory if the list is empty
                if (charList.Count != 0)
                    fromClient.ClientGameData.AccountCharacters = charList.ToDictionary(c => c.CharacterId);
            }
            
            PacketSender.GetCharactersReturn(fromClient);
        }

        public static void GetRacesAttributes(Client fromClient, Packet _)
        {
            PacketSender.GetRacesAttributesReturn(fromClient.Id);
        }

        public static async void CreateCharacter(Client fromClient, Packet packet)
        {
            if (fromClient.RunningTask)
                return;

            bool isTemplate;
            string name;
            byte @class, race, headId, gender;
            var skills = new Dictionary<Skill, byte>();

            try
            {
                isTemplate = packet.ReadBool();
                name = packet.ReadString();
                @class = packet.ReadByte();
                race = packet.ReadByte();
                headId = packet.ReadByte();
                gender = packet.ReadByte();

                var skillsCount = packet.ReadByte();

                for (byte i = 0; i < skillsCount; i++)
                {
                    var skill = (Skill)packet.ReadByte();
                    byte value = packet.ReadByte();
                    skills.Add(skill, value);
                }
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(CreateCharacter), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            var spec = new CharacterCreationSpec
            {
                Client = fromClient,
                CharacterName = name,
                Class = @class,
                Race = race,
                HeadId = headId,
                Gender = gender,
                Skills = skills,
                IsTemplate = isTemplate
            };
            
            await TasksManager.CreateCharacterTask(spec);
        }

        public static async void BeginEnterWorld(Client fromClient, Packet packet)
        {
            if (!DatabaseManager.DatabaseActive)
            {
                fromClient.Disconnect();
                return;
            }

            if (fromClient.RunningTask)
                return;

            CharacterId charId;

            try
            {
                charId = packet.ReadCharacterId();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(BeginEnterWorld), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            if (!fromClient.ClientGameData.AccountCharacters.TryGetValue(charId, out var characterInfo))
            {
                //Edited packet disconnect player TODO ban
                log.Warn("Client {0} tried to connect to a character that isn't theirs. Disconnecting.", fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }
            
            // If this character is being saved do not logging until it's finished saving
            if (CharacterManager.IsSavingPlayer(charId))
                return;
            
            try
            {
                await TasksManager.SendPlayerIntoGameTask(fromClient, characterInfo);
            }
            catch (Exception ex)
            {
                log.Error("Error sending new player into game. {0}\n{1}", ex.Message, ex.StackTrace);
                PacketSender.PlayerDisconnected(fromClient.Id);
            }
        }

        public static void PlayerChat(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            string message;

            try
            {
                message = packet.ReadString();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(PlayerChat), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            ConsoleLogic.ProcessCommand(fromClient, message);
        }

        public static void PlayerMovementInputs(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            PlayerMovementInputs inputs;

            try
            {
                inputs = (PlayerMovementInputs)packet.ReadByte();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(PlayerMovementInputs), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }
            
            fromClient.ClientGameData.Player.MovementInputs = inputs;
        }

        public static void PlayerInput(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            PlayerInput playerInput;

            try
            {
                playerInput = (PlayerInput)packet.ReadByte();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(PlayerInput), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.ClientGameData.Player.HandlePlayerExtraInput(playerInput);
        }

        public static void PlayerItemAction(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            byte itemSlot;
            bool used;

            try
            {
                itemSlot = packet.ReadByte();
                used = packet.ReadBool();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(PlayerItemAction), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            if (used)
                fromClient.ClientGameData.Player.Inventory.UseItem(itemSlot);
            else
                fromClient.ClientGameData.Player.Inventory.EquipItem(itemSlot);
        }

        public static void PlayerDropItem(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            byte itemSlot;
            ushort quantity;
            bool draggedAndDropped;
            Vector2 position = fromClient.ClientGameData.Player.transform.position;

            try
            {
                itemSlot = packet.ReadByte();
                quantity = packet.ReadUShort();
                draggedAndDropped = packet.ReadBool();
                if (draggedAndDropped)
                    position = packet.ReadVector2();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(PlayerDropItem), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.ClientGameData.Player.Inventory.DropItem(itemSlot, quantity, draggedAndDropped, position);
        }

        public static void PlayerLeftClick(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            Vector2 clickPosition;
            bool doubleClick;

            try
            {
                clickPosition = packet.ReadVector2();
                doubleClick = packet.ReadBool();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(PlayerLeftClick), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.ClientGameData.Player.HandleLeftClick(clickPosition, doubleClick);
        }

        public static void PlayerSwappedItemSlot(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            byte originalSlot, newSlot;

            try
            {
                originalSlot = packet.ReadByte();
                newSlot = packet.ReadByte();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(PlayerSwappedItemSlot), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.ClientGameData.Player.Inventory.SwapItemSlot(originalSlot, newSlot);
        }

        public static void NpcTrade(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            Player player = fromClient.ClientGameData.Player;

            if (!player.Flags.InteractingWithNpc || !player.Flags.InteractingWithNpc.InteractingWith.Contains(player.Id))
            {
                //TODO ban
                log.Warn("Client {0} tried to trade with an npc that they weren't trading with. Disconnecting.", fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            bool buying;
            byte slot;
            ushort quantity;

            try
            {
                buying = packet.ReadBool();
                slot = packet.ReadByte();
                quantity = packet.ReadUShort();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(NpcTrade), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }
            
            if (buying)
                player.Flags.InteractingWithNpc.SellToPlayer(player, slot, quantity);
            else
                player.Flags.InteractingWithNpc.BuyFromPlayer(player, slot, quantity);
        }

        public static void EndNpcTrade(Client fromClient, Packet _)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            Player player = fromClient.ClientGameData.Player;
            if (player.Flags.InteractingWithNpc)
                player.Flags.InteractingWithNpc.EndInteraction(player);
        }

        public static void PlayerSelectedSpell(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            byte spellSlot;

            try
            {
                spellSlot = packet.ReadByte();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(PlayerSelectedSpell), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.ClientGameData.Player.SelectSpell(spellSlot);
        }

        public static void PlayerLeftClickRequest(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            Vector2 clickPosition;
            ClickRequest request;

            try
            {
                clickPosition = packet.ReadVector2();
                request = (ClickRequest)packet.ReadByte();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(PlayerLeftClickRequest), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.ClientGameData.Player.HandleLeftClickRequest(clickPosition, request);
        }

        public static void MovePlayerSpell(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            byte spellSlot;
            bool up;

            try
            {
                spellSlot = packet.ReadByte();
                up = packet.ReadBool();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(MovePlayerSpell), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.ClientGameData.Player.MoveSpell(spellSlot, up);
        }

        public static void SkillsChanged(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;

            Dictionary<Skill, byte> skillsChanged;

            try
            {
                var count = packet.ReadByte();
                skillsChanged = new Dictionary<Skill, byte>(count);

                for (byte i = 0; i < count; i++)
                    skillsChanged.Add((Skill)packet.ReadByte(), packet.ReadByte());
            }
            catch (ArgumentException) //Check argument exception to make sure the same key wasn't sent twice
            {
                //Ban the mofo
                log.Warn("Client {0}, with player name {1} sent an edited packet to try to assign skills.", fromClient.Tcp.RemoteIPEndPoint.ToString(), fromClient.ClientGameData.Player.Username);
                fromClient.Disconnect();
                return;
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(SkillsChanged), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.ClientGameData.Player.ChangeSkills(skillsChanged);
        }

        public static void DropGold(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            uint amount;

            try
            {
                amount = packet.ReadUInt();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(DropGold), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.ClientGameData.Player.Inventory.DropGold(amount);
        }

        public static void CraftItem(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            CraftingProfession profession;
            ItemId itemId;
            ushort quantity;

            try
            {
                profession = (CraftingProfession)packet.ReadByte();
                itemId = packet.ReadItemId();
                quantity = packet.ReadUShort();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(CraftItem), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            if (quantity <= 0) 
                return;
            Player player = fromClient.ClientGameData.Player;

            switch (profession)
            {
                case CraftingProfession.Blacksmithing:
                    CraftingProfessions.TryBlacksmithing(player, itemId, quantity);
                    break;
                case CraftingProfession.Woodworking:
                    CraftingProfessions.TryWoodworking(player, itemId, quantity);
                    break;
                case CraftingProfession.Tailoring:
                    CraftingProfessions.TryTailoring(player, itemId, quantity);
                    break;
                default:
                    log.Warn("Client {0} sent an invalid profession id. Disconnecting.", fromClient.Tcp.RemoteIPEndPoint.ToString());
                    fromClient.Disconnect();
                    break;
            }
        }

        public static void CloseCraftingWindow(Client fromClient, Packet _)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            fromClient.ClientGameData.Player.Flags.IsWorking = false;
        }
        
        public static void SelectQuest(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            QuestId selectedQuestId;

            try
            {
                selectedQuestId = packet.ReadQuestId();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(SelectQuest), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.ClientGameData.Player.Flags.SelectedQuestId = selectedQuestId;
        }
        
        public static void SelectQuestItemReward(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            ItemId selectedItemId;

            try
            {
                selectedItemId = packet.ReadItemId();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(SelectQuestItemReward), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.ClientGameData.Player.Flags.SelectedItemRewardId = selectedItemId;
        }

        public static void AcceptQuest(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            QuestId questId;
            
            try
            {
                questId = packet.ReadQuestId();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(AcceptQuest), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            Player player = fromClient.ClientGameData.Player;
            QuestManager.AcceptQuestFromNpc(questId, player);
        }
        
        public static void CompleteQuest(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            QuestId questId;
            
            try
            {
                questId = packet.ReadQuestId();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(CompleteQuest), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            Player player = fromClient.ClientGameData.Player;
            QuestManager.TryCompleteQuest(questId, player);
        }
        
        public static void CanSkillUpTalent(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;

            var player = fromClient.ClientGameData.Player;
            if (player.Class.ClassType != ClassType.Worker)
                return;
            
            Profession profession;
            byte nodeId;

            try
            {
                profession = (Profession)packet.ReadByte();
                nodeId = packet.ReadByte();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(CanSkillUpTalent), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            player.WorkerTalentTrees.CheckCanSkillUpTalent(profession, nodeId);
        }
        
        public static void SkillUpTalents(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            var player = fromClient.ClientGameData.Player;
            if (player.Class.ClassType != ClassType.Worker)
                return;

            try
            {
                var workerTalents = player.WorkerTalentTrees;
                byte talentsCount = packet.ReadByte();
                for (var i = 0; i < talentsCount; i++)
                {
                    var profession = (Profession)packet.ReadByte();
                    byte nodeId = packet.ReadByte();
                    byte pointsToAdd = packet.ReadByte();
                    while (pointsToAdd-- > 0)
                        if (workerTalents.SkillUpTalent(profession, nodeId) == Players.Talents.CanSkillUpTalent.InvalidId)
                        {
                            log.Warn("Invalid id detected while skilling up talents");
                            return;
                        }
                }
                
                PacketSender.PlayerLeveledUpTalents(player);
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(SkillUpTalents), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
            }
        }

        public static void ChangePartyPercentages(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;

            var player = fromClient.ClientGameData.Player; 
            var party = player.Party;
            if (party is null || party.Leader.Id != player.Id)
            {
                log.Warn("Player {0} tried to change party percentages for a party that doesn't exist or the player isn't the leader.", player.Username);
                // TODO prob ban
                return;
            }
            
            Dictionary<ClientId, byte> playerPercentages;

            try
            {
                var playerCount = packet.ReadByte();
                playerPercentages = new Dictionary<ClientId, byte>(playerCount);
                for (var i = 0; i < playerCount; i++)
                    playerPercentages.Add(packet.ReadClientId(), packet.ReadByte());
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(ChangePartyPercentages), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }
            
            party.TryChangePercentages(playerPercentages);
        }

        public static void KickPartyMember(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            var player = fromClient.ClientGameData.Player; 
            var party = player.Party;
            if (party is null || party.Leader.Id != player.Id)
            {
                log.Warn("Player {0} tried to kicked a party member for a party that doesn't exist or the player isn't the leader.", player.Username);
                // TODO prob ban
                return;
            }

            ClientId playerClientId;

            try
            {
                playerClientId = packet.ReadClientId();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(KickPartyMember), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }
            
            party.RemoveMember(playerClientId, true);
        }

        public static async void SendMail(Client fromClient, Packet packet)
        {
            if (fromClient.RunningTask)
                return;
            
            if (!CheckPlayerIsValid(fromClient))
                return;

            string recipientCharName; 
            string subject, body;
            uint gold;
            var slotsToSend = new HashSet<byte>();
            
            try
            {
                recipientCharName = packet.ReadString();
                subject = packet.ReadString();
                body = packet.ReadString();
                gold = packet.ReadUInt();
                byte slotsCount = packet.ReadByte();
                for (var i = 0; i < slotsCount; i++)
                    slotsToSend.Add(packet.ReadByte());
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(SendMail), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            fromClient.RunningTask = true;
            await MailManager.TryCreateAndSendMail(fromClient.ClientGameData.Player, recipientCharName, subject, body, gold, slotsToSend);
            fromClient.RunningTask = false;
        }

        public static async void FetchMails(Client fromClient, Packet _)
        {
            if (fromClient.RunningTask)
                return;

            if (!CheckPlayerIsValid(fromClient))
                return;
            
            // Do this check here to avoid starting task
            if (fromClient.ClientGameData.Player.Flags.CachedMails.Count > 0)
                return;
            
            fromClient.RunningTask = true;
            await MailManager.FetchMails(fromClient.ClientGameData.Player);
            fromClient.RunningTask = false;
        }

        public static async void CollectMailItem(Client fromClient, Packet packet)
        {
            if (fromClient.RunningTask)
                return;

            if (!CheckPlayerIsValid(fromClient))
                return;

            uint mailId;
            ItemId itemId;
            
            try
            {
                mailId = packet.ReadUInt();
                itemId = packet.ReadItemId();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(CollectMailItem), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }
            
            fromClient.RunningTask = true;
            await MailManager.CollectMailItem(fromClient.ClientGameData.Player, mailId, itemId);
            fromClient.RunningTask = false;
        }

        public static void DeleteMail(Client fromClient, Packet packet)
        {
            if (!CheckPlayerIsValid(fromClient))
                return;
            
            uint mailId;
            try
            {
                mailId = packet.ReadUInt();
            }
            catch (Exception)
            {
                log.Warn("Error reading packet data in {0} sent by client {1}. Disconnecting.", nameof(DeleteMail), fromClient.Tcp.RemoteIPEndPoint.ToString());
                fromClient.Disconnect();
                return;
            }

            if (!fromClient.ClientGameData.Player.Flags.CachedMails.TryGetValue(mailId, out Mail mail))
                return; // TODO ban tried to delete a mail that wasn't theirs

            mail.ShouldBeDeleted = true;
        }
    }
}