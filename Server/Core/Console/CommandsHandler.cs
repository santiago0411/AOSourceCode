using System;
using System.Linq;
using AO.Core.Database;
using AO.Core.Ids;
using AO.Core.Logging;
using UnityEngine;
using AO.Core.Utils;
using AO.Npcs;
using AO.World;
using AO.Players;
using AO.Systems.Questing;
using Client = AO.Network.Server.Client;
using PacketSender = AO.Network.PacketSender;

namespace AO.Core.Console
{
    public static class CommandsHandler
    {
        private static readonly LoggerAdapter log = new(typeof(CommandsHandler));

        public static void HandleExitCommand(Client fromClient, string[] args)
        {
            var player = fromClient.ClientGameData.Player;
            if (!player) return;
            
            #if UNITY_EDITOR
            bool force = args.Length > 0 && args[0] == "force";
            player.DisconnectPlayerFromWorld(force);
            #else
            player.DisconnectPlayerFromWorld();
            #endif
        }

        public static void HandleGiveItemCommand(Client fromClient, string[] args)
        {
            if (args.Length < 3) 
                return;

            if (!CharacterManager.Instance.TryGetOnlinePlayer(args[0], out Player player))
                return;
            
            if (ItemId.TryParse(args[1], out var itemId) && ushort.TryParse(args[2], out var quantity))
                player.Inventory.AddItemToInventory(GameManager.Instance.GetItem(itemId), quantity);
        }

        public static void HandleKillCommand(Client fromClient, string[] args)
        {
            var player = fromClient.ClientGameData.Player;
            if (!player) return;
            
            Npc targetNpc = player.Flags.TargetNpc;
           
            if (targetNpc)
                targetNpc.Kill(player);

            player.Flags.TargetNpc = null;
        }

        public static void HandleMeditateCommand(Client fromClient, string[] args)
        {
            var player = fromClient.ClientGameData.Player;
            if (!player) return;
            player.Meditate();
        }

        public static void HandleSpawnNpcCommand(Client fromClient, string[] args)
        {
            var player = fromClient.ClientGameData.Player;
            if (!player) return;
            
            if (args.Length >= 3)
            {
                if (NpcId.TryParse(args[0], out var npcId))
                {
                    if (int.TryParse(args[1], out int x) && int.TryParse(args[2], out int y))
                    {
                        Map map = player.CurrentMap;
                        Vector2 position = WorldMap.MapPositionToWorldPosition(map, new Vector2(x, y));
                        var npcInfo = GameManager.Instance.GetNpcInfo(npcId);
                        GameManager.Instance.SpawnNpc(npcInfo, map, position);
                    }
                }
            }
        }

        public static void HandleSpeedCommand(Client fromClient, string[] args)
        {
            var player = fromClient.ClientGameData.Player;
            if (!player) return;
            
            if (args.Length >= 1)
            {
                if (float.TryParse(args[0], out float speed))
                {
                    if (speed > 50f) speed = 50f;
                    if (speed < 1f) speed = 4f;
                    player.Speed = speed;
                }
            }
        }

        public static void HandleTeleportCommand(Client fromClient, string[] args)
        {
            if (args.Length < 4) 
                return;

            if (!CharacterManager.Instance.TryGetOnlinePlayer(args[0], out Player player)) 
                return;

            if (!short.TryParse(args[1], out short mapNumber) || !int.TryParse(args[2], out int x) || !int.TryParse(args[3], out int y)) 
                return;

            if (!GameManager.Instance.TryGetMap(mapNumber, out Map map))
                return;
            
            Vector2 position = WorldMap.MapPositionToWorldPosition(map, new Vector2(x, y));
            if (!WorldMap.FindEmptyTileForPlayer(player, position, out var newTile)) 
                return;
            
            player.CurrentTile.Player = null;
            player.transform.position = position;
            newTile.Player = player;
            player.CurrentTile = newTile;
        }

        public static void HandleGoldCommand(Client fromClient, string[] args)
        {
            var player = fromClient.ClientGameData.Player;
            if (!player) return;
            
            if (args.Length > 0)
                if (uint.TryParse(args[0], out uint value))
                    PlayerMethods.AddGold(player, value);
        }

        public static void HandleInvitePartyCommand(Client fromClient, string[] args)
        {
            if (args.Length <= 0) 
                return;

            var inviteeName = args[0];
            if (!CharacterManager.Instance.TryGetOnlinePlayer(inviteeName, out Player invitee) || !invitee) 
                return;
            
            Player leader = fromClient.ClientGameData.Player;
            if (!leader)
                return;
            
            var party = leader.Party;
            if (party is null || party.Leader != leader)
                return; // If the player wasn't in a party or isn't the leader (cheating) return

            party.InvitePlayer(invitee);
        }

        public static void HandleAcceptParty(Client fromClient, string[] args)
        {
            Player player = fromClient.ClientGameData.Player;
            if (player)
                player.Flags.PendingPartyInvite?.AddPlayer(player);
        }


        public static void HandleChangeLogLevel(Client fromClient, string[] args)
        {
            LoggerConfigurator.ChangeLogLevel(log4net.Core.Level.Info);
        }
        
        public static async void HandleReload(Client fromClient, string[] args)
        {
            var player = fromClient.ClientGameData.Player;
            if (!player)
                return;
            
            #if !AO_DEBUG
            if (!player.IsGameMaster)
                return;
            #endif
            
            if (args.Length < 1)
                return;

            if (!Enum.TryParse(args[0], true, out Reload reloadType))
                return;

            switch (reloadType)
            {
                case Reload.All:
                    break;
                case Reload.Constants:
                    log.Info("Reloading constants.");
                    PacketSender.BroadcastConsoleMessage("Reloading constants.", ConsoleMessage.DefaultMessage);
                    await Constants.Load(false);
                    PacketSender.BroadcastConsoleMessage("Successfully reloaded constants.", ConsoleMessage.DefaultMessage);
                    break;
                case Reload.Attributes:
                    log.Info("Reloading attributes.");
                    PacketSender.BroadcastConsoleMessage("Reloading attributes.", ConsoleMessage.DefaultMessage);
                    await CharacterManager.Instance.LoadAttributesBaseValues(false);
                    PacketSender.BroadcastConsoleMessage("Successfully reloaded attributes.", ConsoleMessage.DefaultMessage);
                    break;
                case Reload.Classes:
                    log.Info("Reloading classes.");
                    PacketSender.BroadcastConsoleMessage("Reloading classes.", ConsoleMessage.DefaultMessage);
                    await CharacterManager.Instance.LoadClasses(false);
                    PacketSender.BroadcastConsoleMessage("Successfully reloaded classes.", ConsoleMessage.DefaultMessage);
                    break;
                case Reload.Races:
                    log.Info("Reloading races.");
                    PacketSender.BroadcastConsoleMessage("Reloading races.", ConsoleMessage.DefaultMessage);
                    await CharacterManager.Instance.LoadRaces(false);
                    PacketSender.BroadcastConsoleMessage("Successfully reloaded races.", ConsoleMessage.DefaultMessage);
                    break;
                case Reload.Levels:
                    log.Info("Reloading levels.");
                    PacketSender.BroadcastConsoleMessage("Reloading levels.", ConsoleMessage.DefaultMessage);
                    await CharacterManager.Instance.LoadLevels(false);
                    PacketSender.BroadcastConsoleMessage("Successfully reloaded levels.", ConsoleMessage.DefaultMessage);
                    break;
                case Reload.Item:
                    if (args.Length >= 2)
                        await GameManager.Instance.ReloadItem(player.Id, args[1]);
                    break;
                case Reload.Spell:
                    if (args.Length >= 2)
                        await GameManager.Instance.ReloadSpell(player.Id, args[1]);
                    break;
                case Reload.Npc:
                    if (args.Length >= 2)
                        await GameManager.Instance.ReloadNpc(player.Id, args[1]);
                    break;
                case Reload.CraftableItem:
                    if (args.Length >= 2)
                        await GameManager.Instance.ReloadCraftableItem(player.Id, args[1]);
                    break;
                case Reload.Quest:
                    if (args.Length >= 2)
                        await GameManager.Instance.ReloadQuest(player.Id, args[1]);
                    break;
            }
        }

        public static void HandleKick(Client fromClient, string[] args)
        {
            if (args.Length < 1) 
                return;

            if (CharacterManager.Instance.TryGetOnlinePlayer(args[0], out Player player))
                player.Client.Disconnect(true);
        }

        public static void HandleStatus(Client fromClient, string[] args)
        {
            if (args.Length < 2) 
                return;

            if (!CharacterManager.Instance.TryGetOnlinePlayer(args[0], out Player player)) 
                return;
            
            switch (args[1])
            {
                case "poison":
                case "venom":
                    player.Flags.IsEnvenomed = true;
                    break;
                case "bleed":
                    player.Flags.BleedingTicksRemaining = 10;
                    break;
            }
        }

        public static void HandleGetQuest(Client fromClient, string[] args)
        {
            var player = fromClient.ClientGameData.Player;
            if (!player) return;
            
            if (args.Length < 1 || !QuestId.TryParse(args[0], out var questId))
                return;

            if (!QuestManager.AssignQuestToPlayer(questId, player))
                PacketSender.ConsoleMessageToPlayer(player.Id, "Invalid quest id!", ConsoleMessage.Warning);
        }

        public static void HandleCompleteQuest(Client fromClient, string[] args)
        {
            var player = fromClient.ClientGameData.Player;
            if (!player) return;
            
            if (args.Length < 1 || !QuestId.TryParse(args[0], out var questId))
                return;

            QuestManager.TryCompleteQuest(questId, player);
        }

        public static async void HandleFindId(Client fromClient, string[] args)
        {
            var player = fromClient.ClientGameData.Player;
            if (!player) return;
            
            if (args.Length < 2)
                return;
            
            string table;
            string elementName = string.Join(" ", args.Skip(1));

            switch (args[0].ToLower())
            {
                case "item":
                    table = DatabaseOperations.ITEMS_TABLE;
                    break;
                case "npc":
                    table = DatabaseOperations.NPCS_TABLE;
                    break;
                case "quest":
                    table = DatabaseOperations.QUESTS_TABLE;
                    break;
                case "spell":
                    table = DatabaseOperations.SPELLS_TABLE;
                    break;
                default:
                    PacketSender.ConsoleMessageToPlayer(player.Id, "Invalid element type", ConsoleMessage.DefaultMessage);
                    return;
            }
            
            var (status, id) = await DatabaseOperations.FetchIdByName(table, elementName);

            string message = status != DatabaseResultStatus.Ok
                ? $"{elementName}'s id was not found in the database"
                : $"{elementName}'s id is {id}";
            
            PacketSender.ConsoleMessageToPlayer(player.Id, message, ConsoleMessage.DefaultMessage);
        }

        public static void HandleReviveCommand(Client fromClient, string[] _)
        {
            var player = fromClient.ClientGameData.Player;
            if (player)
            {
                player.Revive();
                player.Health.Heal(int.MaxValue);
            }
        }

        public static void HandleKms(Client fromClient, string[] _)
        {
            var player = fromClient.ClientGameData.Player;
            if (player)
                player.Die();
        }

        public static void HandleDescription(Client fromClient, string[] args)
        {
            var player = fromClient.ClientGameData.Player;
            if (player && args.Length > 0)
                CharacterManager.Instance.TryChangeDescription(player, string.Join(' ', args));
        }
    }
}