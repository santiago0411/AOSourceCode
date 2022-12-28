using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AO.Core.Ids;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Npcs;
using AOClient.Player;
using AOClient.Player.Utils;
using AOClient.UI;
using UnityEngine;

namespace AOClient.Network
{
    /// <summary>Contains all the methods to callback when a server packet is received.</summary>
    public static class PacketHandler
    {
        public static void Welcome(Packet packet)
        {
            Client.Instance.MyId = packet.ReadClientId();
            PacketSender.WelcomeReceived();
        }

        public static void LoginReturn(Packet packet)
        {
            LoginRegisterMessage msgId = (LoginRegisterMessage)packet.ReadByte();
            bool loggedIn = msgId == LoginRegisterMessage.LoginOk;
            string message;

            switch (msgId)
            {
                case LoginRegisterMessage.LoginOk:
                    message = string.Empty;
                    break;
                case LoginRegisterMessage.AccountAlreadyLoggedIn:
                    message = "La cuenta ya se encuentra conectada";
                    break;
                case LoginRegisterMessage.InvalidAccountOrPassword:
                    message = "Usuario o contraseña invalida";
                    break;
                default:
                    message = "No se puede conectar en este momento";
                    break;
            }

            if (loggedIn)
            {
                Client.Instance.Udp.Connect(((IPEndPoint)Client.Instance.Tcp.Socket.Client.LocalEndPoint).Port);
                SceneLoader.Instance.LoadCharacterScreenScene();
            }
            else
            {
                UIManager.LoginRegister.ShowPopupWindow(message);
            }
        }

        public static void RegisterAccountReturn(Packet packet)
        {
            LoginRegisterMessage msgId = (LoginRegisterMessage)packet.ReadByte();
            bool registered = msgId == LoginRegisterMessage.RegisterOk;
            string message;

            switch (msgId)
            {
                case LoginRegisterMessage.RegisterOk:
                    message = "Registrado exitosamente";
                    break;
                case LoginRegisterMessage.AccountAlreadyExists:
                    message = "Ya existe una cuenta con ese nombre";
                    break;
                case LoginRegisterMessage.EmailAlreadyUsed:
                    message = "El email ya esta en uso";
                    break;
                default:
                    message = "No se puede registrar la cuenta en este momento";
                    break;
            }

            UIManager.LoginRegister.Registered = registered;
            UIManager.LoginRegister.ShowPopupWindow(message);
        }

        public static void GetRacesAttributesReturn(Packet packet)
        {
            byte length = packet.ReadByte();
            var dic = new Dictionary<RaceType, Dictionary<PlayerAttribute, sbyte>>(length);

            for (int i = 0; i < length; i++)
            {
                RaceType race = (RaceType)packet.ReadByte();

                byte length2 = packet.ReadByte();

                var dic2 = new Dictionary<PlayerAttribute, sbyte>(length2);

                for (int j = 0; j < length2; j++)
                {
                    PlayerAttribute att = (PlayerAttribute)packet.ReadByte();
                    sbyte value = (sbyte)packet.ReadByte();
                    dic2.Add(att, value);
                }

                dic.Add(race, dic2);
            }

            CharacterCreation.RacesAttValues = dic;
        }

        public static void GetCharactersReturn(Packet packet)
        {
            byte count = packet.ReadByte();

            Client.Instance.StartCoroutine(LoadCharacters());

            System.Collections.IEnumerator LoadCharacters()
            {
                while (UIManager.CharacterScreen is null)
                    yield return null;

                if (count == 0)
                {
                    UIManager.CharacterScreen.LoadCharacters(null);
                    yield break;
                }

                var characters = new Dictionary<CharacterId, string>(count);
                for (int i = 0; i < count; i++)
                {
                    CharacterId charId = packet.ReadCharacterId();
                    string name = packet.ReadString();
                    characters.Add(charId, name);
                }

                UIManager.CharacterScreen.LoadCharacters(characters);
            }
        }

        public static void CreateCharacterReturn(Packet packet)
        {
            CreateCharacterMessage msg = (CreateCharacterMessage)packet.ReadByte();
            string message;

            switch (msg)
            {
                case CreateCharacterMessage.Ok:
                    CharacterId newCharId = packet.ReadCharacterId();
                    SceneLoader.Instance.LoadMainScene(newCharId, Scene.Main);
                    return;
                case CreateCharacterMessage.InvalidName:
                    message = "El nombre ingresado es invalido";
                    break;
                case CreateCharacterMessage.NameAlreadyInUse:
                    message = "Debes asignar todos tus skills";
                    break;
                case CreateCharacterMessage.NotAllSkillsAreAssigned:
                    message = "Ya existe un personaje con ese nombre";
                    break;
                default:
                    message = "No se puede crear el personaje en este momento";
                    break;
            }

            UIManager.CharacterCreation.Created = false;
            UIManager.CharacterCreation.ShowPopupWindow(message);
        }

        public static void SpawnPlayer(Packet packet)
        {
            var info = new SpawnPlayerInfo
            (
                packet.ReadClientId(), // ClientId
                packet.ReadShort(), // MapNumber
                packet.ReadString(), // Username
                packet.ReadString(), // Description
                packet.ReadByte(), // Class
                packet.ReadByte(), // Race
                packet.ReadByte(), // Gender
                packet.ReadByte(), // Faction
                packet.ReadBool(), // IsGm
                packet.ReadByte() // HeadId
            );
            
            GameManager.Instance.SpawnPlayer(ref info);
        }

        public static void PlayerMaxResources(Packet packet)
        {
            int maxHealth = packet.ReadInt();
            ushort maxMana = packet.ReadUShort();
            ushort maxStamina = packet.ReadUShort();
            ushort maxHunger = packet.ReadUShort();
            ushort maxThirst = packet.ReadUShort();

            PlayerManager.SetMaxResources(maxHealth, maxMana, maxStamina, maxHunger, maxThirst);
        }

        public static void PlayerPrivateInfo(Packet packet)
        {
            byte @class = packet.ReadByte();
            GameManager.Instance.LocalPlayer.LoadPlayerPrivateInfo(@class);
        }

        public static void PlayerSkills(Packet packet)
        {
            byte skillsCount = packet.ReadByte();
            var skillsValues = new Dictionary<Skill, byte>(skillsCount);

            for (byte i = 0; i < skillsCount; i++)
            {
                Skill skill = (Skill)packet.ReadByte();
                byte value = packet.ReadByte();
                skillsValues.Add(skill, value);
            }

            UIManager.GameUI.StatsWindow.SetSkills(skillsValues);
        }

        public static void ChatBroadcast(Packet packet)
        {
            ClientId senderId = packet.ReadClientId();
            string message = packet.ReadString();
            GameManager.Instance.GetPlayer(senderId).SetChatBubbleText(message, true);
        }

        public static void PlayerPosition(Packet packet)
        {
            ClientId id = packet.ReadClientId();
            Vector2 position = packet.ReadVector2();
            var heading = (Heading)packet.ReadByte();

            if (GameManager.Instance is null || !GameManager.Instance.TryGetPlayer(id, out var player)) 
                return;
        
            if (!player.Equals(null) && !player.gameObject.activeSelf)
                return;

            //Vector3 realPosition = new Vector3(position.x, 3f, position.y);
            bool moving = position != (Vector2)player.transform.position;
            player.PlayAnimationOnce(heading, moving);
            player.transform.position = position;
        }

        public static void PlayerRangeChanged(Packet packet)
        {
            ClientId id = packet.ReadClientId();
            bool inRange = packet.ReadBool();

            if (GameManager.Instance.TryGetPlayer(id, out var player)) 
                player.gameObject.SetActive(inRange);
        }

        public static void PlayerUpdatePosition(Packet packet)
        {
            int xPos = packet.ReadInt();
            int yPos = packet.ReadInt();
            GameManager.Instance.LocalPlayer.UpdatePosition(new Vector2(xPos, yPos));
        }

        public static void PlayerDisconnected(Packet packet)
        {
            ClientId id = packet.ReadClientId();

            if (Client.Instance.MyId == id)
            {
                SceneLoader.Instance.LoadCharacterScreenScene();
            }

            if (GameManager.Instance.TryGetPlayer(id, out var player))
            {
                UnityEngine.Object.Destroy(player.gameObject);
                GameManager.Instance.RemovePlayer(id);
            }
        }

        public static void PlayerResources(Packet packet)
        {
            int health = packet.ReadInt();
            ushort mana = packet.ReadUShort();
            ushort stamina = packet.ReadUShort();
            ushort hunger = packet.ReadUShort();
            ushort thirst = packet.ReadUShort();

            PlayerManager.UpdateResources(health, mana, stamina, hunger, thirst);
        }

        public static void PlayerIndividualResource(Packet packet)
        {
            Resource resource = (Resource)packet.ReadByte();

            switch (resource)
            {
                case Resource.Health:
                    UIManager.GameUI.PlayerResources.SetCurrentHpText(packet.ReadInt());
                    break;
                case Resource.Mana:
                    UIManager.GameUI.PlayerResources.SetCurrentManaText(packet.ReadUShort());
                    break;
                case Resource.Stamina:
                    UIManager.GameUI.PlayerResources.SetCurrentStaminaText(packet.ReadUShort());
                    break;
                case Resource.HungerAndThirst:
                    UIManager.GameUI.PlayerResources.SetCurrentHungerText(packet.ReadUShort());
                    UIManager.GameUI.PlayerResources.SetCurrentThirstText(packet.ReadUShort());
                    break;
            }
        }

        public static void PlayerStats(Packet packet)
        {
            bool aux = packet.ReadBool();

            if (!aux)
            {
                PlayerStat playerStat = (PlayerStat)packet.ReadByte();
                uint value = packet.ReadUInt();
                UIManager.GameUI.StatsWindow.SetStat(playerStat, value);
            }
            else
            {
                byte level = packet.ReadByte();
                uint currentXp = packet.ReadUInt();
                uint maxXp = packet.ReadUInt();
                ushort assignableSkills = packet.ReadUShort();
                byte availableTalentPoints = packet.ReadByte();

                byte statsCount = packet.ReadByte();
                List<uint> stats = new List<uint>(statsCount);
                for (byte i = 0; i < statsCount; i++)
                {
                    uint value = packet.ReadUInt();
                    stats.Add(value);
                }

                UIManager.GameUI.StatsWindow.SetStats(stats);
                UIManager.GameUI.StatsWindow.AssignableSkills = assignableSkills;
                UIManager.GameUI.StatsWindow.AvailableTalentPoints = availableTalentPoints;
                UIManager.GameUI.PlayerInfo.SetLevelAndXp(level.ToString(), currentXp, maxXp);
            }
        }

        public static void PlayerTalentPoints(Packet packet)
        {
            var workerTalentsUI = UIManager.GameUI.StatsWindow.WorkerTalents;

            foreach (var profession in (Profession[])Enum.GetValues(typeof(Profession)))
            {
                var nodesCount = packet.ReadByte();
                for (var i = 0; i < nodesCount; i++)
                {
                    byte nodeId = packet.ReadByte();
                    byte currentPoints = packet.ReadByte();
                    workerTalentsUI.SetNodeCurrentPoints(profession, nodeId, currentPoints);
                }
            }
        }

        public static void PlayerLeveledUpTalents(Packet packet)
        {
            var workerTalentsUI = UIManager.GameUI.StatsWindow.WorkerTalents;
            
            while (packet.UnreadLength() > 0)
            {
                var profession = (Profession)packet.ReadByte();
                byte talentsCount = packet.ReadByte();
                for (var i = 0; i < talentsCount; i++)
                {
                    byte talentId = packet.ReadByte();
                    byte currentPoints = packet.ReadByte();
                    workerTalentsUI.SetNodeCurrentPoints(profession, talentId, currentPoints);
                }
            }
        }

        public static void PlayerGainedXp(Packet packet)
        {
            uint gainedXp = packet.ReadUInt();
            UIManager.GameUI.PlayerInfo.AddCurrentXp(gainedXp);
            UIManager.GameUI.Console.WriteLine($"¡Has ganado {gainedXp} puntos de experiencia!", Core.Utils.ConsoleMessage.Combat);
        }

        public static void PlayerAttributes(Packet packet)
        {
            byte attCount = packet.ReadByte();
            List<byte> attributes = new List<byte>(attCount);

            for (short i = 0; i < attCount; i++)
            {
                byte value = packet.ReadByte();
                attributes.Add(value);
            }

            UIManager.GameUI.StatsWindow.SetAttributes(attributes);
        }

        public static void PlayerGold(Packet packet)
        {
            uint gold = packet.ReadUInt();
            GameManager.Instance.LocalPlayer.UpdateGold(gold);
        }

        public static void ClickRequest(Packet packet)
        {
            var clickRequest = (ClickRequest)packet.ReadByte();
            GameManager.Instance.LocalPlayer.SetClickRequest(clickRequest);
        }

        public static void WorldItemSpawned(Packet packet)
        {
            int instanceId = packet.ReadInt();
            ItemId itemId = packet.ReadItemId();
            short mapNumber = packet.ReadShort();
            Vector2 mapPosition = packet.ReadVector2();
            GameManager.Instance.CreateWorldItem(instanceId, itemId, mapNumber, mapPosition);
        }

        public static void WorldItemDestroyed(Packet packet)
        {
            int instanceId = packet.ReadInt();
            GameManager.Instance.DestroyWorldItem(instanceId);
        }

        public static void PlayerInventory(Packet packet)
        {
            byte slot = packet.ReadByte();
            ItemId itemId = packet.ReadItemId();
            ushort quantity = packet.ReadUShort();
            uint sellingPrice = packet.ReadUInt();
            GameManager.Instance.LocalPlayer.AddItemToInventory(slot, itemId, quantity, sellingPrice);
        }

        public static void PlayerUpdateInventory(Packet packet)
        {
            byte slot = packet.ReadByte();
            ushort quantity = packet.ReadUShort();
            GameManager.Instance.LocalPlayer.UpdateInventory(slot, quantity);
        }

        public static void PlayerSwapInventorySlots(Packet packet)
        {
            byte slotA = packet.ReadByte();
            byte slotB = packet.ReadByte();
            GameManager.Instance.LocalPlayer.SwapInventorySlots(slotA, slotB);
        }

        public static void PlayerEquippedItems(Packet packet)
        {
            var player = GameManager.Instance.GetPlayer(packet.ReadClientId());
            player.ClearEquippedItems();
            
            byte itemCount = packet.ReadByte();
            for (var i = 0; i < itemCount; i++)
            {
                byte slot = packet.ReadByte();
                var item = GameManager.Instance.GetItem(packet.ReadItemId());
                player.EquipItem(slot, item, true);
            }
        }
        
        public static void OnPlayerItemEquippedChanged(Packet packet)
        {
            ClientId equippedPlayerId = packet.ReadClientId();
            byte slot = packet.ReadByte();
            var item = GameManager.Instance.GetItem(packet.ReadItemId());
            bool equipped = packet.ReadBool();
            GameManager.Instance.GetPlayer(equippedPlayerId).EquipItem(slot, item, equipped);
        }

        public static void EndEnterWorld(Packet _)
        {
            SceneLoader.Instance.ReadyToShowMain = true;
        }

        public static void ConsoleMessage(Packet packet)
        {
            string message = packet.ReadString();
            byte type = packet.ReadByte();

            UIManager.GameUI.Console.WriteLine(message, (ConsoleMessage)type);
        }

        public static void UpdatePlayerSpells(Packet packet)
        {
            byte spellSlot = packet.ReadByte();
            SpellId spellId = packet.ReadSpellId();
            GameManager.Instance.LocalPlayer.UpdateSpells(GameManager.Instance.GetSpell(spellId), spellSlot);
        }

        public static void MovePlayerSpell(Packet packet)
        {
            byte slotOne = packet.ReadByte();
            byte slotTwo = packet.ReadByte();
            GameManager.Instance.LocalPlayer.MoveSpells(slotOne, slotTwo);
        }

        public static void SayMagicWords(Packet packet)
        {
            ClientId playerId = packet.ReadClientId();
            SpellId spellId = packet.ReadSpellId();
            GameManager.Instance.GetPlayer(playerId).SetChatBubbleText(GameManager.Instance.GetSpell(spellId).MagicWords, false);
        }

        public static void NpcSpawn(Packet packet)
        {
            NpcId npcId = packet.ReadNpcId();
            int instanceId = packet.ReadInt();
            short mapNumber = packet.ReadShort();
            Vector2 position = packet.ReadVector2();

            GameManager.Instance.CreateNpc(npcId, instanceId, mapNumber, position);
        }

        public static void NpcPosition(Packet packet)
        {
            int instanceId = packet.ReadInt();
            Vector2 position = packet.ReadVector2();

            if (GameManager.Instance.NpcsPool.TryFindObject(instanceId, out Npc npc))
            {
                if (!npc.gameObject.activeSelf)
                    return;
                
                npc.PlayAnimationOnce(position != (Vector2)npc.transform.position);
                npc.transform.position = position;
            }
        }

        public static void NpcRangeChanged(Packet packet)
        {
            int instanceId = packet.ReadInt();
            bool inRange = packet.ReadBool();

            if (GameManager.Instance.NpcsPool.TryFindObject(instanceId, out Npc npc))
                npc.gameObject.SetActive(inRange);
        }

        public static void NpcFacing(Packet packet)
        {
            int instanceId = packet.ReadInt();
            Vector2 difference = packet.ReadVector2();

            if (GameManager.Instance.NpcsPool.TryFindObject(instanceId, out Npc npc))
            {
                npc.CheckFacing(difference);
            }
        }

        public static void NpcTrade(Packet packet)
        {
            var inventory = new NpcInventory[Constants.NPC_INV_SPACE];
            bool hasQuests = GameManager.Instance.GetNpcInfo(packet.ReadNpcId()).AvailableQuests.Count > 0;
            byte length = packet.ReadByte();
            for (int i = 0; i < length; i++)
            {
                byte slot = packet.ReadByte();
                ItemId itemId = packet.ReadItemId();
                ushort quantity = packet.ReadUShort();
                int price = packet.ReadInt();
                inventory[slot] = new NpcInventory(slot, itemId, quantity, price);
            }

            UIManager.GameUI.NpcTradeWindow.Open(hasQuests);
            UIManager.GameUI.NpcTradeWindow.LoadNpcInventory(inventory);
        }

        public static void NpcInventoryUpdate(Packet packet)
        {
            byte slot = packet.ReadByte();
            ushort quantity = packet.ReadUShort();
            ItemId itemId = 0;
            int price = 0;

            if (UIManager.GameUI.NpcTradeWindow.NpcInventorySlotIsNull(slot))
            {
                itemId = packet.ReadItemId();
                price = packet.ReadInt();
            }

            UIManager.GameUI.NpcTradeWindow.UpdateNpcInventory(slot, quantity, itemId, price);
        }

        public static void NpcDespawned(Packet packet)
        {
            int instanceId = packet.ReadInt();
            GameManager.Instance.NpcsPool.FindObject(instanceId).ResetPoolObject();
        }

        public static void UpdatePlayerStatus(Packet packet)
        {
            ClientId playerClientId = packet.ReadClientId();
            PlayerStatus playerStatus = (PlayerStatus)packet.ReadByte();

            switch (playerStatus)
            {
                case PlayerStatus.Died:
                    GameManager.Instance.GetPlayer(playerClientId).Die();
                    break;
                case PlayerStatus.Revived:
                    GameManager.Instance.GetPlayer(playerClientId).Revive();
                    break;
                case PlayerStatus.UsedBoat:
                    GameManager.Instance.GetPlayer(playerClientId).UseBoat(packet.ReadBool());
                    break;
                case PlayerStatus.Mounted:
                    break;
                case PlayerStatus.ChangedFaction:
                    GameManager.Instance.GetPlayer(playerClientId).SetFaction((Faction)packet.ReadByte(), packet.ReadBool());
                    break;
                case PlayerStatus.ChangedGuildName:
                    break;
                case PlayerStatus.Meditate:
                    GameManager.Instance.GetPlayer(playerClientId).Meditate();
                    break;
            }   
        }

        public static void MultiMessage(Packet packet)
        {
            MultiMessageWriter.WriteMultiMessage(packet);
        }

        public static void PlayerInputReturn(Packet packet)
        {
            PlayerInput playerInput = (PlayerInput)packet.ReadByte();

            switch (playerInput)
            {
                case PlayerInput.SafeToggle:
                    UIManager.GameUI.Menu.ToggleSafe(packet.ReadBool());
                    break;
                case PlayerInput.RessToggle:
                    UIManager.GameUI.Menu.ToggleRess(packet.ReadBool());
                    break;
            }
        }

        public static void CreateParticle(Packet packet)
        {
            ushort particleId = packet.ReadUShort();
            SpellTarget target = (SpellTarget)packet.ReadByte();
            Transform transform;
            Vector3 position;

            switch (target)
            {
                case SpellTarget.User:
                {
                    ClientId playerClientId = packet.ReadClientId();
                    transform = GameManager.Instance.GetPlayer(playerClientId).transform;
                    var transformPosition = transform.position;
                    position = new Vector2(transformPosition.x, transformPosition.y + 0.5f);
                    GameManager.Instance.CreateParticle(particleId, position, transform);
                    break;
                }
                case SpellTarget.Npc:
                {
                    int npcInstanceId = packet.ReadInt();
                    transform = GameManager.Instance.NpcsPool.FindObject(npcInstanceId).transform;
                    var transformPosition = transform.position;
                    position = new Vector2(transformPosition.x, transformPosition.y + 0.5f);
                    GameManager.Instance.CreateParticle(particleId, position, transform);
                    break;
                }
                case SpellTarget.Terrain:
                {
                    short mapNumber = packet.ReadShort();
                    Vector2 pos = packet.ReadVector2();
                    position = new Vector2(pos.x, pos.y);
                    GameManager.Instance.CreateParticle(particleId, position,
                        GameManager.Instance.GetWorldMap(mapNumber).transform);
                    break;
                }
            }
        }

        public static void OpenCraftingWindow(Packet packet)
        {
            CraftingProfession profession = (CraftingProfession)packet.ReadByte();
            byte skillInProf = packet.ReadByte();
            int length = packet.ReadInt();

            for (int i = 0; i < length; i++)
            {
                Item item = GameManager.Instance.GetItem(packet.ReadItemId());
                int requiredItemsCount = packet.ReadInt();

                var requiredItems = new List<(Item, ushort)>(requiredItemsCount);

                for (int j = 0; j < requiredItemsCount; j++)
                {
                    Item requiredItem = GameManager.Instance.GetItem(packet.ReadItemId());
                    ushort requiredAmount = packet.ReadUShort();
                    requiredItems.Add((requiredItem, requiredAmount));
                }

                var craftableItem = new CraftableItem(profession, item, requiredItems);

                switch (profession)
                {
                    case CraftingProfession.Blacksmithing:
                        UIManager.GameUI.CraftingWindow.BlacksmithingItems.Add(item.Id, craftableItem);
                        break;
                    case CraftingProfession.Woodworking:
                        UIManager.GameUI.CraftingWindow.WoodworkingItems.Add(item.Id, craftableItem);
                        break;
                    case CraftingProfession.Tailoring:
                        UIManager.GameUI.CraftingWindow.TailoringItems.Add(item.Id, craftableItem);
                        break;
                }
            }

            UIManager.GameUI.CraftingWindow.OpenAndLoad(profession, skillInProf);
        }

        public static void DoorState(Packet packet)
        {
            var doorsCount = packet.ReadByte();
            for (var i = 0; i < doorsCount; i++)
            {
                Vector2 position = packet.ReadVector2();
                bool state = packet.ReadBool();
                if (Door.Doors.TryGetValue(position, out var door))
                    door.gameObject.SetActive(state);
                else
                    Debug.Log($"Door at position {position} not found");
            }
        }

        public static void QuestAssigned(Packet packet)
        {
            QuestId questId = packet.ReadQuestId();
            byte startOnStep = packet.ReadByte();
            bool autoComplete = packet.ReadBool();
            if (!autoComplete)
            {
                byte npcsCount = packet.ReadByte();
                var turnInNpcsIds = new NpcId[npcsCount];
                for (byte i = 0; i < npcsCount; i++)
                {
                    NpcId npcId = packet.ReadNpcId();
                    GameManager.Instance.GetNpcInfo(npcId).TurnInableQuests.Add(questId);
                    turnInNpcsIds[i] = npcId;
                }
                
                GameManager.Instance.NpcsPool.ForEachActiveObject((npc, npcIds) =>
                {
                    if (npcIds.Contains(npc.Info.Id))
                    {
                        // TODO change this npc quest head icon to '?'
                    }
                }, turnInNpcsIds);
            }
            GameManager.Instance.LocalPlayer.QuestManager.AddNewQuest(questId, autoComplete, startOnStep);
        }

        public static void QuestProgressUpdate(Packet packet)
        {
            QuestId questId = packet.ReadQuestId();
            GameManager.Instance.LocalPlayer.QuestManager.UpdateProgress(questId, packet);
        }

        public static void QuestCompleted(Packet packet)
        {
            QuestId questId = packet.ReadQuestId();
            GameManager.Instance.LocalPlayer.QuestManager.CompleteQuest(questId);
        }

        public static void NpcQuests(Packet packet)
        {
            int npcInstanceId = packet.ReadInt();
            NpcId npcId = packet.ReadNpcId();
            byte length = packet.ReadByte();
            var npcQuests = GameManager.Instance.GetNpcInfo(npcId).AvailableQuests;
            npcQuests.Clear();
            for (byte i = 0; i < length; i++)
                npcQuests.Add(packet.ReadQuestId());
            
            if (GameManager.Instance.NpcsPool.TryFindObject(npcInstanceId, out var npc))
            {
                // TODO update npcs quests and show ! icon over the head
            }
            //UIManager.GameUI.QuestWindow.LoadNpcQuests(questIds);
        }

        public static void CanSkillUpTalentReturn(Packet packet)
        {
            var profession = (Profession)packet.ReadByte();
            byte nodeId = packet.ReadByte();
            bool canSkillUp = packet.ReadBool();
            UIManager.GameUI.StatsWindow.WorkerTalents.CanSkillUpReturn(profession, nodeId, canSkillUp);
        }

        public static void OnYouJoinedParty(Packet packet)
        {
            ClientId leaderClientId = packet.ReadClientId();
            bool canEditPercentages = packet.ReadBool();
            byte membersCount = packet.ReadByte();
            var members = new List<PlayerManager>(membersCount);
            for (var i = 0; i < membersCount; i++)
            {
                ClientId playerClientId = packet.ReadClientId();
                members.Add(GameManager.Instance.GetPlayer(playerClientId));
            }
            UIManager.GameUI.PartyWindow.OnLocalPlayerJoinedParty(leaderClientId, canEditPercentages, members);
        }

        public static void OnPlayerJoinedParty(Packet packet)
        {
            var player = GameManager.Instance.GetPlayer(packet.ReadClientId());
            UIManager.GameUI.PartyWindow.OnPlayerJoinedParty(player);
        }

        public static void OnPlayerLeftParty(Packet packet)
        {
            var player = GameManager.Instance.GetPlayer(packet.ReadClientId());
            bool kicked = packet.ReadBool();
            UIManager.GameUI.PartyWindow.OnPlayerLeftParty(player, kicked);
        }

        public static void OnCanEditPercentagesChanged(Packet packet)
        {
            bool canEditPercentages = packet.ReadBool();
            UIManager.GameUI.PartyWindow.OnCanEditPercentagesChanged(canEditPercentages);
        }
        
        public static void OnExperiencePercentageChanged(Packet packet)
        {
            byte percentageBonus = packet.ReadByte();
            byte playerCount = packet.ReadByte();
            var playerPercentages = new Dictionary<PlayerManager, byte>(playerCount);

            for (var i = 0; i < playerCount; i++)
            {
                var player = GameManager.Instance.GetPlayer(packet.ReadClientId());
                playerPercentages.Add(player, packet.ReadByte());
            }
            
            UIManager.GameUI.PartyWindow.OnExperiencePercentagesChanged(playerPercentages, percentageBonus);
        }

        public static void OnPartyLeaderChanged(Packet packet)
        {
            ClientId leaderClientId = packet.ReadClientId();
            UIManager.GameUI.PartyWindow.OnPartyLeaderChanged(GameManager.Instance.GetPlayer(leaderClientId));
        }

        public static void OnPartyGainedExperience(Packet packet)
        {
            uint experience = packet.ReadUInt();
            byte membersCount = packet.ReadByte();
            var members = new PlayerManager[membersCount];
            for (var i = 0; i < membersCount; i++)
                members[i] = GameManager.Instance.GetPlayer(packet.ReadClientId());
            UIManager.GameUI.PartyWindow.OnPartyGainedExperience(members, experience);
        }

        public static void OnPartyMemberGainedExperience(Packet packet)
        {
            uint experience = packet.ReadUInt();
            var player = GameManager.Instance.GetPlayer(packet.ReadClientId());
            UIManager.GameUI.PartyWindow.OnPartyMemberGainedExperience(player, experience);
        }

        public static void FetchMailsReturn(Packet packet)
        {
            var mailCount = packet.ReadByte();
            var mails = new List<Mail>(mailCount);
            for (var i = 0; i < mailCount; i++)
            {
                var mail = new Mail
                {
                    Id = packet.ReadUInt(),
                    SenderName = packet.ReadString(),
                    Subject = packet.ReadString(),
                    Body = packet.ReadString(),
                    ExpiresIn = new TimeSpan(packet.ReadLong()),
                };
                mail.ExpirationDate = DateTime.Now.Add(mail.ExpiresIn);
                var itemsCount = packet.ReadByte();
                for (var j = 0; j < itemsCount; j++)
                    mail.Items.Add(packet.ReadUShort(), packet.ReadUInt());
                
                mails.Add(mail);
            }
            
            UIManager.GameUI.MailWindow.ReceivedMailPanel.AddEntries(mails);
        }

        public static void RemoveMailItem(Packet packet)
        {
            uint mailId = packet.ReadUInt();
            ItemId itemId = packet.ReadItemId();
            UIManager.GameUI.MailWindow.RemoveItemFromMail(mailId, itemId);
        }

        public static void PlayerDescriptionChanged(Packet packet)
        {
            ClientId clientId = packet.ReadClientId();
            string description = packet.ReadString();
            GameManager.Instance.GetPlayer(clientId).Description = description;
        }
        
        // Cannot use Conditional attribute here because this method is called from a dictionary initialized on runtime
        #if AO_DEBUG
        public static void DebugNpcPath(Packet packet)
        {
            int npcInstanceId = packet.ReadInt();
            if (GameManager.Instance.NpcsPool.TryFindObject(npcInstanceId, out var npc))
                npc.UpdatePath(packet);
        }
        #endif
    }
}