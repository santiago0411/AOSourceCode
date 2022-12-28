using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Items;
using AO.Npcs;
using AO.Npcs.Utils;
using AO.Players;
using AO.Players.Utils;
using AO.Spells;
using AO.Systems;
using AO.Systems.Mailing;
using AO.Systems.Professions;
using AO.World;
using Client = AO.Network.Server.Client;

namespace AO.Network
{ 
    /// <summary>Contains all the methods to to send packets to the client(s).</summary>
    public static class PacketSender
    {
        /// <summary>Sends a packet to a client via TCP.</summary>
        /// <param name="toClient">The client to send the packet the packet to.</param>
        /// <param name="packet">The packet to send to the client.</param>
        private static void SendTcpData(ClientId toClient, Packet packet)
        {
            NetworkManager.Instance.SendPacketTcp(toClient, packet);
        }

        /// <summary>Sends a packet to all clients via TCP.</summary>
        /// <param name="packet">The packet to send.</param>
        private static void SendTcpDataToAll(Packet packet)
        {
            NetworkManager.Instance.SendPacketTcpToAll(packet);
        }

        /// <summary>Sends a packet to all clients except one via TCP.</summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="exceptClient">The client to NOT send the data to.</param>
        private static void SendTcpDataToAll(Packet packet, ClientId exceptClient)
        {
            NetworkManager.Instance.SendPacketTcpToAll(packet, exceptClient);
        }

        /// <summary>Sends a packet to all clients in near the player via TCP.</summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="playerSender">The player sending this packet.</param>
        private static void SendTcpDataToNearby(Packet packet, Player playerSender)
        {
            // Send it to the player themselves and then to everyone around
            NetworkManager.Instance.SendPacketTcp(playerSender.Id, packet);
            foreach (var clientId in playerSender.NearbyPlayers)
                NetworkManager.Instance.SendPacketTcp(clientId, packet);
        }
        
        /// <summary>Sends a packet to all clients in the same area as the player via TCP.</summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="map">The map to send the packet to.</param>
        private static void SendTcpDataToSameMap(Packet packet, Map map)
        {
            foreach (var player in map.PlayersInMap)
                NetworkManager.Instance.SendPacketTcp(player.Id, packet);
        }

        /// <summary>Sends a packet to all clients in near the player via TCP.</summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="map">The map to send the packet to.</param>
        /// <param name="senderPosition">The starting point to check the area.</param>
        private static void SendTcpDataToSameMap(Packet packet, Map map, Vector2 senderPosition)
        {
            foreach (var player in map.PlayersInMap)
            {
                Vector2 distance = senderPosition - player.CurrentTile.Position;
                if (Mathf.Abs(distance.x) <= Constants.VISION_RANGE_X + 1 && Mathf.Abs(distance.y) <= Constants.VISION_RANGE_Y + 1)
                    NetworkManager.Instance.SendPacketTcp(player.Id, packet);
            }
        }

        /// <summary>Sends a packet to a client via UDP.</summary>
        /// <param name="toClient">The client to send the packet the packet to.</param>
        /// <param name="packet">The packet to send to the client.</param>
        private static void SendUdpData(ClientId toClient, Packet packet)
        {
            NetworkManager.Instance.SendPacketUdp(toClient, packet);
        }
        
        /// <summary>Sends a packet to all clients in near the player via UDP.</summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="playerSender">The player sending this packet.</param>
        private static void SendUdpDataToNearby(Packet packet, Player playerSender)
        {
            // Send it to the player themselves and then to everyone around
            NetworkManager.Instance.SendPacketUdp(playerSender.Id, packet);
            foreach (var clientId in playerSender.NearbyPlayers)
                NetworkManager.Instance.SendPacketUdp(clientId, packet);
        }

        #region Packets
        public static void Welcome(ClientId toClient)
        {
            using var packet = new Packet(ServerPackets.Welcome);
            packet.Write(toClient.AsPrimitiveType());
            SendTcpData(toClient, packet);
        }

        public static void LoginReturn(ClientId toClient, LoginRegisterMessage msgId)
        {
            using var packet = new Packet(ServerPackets.LoginReturn);
            packet.Write((byte)msgId);
            SendTcpData(toClient, packet);
        }

        public static void RegisterAccountReturn(ClientId toClient, LoginRegisterMessage msgId)
        {
            using var packet = new Packet(ServerPackets.RegisterAccountReturn);
            packet.Write((byte)msgId);
            SendTcpData(toClient, packet);
        }

        public static void GetRacesAttributesReturn(ClientId toClient)
        {
            using var packet = new Packet(ServerPackets.GetRacesAttributesReturn);

            packet.Write((byte)CharacterManager.Instance.Races.Count);

            foreach (var (raceType, race) in CharacterManager.Instance.Races)
            {
                packet.Write((byte)raceType);
                packet.Write((byte)race.RaceModifiers.Count);

                foreach (var (attribute, value) in race.RaceModifiers)
                {
                    packet.Write((byte)attribute);
                    packet.Write((byte)value);
                }
            }

            SendTcpData(toClient, packet);
        }

        public static void GetCharactersReturn(Client toClient)
        {
            Dictionary<CharacterId, AOCharacterInfo> charactersList = toClient.ClientGameData.AccountCharacters;
            
            using var packet = new Packet(ServerPackets.GetCharactersReturn);
            packet.Write((byte)charactersList.Count);

            foreach (AOCharacterInfo charInfo in charactersList.Values)
            {
                packet.Write(charInfo.CharacterId.AsPrimitiveType());
                packet.Write(charInfo.CharacterName);  
            }

            SendTcpData(toClient.Id, packet);
        }

        public static void CreateCharacterReturn(ClientId toClient, CreateCharacterMessage msg, CharacterId newCharId = default)
        {
            using var packet = new Packet(ServerPackets.CreateCharacterReturn);
            packet.Write((byte)msg);
            if (msg == CreateCharacterMessage.Ok)
                packet.Write(newCharId.AsPrimitiveType());
            SendTcpData(toClient, packet);
        }

        public static void SpawnPlayer(ClientId toClient, Player player)
        {
            using var packet = new Packet(ServerPackets.SpawnPlayer);
            packet.Write(player.Id.AsPrimitiveType());
            packet.Write(player.CurrentMap.Number);
            packet.Write(player.Username);
            packet.Write(player.Description);
            packet.Write((byte)player.Class.ClassType);
            packet.Write((byte)player.Race.RaceType);
            packet.Write((byte)player.Gender);
            packet.Write((byte)player.Faction);
            packet.Write(player.IsGameMaster);
            packet.Write(player.Head);

            SendTcpData(toClient, packet);
        }

        public static void PlayerMaxResources(Player player)
        {
            using var packet = new Packet(ServerPackets.PlayerMaxResources);
            packet.Write(player.Health.MaxHealth);
            packet.Write(player.Mana.MaxAmount);
            packet.Write(player.Stamina.MaxAmount);
            packet.Write(player.Hunger.MaxAmount);
            packet.Write(player.Thirst.MaxAmount);

            SendTcpData(player.Id, packet);
        }

        public static void PlayerPrivateInfo(Player player)
        {
            using (var packet = new Packet(ServerPackets.PlayerPrivateInfo))
            {
                packet.Write((byte)player.Class.ClassType);
                SendTcpData(player.Id, packet);
            }

            PlayerInputReturn(player, PlayerInput.SafeToggle);
        }

        public static void PlayerSkills(Player player)
        {
            using var packet = new Packet(ServerPackets.PlayerSkills);
            packet.Write((byte)player.Skills.Count);

            foreach (var (skill, value) in player.Skills)
            {
                packet.Write((byte)skill);
                packet.Write(value);
            }

            SendTcpData(player.Id, packet);
        }

        public static void ChatBroadcast(Player playerSender, string message)
        {
            using var packet = new Packet(ServerPackets.ChatBroadcast);
            packet.Write(playerSender.Id.AsPrimitiveType());
            packet.Write(message);
            SendTcpDataToNearby(packet, playerSender);
        }

        public static void PlayerPosition(Player player)
        {
            using var packet = new Packet(ServerPackets.PlayerPosition);
            packet.Write(player.Id.AsPrimitiveType());
            packet.Write(player.transform.position);
            packet.Write((byte)player.Facing.Heading);
            SendUdpDataToNearby(packet, player);
        }

        public static void PlayerRangeChanged(ClientId toClient, ClientId playerRangeChangedId, bool inRange)
        {
            using var packet = new Packet(ServerPackets.PlayerRangeChanged);
            packet.Write(playerRangeChangedId.AsPrimitiveType());
            packet.Write(inRange);

            SendTcpData(toClient, packet);
        }

        public static void PlayerUpdatePosition(ClientId toClient, int xPos, int yPos)
        {
            using var packet = new Packet(ServerPackets.PlayerUpdatePosition);
            packet.Write(xPos);
            packet.Write(yPos);
            SendTcpData(toClient, packet);
        }

        public static void PlayerDisconnected(ClientId clientId)
        {
            using var packet = new Packet(ServerPackets.PlayerDisconnected);
            packet.Write(clientId.AsPrimitiveType());
            SendTcpDataToAll(packet);
        }

        public static void PlayerResources(Player player)
        {
            using var packet = new Packet(ServerPackets.PlayerResources);
            packet.Write(player.Health.CurrentHealth);
            packet.Write(player.Mana.CurrentAmount);
            packet.Write(player.Stamina.CurrentAmount);
            packet.Write(player.Hunger.CurrentAmount);
            packet.Write(player.Thirst.CurrentAmount);

            SendTcpData(player.Id, packet);
        }

        public static void PlayerIndividualResource(Player player, Resource resource)
        {
            using var packet = new Packet(ServerPackets.PlayerIndividualResource);
            packet.Write((byte)resource);

            switch (resource)
            {
                case Resource.Health:
                    packet.Write(player.Health.CurrentHealth);
                    break;
                case Resource.Mana:
                    packet.Write(player.Mana.CurrentAmount);
                    break;
                case Resource.Stamina:
                    packet.Write(player.Stamina.CurrentAmount);
                    break;
                case Resource.HungerAndThirst:
                    packet.Write(player.Hunger.CurrentAmount);
                    packet.Write(player.Thirst.CurrentAmount);
                    break;
            }

            SendTcpData(player.Id, packet);
        }

        public static void PlayerStats(Player player)
        {
            using var packet = new Packet(ServerPackets.PlayerStats);
            packet.Write(true); // True means all stats
            packet.Write(player.Level);
            packet.Write(player.CurrentExperience);
            packet.Write(CharacterManager.Instance.Levels[player.Level].MaxXp);
            packet.Write(player.AssignableSkills);
            packet.Write(player.AvailableTalentPoints);

            packet.Write((byte)player.Stats.Count);
            foreach (var entry in player.Stats)
                packet.Write(entry.Value);

            SendTcpData(player.Id, packet);
        }

        public static void PlayerStat(Player player, PlayerStat playerStat)
        {
            using var packet = new Packet(ServerPackets.PlayerStats);
            packet.Write(false); // False means only one stat is being sent
            packet.Write((byte)playerStat);
            packet.Write(player.Stats[playerStat]);
            SendTcpData(player.Id, packet);
        }

        public static void PlayerTalentsPoints(Player player)
        {
            if (player.Class.ClassType != ClassType.Worker)
                return;
            
            using var packet = new Packet(ServerPackets.PlayerTalentsPoints);
            player.WorkerTalentTrees.WriteAllTalentsCurrentPoints(packet);
            SendTcpData(player.Id, packet);
        }

        public static void PlayerLeveledUpTalents(Player player)
        {
            using var packet = new Packet(ServerPackets.PlayerLeveledUpTalents);
            player.WorkerTalentTrees.WriteLeveledUpTalents(packet);
            SendTcpData(player.Id, packet);
        }

        public static void PlayerGainedXp(ClientId toClient, uint xpGained)
        {
            using var packet = new Packet(ServerPackets.PlayerGainedXp);
            packet.Write(xpGained);
            SendTcpData(toClient, packet);
        }

        public static void PlayerAttributes(Player player)
        {
            using var packet = new Packet(ServerPackets.PlayerAttributes);
            packet.Write((byte)player.Attributes.Count);

            foreach (var value in player.Attributes.Values)
                packet.Write(value);

            SendTcpData(player.Id, packet);
        }

        public static void PlayerGold(Player player)
        {
            using var packet = new Packet(ServerPackets.PlayerGold);
            packet.Write(player.Gold);
            SendTcpData(player.Id, packet);
        }

        public static void ClickRequest(ClientId toClient, ClickRequest request)
        {
            using var packet = new Packet(ServerPackets.ClickRequest);
            packet.Write((byte)request);
            SendTcpData(toClient, packet);
        }

        public static void SendWorldItemToPlayer(Player player, WorldItem worldItem)
        {
            using var packet = new Packet(ServerPackets.WorldItemSpawned);
            packet.Write(worldItem.InstanceId);
            packet.Write(worldItem.ItemId.AsPrimitiveType());
            packet.Write(worldItem.CurrentMap.Number);
            packet.Write(worldItem.transform.position);
            SendTcpData(player.Id, packet);
        }

        public static void WorldItemDestroyed(int itemInstanceId, HashSet<Player> sendToPlayers)
        {
            using var packet = new Packet(ServerPackets.WorldItemDestroyed);
            packet.Write(itemInstanceId);
            foreach (var player in sendToPlayers)
                SendTcpData(player.Id, packet);
        }

        public static void PlayerInventory(ClientId toClient, InventorySlot slot)
        {
            using var packet = new Packet(ServerPackets.PlayerInventory);
            packet.Write(slot.Slot);
            packet.Write(slot.Item.Id.AsPrimitiveType());
            packet.Write(slot.Quantity);
            packet.Write(slot.SellingPrice);
            SendTcpData(toClient, packet);
        }

        public static void PlayerUpdateInventory(ClientId toClient, byte slot, ushort quantity)
        {
            using var packet = new Packet(ServerPackets.PlayerUpdateInventory);
            packet.Write(slot);
            packet.Write(quantity);
            SendTcpData(toClient, packet);
        }

        public static void PlayerSwapInventorySlots(ClientId toClient, byte slotA, byte slotB)
        {
            using var packet = new Packet(ServerPackets.PlayerSwapInventorySlots);
            packet.Write(slotA);
            packet.Write(slotB);
            SendTcpData(toClient, packet);
        }

        public static void PlayerEquippedItems(ClientId equippedPlayerId, Span<(byte, ItemId)> equippedItems, ClientId toClient)
        {
            using var packet = new Packet(ServerPackets.PlayerEquippedItems);
            packet.Write(equippedPlayerId.AsPrimitiveType());
            packet.Write((byte)equippedItems.Length);
            foreach (var (slotId, itemId) in equippedItems)
            {
                packet.Write(slotId);
                packet.Write(itemId.AsPrimitiveType());
            }
            SendTcpData(toClient, packet);
        }

        public static void OnPlayerItemEquippedChanged(Player equippedPlayer, byte slot, ItemId itemId, bool equipped)
        {
            using var packet = new Packet(ServerPackets.OnPlayerItemEquippedChanged);
            packet.Write(equippedPlayer.Id.AsPrimitiveType());
            packet.Write(slot);
            packet.Write(itemId.AsPrimitiveType());
            packet.Write(equipped);
            SendTcpDataToNearby(packet, equippedPlayer);
        }

        public static void EndEnterWorld(ClientId toClient)
        {
            using var packet = new Packet(ServerPackets.EndEnterWorld);
            SendTcpData(toClient, packet);
        }

        public static void ConsoleMessageToPlayer(ClientId toClient, string message, ConsoleMessage messageType)
        {
            using var packet = new Packet(ServerPackets.ConsoleMessage);
            packet.Write(message);
            packet.Write((byte)messageType);
            SendTcpData(toClient, packet);
        }

        public static void BroadcastConsoleMessage(string message, ConsoleMessage messageType)
        {
            using var packet = new Packet(ServerPackets.ConsoleMessage);
            packet.Write(message);
            packet.Write((byte)messageType);
            SendTcpDataToAll(packet);
        }

        public static void UpdatePlayerSpell(ClientId toClient, byte spellSlot, SpellId spellId)
        {
            using var packet = new Packet(ServerPackets.UpdatePlayerSpells);
            packet.Write(spellSlot);
            packet.Write(spellId.AsPrimitiveType());
            SendTcpData(toClient, packet);
        }

        public static void UpdatePlayerSpells(Player player)
        {
            for (byte i = 0; i < player.Spells.Length; i++)
            {
                if (player.Spells[i] is not null)
                {
                    var spell = player.Spells[i];
                    UpdatePlayerSpell(player.Id, i, spell.Id);
                }
            }
        }

        public static void MovePlayerSpell(ClientId toClient, byte slotOne, byte slotTwo)
        {
            using var packet = new Packet(ServerPackets.MovePlayerSpell);
            packet.Write(slotOne);
            packet.Write(slotTwo);
            SendTcpData(toClient, packet);
        }

        public static void SayMagicWords(Player player, SpellId spellId)
        {
            using var packet = new Packet(ServerPackets.SayMagicWords);
            packet.Write(player.Id.AsPrimitiveType());
            packet.Write(spellId.AsPrimitiveType());
            SendTcpDataToNearby(packet, player);
        }

        public static void NpcSpawn(Npc npc, ClientId toClient)
        {
            using var packet = new Packet(ServerPackets.NpcSpawn);
            packet.Write(npc.Info.Id.AsPrimitiveType());
            packet.Write(npc.InstanceId);
            packet.Write(npc.CurrentMap.Number);
            packet.Write(npc.transform.position);
            SendTcpData(toClient, packet);
        }

        public static void NpcPosition(Npc npc, CustomUniqueList<Player> playersInRange)
        {
            using var packet = new Packet(ServerPackets.NpcPosition);
            packet.Write(npc.InstanceId);
            packet.Write(npc.transform.position);
            foreach (var p in playersInRange)
                SendUdpData(p.Id, packet);
        }

        public static void NpcRangeChanged(ClientId toClient, int npcInstanceId, bool inRange)
        {
            using var packet = new Packet(ServerPackets.NpcRangeChanged);
            packet.Write(npcInstanceId);
            packet.Write(inRange);
            SendTcpData(toClient, packet);
        }

        public static void NpcFacing(Npc npc, Vector2 difference, CustomUniqueList<Player> playersInRange)
        {
            using var packet = new Packet(ServerPackets.NpcFacing);
            packet.Write(npc.InstanceId);
            packet.Write(difference);
            foreach (var p in playersInRange)
                SendUdpData(p.Id, packet);
        }

        public static void NpcStartTrade(ClientId toClient, NpcId npcId, NpcInventorySlot[] npcInventory, float discount)
        {
            using var packet = new Packet(ServerPackets.NpcStartTrade);
            packet.Write(npcId.AsPrimitiveType());
            int realLength = npcInventory.Count(x => x is not null);
            packet.Write((byte)realLength);

            foreach (var slot in npcInventory)
            {
                if (slot is not null)
                {
                    packet.Write(slot.Slot);
                    packet.Write(slot.Item.Id.AsPrimitiveType());
                    packet.Write(slot.Quantity);
                    packet.Write(Mathf.CeilToInt(slot.Price / discount));
                }
            }
            SendTcpData(toClient, packet);
        }

        public static void NpcUpdateInventory(ClientId toClient, byte slot, ushort quantity, ItemId? itemId = null)
        {
            using var packet = new Packet(ServerPackets.NpcInventoryUpdate);
            packet.Write(slot);
            packet.Write(quantity);
            if (itemId is not null)
            {
                packet.Write(itemId.Value.AsPrimitiveType());
                packet.Write(GameManager.Instance.GetItem(itemId.Value).Price);
            }
            SendTcpData(toClient, packet);
        }

        public static void NpcDespawned(Npc npc, HashSet<Player> sendToPlayers)
        {
            using var packet = new Packet(ServerPackets.NpcDespawned);
            packet.Write(npc.InstanceId);
            foreach (var player in sendToPlayers)
                SendTcpData(player.Id, packet);
        }

        public static void UpdatePlayerStatus(Player player, PlayerStatus playerStatus)
        {
            using var packet = new Packet(ServerPackets.UpdatePlayerStatus);
            packet.Write(player.Id.AsPrimitiveType());
            packet.Write((byte)playerStatus);

            switch (playerStatus)
            {
                case PlayerStatus.UsedBoat:
                    packet.Write(player.Flags.IsSailing);
                    break;
                case PlayerStatus.Mounted:
                    break;
                case PlayerStatus.ChangedFaction:
                    packet.Write((byte)player.Faction);
                    packet.Write(player.IsGameMaster);
                    break;
                case PlayerStatus.ChangedGuildName:
                    packet.Write(player.GuildName);
                    break;
            }

            SendTcpDataToNearby(packet, player);
        }

        public static void SendMultiMessage(ClientId toClient, MultiMessage multiMessage)
        {
            using var packet = new Packet(ServerPackets.MultiMessage);
            packet.Write((byte)multiMessage);
            SendTcpData(toClient, packet);
        }
        
        public static void SendMultiMessage(ClientId toClient, MultiMessage multiMessage, Span<int> args)
        {
            using var packet = new Packet(ServerPackets.MultiMessage);
            packet.Write((byte)multiMessage);

            switch (multiMessage)
            {
                //// COMBAT ////
                case MultiMessage.AttackerEnvenomed:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // TargetId
                    break;
                case MultiMessage.KilledPlayer:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // TargetId
                    break;
                case MultiMessage.NpcHitPlayer:
                    packet.Write((byte)args[0]); // BodyPart
                    packet.Write(args[1]); // Damage
                    break;
                case MultiMessage.NpcDamageSpellPlayer:
                    packet.Write(((NpcId)args[0]).AsPrimitiveType()); // NpcId
                    packet.Write(((SpellId)args[1]).AsPrimitiveType()); // SpellId
                    packet.Write(args[2]); // Damage
                    break;
                case MultiMessage.PlayerDamageSpellNpc:
                    packet.Write(args[0]); // Damage
                    break;
                case MultiMessage.PlayerAttackedSwing:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // AttackerId
                    break;
                case MultiMessage.PlayerDamageSpellEnemy:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // TargetId
                    packet.Write(args[1]); // Damage
                    break;
                case MultiMessage.EnemyDamageSpellPlayer:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // AttackerId
                    packet.Write(args[1]); // Damage
                    break;
                case MultiMessage.PlayerGotStabbed:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // AttackerId
                    packet.Write(args[1]); // Damage
                    break;
                case MultiMessage.PlayerHitByPlayer:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // AttackerId
                    packet.Write((byte)args[1]); // BodyPart
                    packet.Write(args[2]); // Damage
                    break;
                case MultiMessage.PlayerHitNpc:
                    packet.Write(args[0]); // Damage
                    break;
                case MultiMessage.PlayerHitPlayer:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // TargetId
                    packet.Write((byte)args[1]); // BodyPart
                    packet.Write(args[2]); // Damage
                    break;
                case MultiMessage.PlayerKilled:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // AttackerId
                    break;
                case MultiMessage.StabbedNpc:
                    packet.Write(args[0]); // Damage
                    break;
                case MultiMessage.StabbedPlayer:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType());; // TargetId
                    packet.Write(args[1]); // Damage
                    break;
                case MultiMessage.TargetGotEnvenomed:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // AttackerId
                    break;
                ////////////////
                
                ///// NPC //////
                case MultiMessage.DisplayInfo:
                    packet.Write(((NpcId)args[0]).AsPrimitiveType()); // NpcId
                    packet.Write(args[1]); // CurrentHealth
                    packet.Write(args[2]); // MaxHealth
                    packet.Write(Convert.ToBoolean(args[3])); // IsPet
                    packet.Write(((ClientId)args[4]).AsPrimitiveType()); // PetOwner or FightingUser Id
                    break;
                case MultiMessage.ShowNpcDescription:
                    packet.Write(args[0]); //NpcInstanceId
                    break;
                ////////////////
                
                //// ITEMS  ////
                case MultiMessage.NotEnoughSkillToUse:
                    packet.Write((byte)args[0]); // Skill
                    break;
                case MultiMessage.PlayerDroppedItemTo:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // TargetId
                    packet.Write(((ItemId)args[1]).AsPrimitiveType()); // ItemId
                    packet.Write((ushort)args[2]); // Quantity
                    break;
                case MultiMessage.PlayerGotItemDropped:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // DropperId
                    packet.Write((ushort)args[1]); // ItemId
                    packet.Write((ushort)args[2]); // Quantity
                    break;
                ////////////////

                //// SPELLS ////
                case MultiMessage.NpcHealedPlayer:
                    packet.Write(((NpcId)args[0]).AsPrimitiveType()); // NpcId
                    packet.Write(args[1]); // Healing amount
                    break;
                case MultiMessage.PlayerHealedNpc:
                    packet.Write(args[0]); // Healing amount
                    break;
                case MultiMessage.PlayerHealed:
                    packet.Write(args[0]); // Healing amount
                    packet.Write(((ClientId)args[1]).AsPrimitiveType()); // TargetId
                    break;
                case MultiMessage.PlayerGotHealed:
                    packet.Write(args[0]); // Healing amount
                    packet.Write(((ClientId)args[1]).AsPrimitiveType()); // CasterId
                    break;
                case MultiMessage.PlayerSelfHeal:
                    packet.Write(args[0]); // Healing amount
                    break;
                case MultiMessage.SpellMessage:
                    packet.Write(((SpellId)args[0]).AsPrimitiveType()); // SpellId
                    packet.Write(((ClientId)args[1]).AsPrimitiveType()); //CasterId
                    break;
                case MultiMessage.SpellSelfMessage:
                    packet.Write(((SpellId)args[0]).AsPrimitiveType()); // SpellId
                    break;
                case MultiMessage.SpellTargetMessage:
                    packet.Write(((SpellId)args[0]).AsPrimitiveType()); // SpellId
                    packet.Write(((ClientId)args[1]).AsPrimitiveType()); //TargetId
                    break;
                ////////////////

                //  LEVELING  //
                case MultiMessage.IncreasedHit:
                    packet.Write((ushort)args[0]); // Hit Increase
                    break;
                case MultiMessage.IncreasedHp:
                    packet.Write(args[0]); // Hp Increase
                    break;
                case MultiMessage.IncreasedStamina:
                    packet.Write((ushort)args[0]); // Stam Increase
                    break;
                case MultiMessage.IncreasedMana:
                    packet.Write((ushort)args[0]); // Mana Increase
                    break;
                ///////////////
                
                //// PARTY ////
                case MultiMessage.PlayerInvitedToParty:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // ClientId
                    break;
                case MultiMessage.YouInvitedPlayerToParty:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // ClientId
                    break;
                ///////////////

                // MAILING ///
                case MultiMessage.NewMailReceived:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // ClientId
                    break;
                ///////////////
                
                //// MISC /////
                case MultiMessage.ClickedOnPlayer:
                    packet.Write(((ClientId)args[0]).AsPrimitiveType()); // ClientId
                    packet.Write((byte)args[1]); // IsNewbie
                    packet.Write((byte)args[2]); // FactionType
                    break;
                case MultiMessage.ClickedOnWorldItem:
                    packet.Write(((ItemId)args[0]).AsPrimitiveType()); // ItemId
                    packet.Write((ushort)args[1]); // Quantity
                    break;
                case MultiMessage.ManaRecovered:
                    packet.Write((short)args[0]); // Mana
                    break;
                case MultiMessage.ObstacleClick:
                    packet.Write((byte)args[0]); // TagId
                    break;
                case MultiMessage.SkillLevelUp:
                    packet.Write((byte)args[0]); // Skill
                    packet.Write((byte)args[1]); // SkillLevel
                    break;
                case MultiMessage.TalentPointsObtained:
                    packet.Write((byte)args[0]); // Points
                    break;
                ///////////////
            }

            SendTcpData(toClient, packet);
        }

        public static void PlayerInputReturn(Player player, PlayerInput playerInput)
        {
            using var packet = new Packet(ServerPackets.PlayerInputReturn);
            packet.Write((byte)playerInput);

            switch (playerInput)
            {
                case PlayerInput.SafeToggle:
                    packet.Write(player.Flags.SafeToggleOn);
                    break;
                case PlayerInput.RessToggle:
                    packet.Write(player.Flags.RessToggleOn);
                    break;
            }

            SendTcpData(player.Id, packet);
        }

        public static void CreateParticlePlayer(ushort particleId, Player player)
        {
            using var packet = new Packet(ServerPackets.CreateParticle);
            packet.Write(particleId);
            packet.Write((byte)SpellTarget.User);
            packet.Write(player.Id.AsPrimitiveType());
            SendTcpDataToNearby(packet, player);
        }

        public static void CreateParticleNpc(ushort particleId, Npc npc)
        {
            using var packet = new Packet(ServerPackets.CreateParticle);
            packet.Write(particleId);
            packet.Write((byte)SpellTarget.Npc);
            packet.Write(npc.InstanceId);
            SendTcpDataToSameMap(packet, npc.CurrentMap, npc.transform.position);
        }

        public static void CreateParticleTerrain(ushort particleId, Map map, Vector2 position)
        {
            using var packet = new Packet(ServerPackets.CreateParticle);
            packet.Write(particleId);
            packet.Write((byte)SpellTarget.Terrain);
            packet.Write(position);
            SendTcpDataToSameMap(packet, map, position);
        }

        public static void OpenCraftingWindow(Player player, CraftingProfession profession)
        {
            Dictionary<ItemId, CraftableItem> craftablesDic;
            Skill skill;

            switch (profession)
            {
                case CraftingProfession.Blacksmithing:
                    craftablesDic = CraftingProfessions.BlacksmithingItems;
                    skill = Skill.Blacksmithing;
                    break;
                case CraftingProfession.Woodworking:
                    craftablesDic = CraftingProfessions.WoodworkingItems;
                    skill = Skill.Woodworking;
                    break;
                case CraftingProfession.Tailoring:
                    craftablesDic = CraftingProfessions.TailoringItems;
                    skill = Skill.Tailoring;
                    break;
                default:
                    return;
            }

            using var packet = new Packet(ServerPackets.OpenCraftingWindow);
            packet.Write((byte)profession);
            packet.Write(player.Skills[skill]);
            
            var craftableItems = new List<CraftableItem>(craftablesDic.Count);
            
            // Make a list with the items that haven't yet been sent to the player and the player can craft
            foreach (var craftableItem in craftablesDic.Values)
                if (!player.Flags.CraftableItemsSent.Contains(craftableItem.Id))
                    if (craftableItem.CanCraftItem(player))
                        craftableItems.Add(craftableItem);
            
            // Then write the list length and each of the items the player can craft
            packet.Write(craftableItems.Count);

            foreach (CraftableItem craftableItem in craftableItems)
            {
                packet.Write(craftableItem.Item.Id.AsPrimitiveType());
                packet.Write(craftableItem.RequiredItemsAndAmounts.Count);

                foreach (var (requiredItemId, requiredQuantity) in craftableItem.RequiredItemsAndAmounts)
                {
                    packet.Write(requiredItemId.AsPrimitiveType());
                    packet.Write(requiredQuantity);
                }
                player.Flags.CraftableItemsSent.Add(craftableItem.Id);
            }

            SendTcpData(player.Id, packet);
        }

        public static void DoorStates(Span<Vector2> positions, Span<bool> states, ClientId toClient)
        {
            using var packet = new Packet(ServerPackets.DoorState);
            packet.Write((byte)positions.Length);
            for (var i = 0; i < positions.Length; i++)
            {
                packet.Write(positions[i]);
                packet.Write(states[i]);
            }
            SendTcpData(toClient, packet);
        }

        public static void DoorState(Vector2 position, bool state, Map map)
        {
            using var packet = new Packet(ServerPackets.DoorState);
            packet.Write((byte)1); // Only sending 1 door info
            packet.Write(position);
            packet.Write(state);
            SendTcpDataToSameMap(packet, map);
        }

        public static void QuestAssigned(ClientId toClient, QuestId questId, bool autoComplete, List<NpcId> npcsTurnIn, byte startOnStep = 1)
        {
            using var packet = new Packet(ServerPackets.QuestAssigned);
            packet.Write(questId.AsPrimitiveType());
            packet.Write(startOnStep);
            packet.Write(autoComplete);
            if (!autoComplete)
            {
                packet.Write((byte)npcsTurnIn.Count);
                foreach (var npcId in npcsTurnIn)
                    packet.Write(npcId.AsPrimitiveType());
            }
            SendTcpData(toClient, packet);
        }

        public static void QuestProgressUpdate<T>(ClientId toClient, QuestId questId, T callbackData, Action<Packet, T> writeProgressCb)
        {
            using var packet = new Packet(ServerPackets.QuestProgressUpdate);
            packet.Write(questId.AsPrimitiveType());
            writeProgressCb(packet, callbackData);
            SendTcpData(toClient, packet);
        }

        public static void QuestCompleted(ClientId toClient, QuestId questId)
        {
            using var packet = new Packet(ServerPackets.QuestCompleted);
            packet.Write(questId.AsPrimitiveType());
            SendTcpData(toClient, packet);
        }
        
        /*public static void NpcQuests(ClientId toClient, IEnumerable<ushort> quests)
        {
            using var packet = new Packet(ServerPackets.NpcQuests);
            var enumerable = quests as ushort[] ?? quests.ToArray();
            packet.Write((byte)enumerable.Length);
            foreach (var quest in enumerable)
                packet.Write(quest);
            SendTcpData(toClient, packet);
        }*/

        public static void NpcQuests(ClientId toClient, int instanceId, NpcId npcId, List<QuestId> eligibleQuests)
        {
            using var packet = new Packet(ServerPackets.NpcQuests);
            packet.Write(instanceId);
            packet.Write(npcId.AsPrimitiveType());
            packet.Write((byte)eligibleQuests.Count);
            foreach (var questId in eligibleQuests)
                packet.Write(questId.AsPrimitiveType());
            SendTcpData(toClient, packet);
        }

        public static void CanSkillUpTalentReturn(ClientId toClient, Profession profession, byte nodeId, bool canSkillUp)
        {
            using var packet = new Packet(ServerPackets.CanSkillUpTalentReturn);
            packet.Write((byte)profession);
            packet.Write(nodeId);
            packet.Write(canSkillUp);
            SendTcpData(toClient, packet);
        }

        public static void OnYouJoinedParty(ClientId toClient, ClientId leaderClientId, bool canEditPercentages, List<Party.PartyMember> members)
        {
            using var packet = new Packet(ServerPackets.OnYouJoinedParty);
            packet.Write(leaderClientId.AsPrimitiveType());
            packet.Write(canEditPercentages);
            packet.Write((byte)(members.Count - 1));
            foreach (var member in members)
                if (member.Player.Id != toClient)
                    packet.Write(member.Player.Id.AsPrimitiveType());
            
            SendTcpData(toClient, packet);
        }

        public static void OnPlayerJoinedParty(ClientId newPlayerClientId, List<Party.PartyMember> members)
        {
            using var packet = new Packet(ServerPackets.OnPlayerJoinedParty);
            packet.Write(newPlayerClientId.AsPrimitiveType());
            foreach (var member in members)
                if (member.Player.Id != newPlayerClientId)
                    SendTcpData(member.Player.Id, packet);
        }

        public static void OnPlayerLeftParty(ClientId leaverClientId, bool kicked, List<Party.PartyMember> members)
        {
            using var packet = new Packet(ServerPackets.OnPlayerLeftParty);
            packet.Write(leaverClientId.AsPrimitiveType());
            packet.Write(kicked);
            foreach (var member in members)
                SendTcpData(member.Player.Id, packet);
        }

        public static void OnCanEditPercentagesChanged(ClientId toClient, bool canEditPercentages)
        {
            using var packet = new Packet(ServerPackets.OnCanEditPercentagesChanged);
            packet.Write(canEditPercentages);
            SendTcpData(toClient, packet);
        }

        public static void OnExperiencePercentageChanged(byte percentageBonus, List<Party.PartyMember> partyMembers)
        {
            using var packet = new Packet(ServerPackets.OnExperiencePercentageChanged);
            packet.Write(percentageBonus);
            packet.Write((byte)partyMembers.Count);
            foreach (var member in partyMembers)
            {
                packet.Write(member.Player.Id.AsPrimitiveType());
                packet.Write(member.ExperienceAsPercentage());
            }
            foreach (var member in partyMembers)
                SendTcpData(member.Player.Id, packet);
        }

        public static void OnPartyLeaderChanged(ClientId leaderClientId, List<Party.PartyMember> members)
        {
            using var packet = new Packet(ServerPackets.OnPartyLeaderChanged);
            packet.Write(leaderClientId.AsPrimitiveType());
            foreach (var member in members)
                SendTcpData(member.Player.Id, packet);
        }
        
        // This method is used when every party member gains the same amount of experience
        public static void OnPartyGainedExperience(uint experience, List<Party.PartyMember> membersThatGainedXp, List<Party.PartyMember> allMembers)
        {
            using var packet = new Packet(ServerPackets.OnPartyGainedExperience);
            packet.Write(experience);
            packet.Write((byte)membersThatGainedXp.Count);
            {
                foreach (var member in membersThatGainedXp)
                    packet.Write(member.Player.Id.AsPrimitiveType());
            }

            foreach (var member in allMembers)
                SendTcpData(member.Player.Id, packet);
        }
        
        // This method is used when custom percentages are active and each party member gains a different amount of xp
        public static void OnPartyMemberGainedExperience(uint experience, ClientId xpPlayerClientId, List<Party.PartyMember> allMembers)
        {
            using var packet = new Packet(ServerPackets.OnPartyMemberGainedExperience);
            packet.Write(experience);
            packet.Write(xpPlayerClientId.AsPrimitiveType());
            foreach (var member in allMembers)
                SendTcpData(member.Player.Id, packet);
        }

        public static void FetchMailsReturn(ClientId toClient, Mail mail)
        {
            using var packet = new Packet(ServerPackets.FetchMailsReturn);
            packet.Write(1); // Mail count
            WriteMailReturnPacket(packet, mail);
            SendTcpData(toClient, packet);
        }
        
        public static void FetchMailsReturn(ClientId toClient, List<Mail> mails)
        {
            using var packet = new Packet(ServerPackets.FetchMailsReturn);
            packet.Write((byte)mails.Count);
            foreach (var mail in mails)
                WriteMailReturnPacket(packet, mail);
            
            SendTcpData(toClient, packet);
        }

        private static void WriteMailReturnPacket(Packet packet, Mail mail)
        {
            packet.Write(mail.Id);
            packet.Write(mail.SenderCharacterName);
            packet.Write(mail.Subject);
            packet.Write(mail.Body);
            TimeSpan expiresIn = mail.ExpirationDate - DateTime.Now;
            packet.Write(expiresIn.Ticks);
            packet.Write((byte)mail.DeserializedItems.Count);
            foreach (var (itemId, quantity) in mail.DeserializedItems)
            {
                packet.Write(itemId.AsPrimitiveType());
                packet.Write(quantity);
            }
        }

        public static void RemoveMailItem(ClientId toClient, uint mailId, ItemId itemId)
        {
            using var packet = new Packet(ServerPackets.RemoveMailItem);
            packet.Write(mailId);
            packet.Write(itemId.AsPrimitiveType());
            SendTcpData(toClient, packet);
        }

        public static void PlayerDescriptionChanged(Player player)
        {
            using var packet = new Packet(ServerPackets.PlayerDescriptionChanged);
            packet.Write(player.Id.AsPrimitiveType());
            packet.Write(player.Description);
            SendTcpDataToAll(packet);
        }
        
        [Conditional("AO_DEBUG")]
        public static void DebugNpcPath(int instanceId, World.Tile[] path, CustomUniqueList<Player> sendTo)
        {
            using var packet = new Packet(ServerPackets.DebugNpcPath);
            packet.Write(instanceId);
            packet.Write((byte) path.Length);
            foreach (var tile in path)
                packet.Write(tile.Position);
            foreach (var player in sendTo)
                SendTcpData(player.Id, packet);
        }
        #endregion
    }
}