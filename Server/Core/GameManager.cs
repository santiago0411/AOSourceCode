using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using AO.Core.Database;
using AO.Core.Ids;
using AO.Core.Logging;
using AO.Core.Utils;
using AO.Items;
using AO.Network;
using AO.Npcs;
using AO.Npcs.Utils;
using AO.Players;
using AO.Spells;
using AO.Systems.Professions;
using AO.Systems.Questing;
using AO.World;
using PacketSender = AO.Network.PacketSender;

[assembly: System.Reflection.AssemblyVersion("0.6.0")]
namespace AO.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static bool GameMangerLoaded { get; private set; }

        private static readonly LoggerAdapter log = new(typeof(GameManager));

        private Pool<WorldItem> worldItemsPool;
        private Pool<Npc> npcsPool;
        
        private readonly Dictionary<short, Map> worldMaps = new();
        private readonly Dictionary<ItemId, Item> items = new();
        private readonly Dictionary<SpellId, Spell> spells = new();
        private readonly Dictionary<NpcId, NpcInfo> npcsInfo = new();

        [SerializeField] private WorldItem worldItemPrefab;
        [SerializeField] private Npc npcPrefab;
        
        private void Awake()
        {
            if (Instance is null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else if (Instance != this)
            {
                gameObject.SetActive(false);
                Destroy(this);
            }
            
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 30;
        }

        private async void Start()
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            log.Info("Starting AO Server version {0}...", version);
            
            StartCoroutine(DestroyTilemap());
            
            AoDebug.BeingTimer(Instance.GetInstanceID());
            
            WorldMap.LoadTiles();
            LoadWorldMapsAndPools();
            
            await DatabaseManager.Init();
            await Constants.Load(true);
            await LoadItems();
            await LoadSpells();
            await LoadNpcs();
            await QuestManager.LoadQuests();
            await DatabaseOperations.FetchAndLoadCraftableItems();
            await CharacterManager.Instance.LoadAttributesBaseValues(true);
            await CharacterManager.Instance.LoadClasses(true);
            await CharacterManager.Instance.LoadRaces(true);
            await CharacterManager.Instance.LoadLevels(true);
            
            NetworkManager.Instance.InitializeServer();
            
            GameMangerLoaded = true;
            log.Info("GameManager start-up successful.");
            
            AoDebug.EndTimer<GameManager>(Instance.GetInstanceID());
        }
        
        public static void CloseApplication()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private static IEnumerator DestroyTilemap()
        {
            var tilemapObstacles = WorldMap.ObstaclesRoofsTreesMap;
            var nonNullTilesCount = tilemapObstacles.GetTilesBlock(tilemapObstacles.cellBounds).Count(x => x);

            while (Obstacle.SpawnedObstacles < nonNullTilesCount)
                yield return new WaitForFixedUpdate();

            #if UNITY_EDITOR
            Destroy(tilemapObstacles.gameObject);
            Destroy(WorldMap.RoofsMap.gameObject);
            #else
            Destroy(tilemapObstacles.transform.parent.gameObject);
            #endif
        }

        public Npc SpawnNpc(NpcInfo npcInfo, Map map, Vector2 position)
        {
            Npc npc = npcsPool.GetObject();
            npc.Spawn(map, position, npcInfo);
            return npc;
        }

        public void AddSceneNpcToPool(Npc npc)
        {
            npcsPool.AddExistingObjectToPool(npc);
        }

        /// <summary>Creates a world item and its quantity in the specified map and position.</summary>
        /// <returns>Returns false if it couldn't find an empty position and the item wasn't created.</returns>
        public bool CreateWorldItem(Item item, ushort quantity, Vector2 mapPosition)
        {
            // If it can't find an empty position the item won't be created
            if (!WorldMap.FindEmptyTileForItem(mapPosition, out var tile))
                return false;

            // Get an item from the pool and initialize it
            WorldItem worldItem = worldItemsPool.GetObject();
            worldItem.Initialize(item.Id, item.Name, item.Grabbable, quantity, tile);
            return true;
        }

        public Item GetItem(ItemId id)
        {
            AoDebug.Assert(items.ContainsKey(id), $"Item {id} was not found.");
            // First() should never happen but just in case someone edits a wrong value in database
            return items.TryGetValue(id, out var item) ? item : items.Values.First();
        }

        public Spell GetSpell(SpellId id)
        {
            AoDebug.Assert(spells.ContainsKey(id), $"Spell {id} was not found.");
            // First() should never happen but just in case someone edits a wrong value in database
            return spells.TryGetValue(id, out var spell) ? spell : spells.Values.First();
        }

        public bool TryGetMap(short id, out Map map)
        {
            return worldMaps.TryGetValue(id, out map);
        }
        
        public Map GetMap(short id)
        {
            AoDebug.Assert(worldMaps.ContainsKey(id), $"World map {id} was not found.");
            // First() should never happen but just in case someone edits a wrong value in database
            return worldMaps.TryGetValue(id, out var map) ? map : worldMaps.Values.First();
        }
        
        public NpcInfo GetNpcInfo(NpcId id)
        {
            AoDebug.Assert(npcsInfo.ContainsKey(id), $"Npc {id} was not found.");
            // First() should never happen but just in case someone edits a wrong value in database
            return npcsInfo.TryGetValue(id, out var info) ? info : npcsInfo.Values.First();
        }

        public void RemovePlayerFromMap(Player player)
        {
            worldMaps[player.CurrentMap.Number].PlayersInMap.Remove(player);
        }

        public void MovePlayerIntoNewMap(Player player, Map map)
        {
            worldMaps[player.CurrentMap.Number].PlayersInMap.Remove(player);
            player.transform.parent = worldMaps[map.Number].transform;
            worldMaps[map.Number].PlayersInMap.Add(player);
        }

        public void SendDoorStatesInCurrentMap(Player toPlayer)
        {
            var doors = toPlayer.CurrentMap.Doors;
            AoDebug.Assert(doors.Count <= 256); // Just to know if possibly stack overflow could happen 
            Span<Vector2> positions = stackalloc Vector2[doors.Count];
            Span<bool> states = stackalloc bool[doors.Count];

            for (var i = 0; i < doors.Count; i++)
            {
                Door door = doors[i];
                positions[i] = door.Position;
                states[i] = door.State;
            }
            
            PacketSender.DoorStates(positions, states, toPlayer.Id);
        }

        private void LoadWorldMapsAndPools()
        {
            var maps = FindObjectsOfType<Map>();

            foreach (var map in maps)
                worldMaps.Add(map.Number, map);

            worldItemsPool = new Pool<WorldItem>(worldItemPrefab);
            npcsPool = new Pool<Npc>(npcPrefab);
        }

        private async Task LoadItems()
        {
            foreach (var item in await DatabaseOperations.FetchAllItems())
                items.Add(item.Id, item);

            log.Info("Successfully loaded items.");
        }

        public async Task ReloadItem(ClientId clientRequesterId, string itemIdString)
        {
            var item = await ReloadInternal(clientRequesterId, "item", itemIdString, DatabaseOperations.FetchSingleItem);
            if (item is not null)
                items.AddOrUpdate(item.Id, item);
        }

        private async Task LoadSpells()
        {
            foreach (var spell in await DatabaseOperations.FetchAllSpells())
                spells.Add(spell.Id, spell);

            log.Info("Successfully loaded spells.");
        }
        
        public async Task ReloadSpell(ClientId clientRequesterId, string spellIdString)
        {
            var spell = await ReloadInternal(clientRequesterId, "spell", spellIdString, DatabaseOperations.FetchSingleSpell);
            if (spell is not null)
                spells.AddOrUpdate(spell.Id, spell);
        }

        private async Task LoadNpcs()
        {
            foreach (var npcInfo in await DatabaseOperations.FetchAllNpcsInfo())
            {
                npcsInfo.Add(npcInfo.Id, npcInfo);
                if (npcInfo.TurnInQuests.Count > 0)
                    QuestManager.NpcAddQuestTurnIn(npcInfo.Id, npcInfo.TurnInQuests);
            }

            log.Info("Successfully loaded npcs info.");
        }
        
        public async Task ReloadNpc(ClientId clientRequesterId,string npcIdString)
        {
            var npcInfo = await ReloadInternal(clientRequesterId, "npc", npcIdString, DatabaseOperations.FetchSingleNpcInfo);
            if (npcInfo is not null)
            {
                npcsInfo.AddOrUpdate(npcInfo.Id, npcInfo);
                
                if (npcInfo.TurnInQuests.Count > 0)
                    QuestManager.NpcAddQuestTurnIn(npcInfo.Id, npcInfo.TurnInQuests);
            }
        }

        public async Task ReloadCraftableItem(ClientId clientRequesterId, string craftableIdString)
        {
            var craftableItem = await ReloadInternal(clientRequesterId, "craftable item", craftableIdString, DatabaseOperations.FetchSingleCraftableItem);
            if (craftableItem is null)
                return;
            
            Dictionary<ItemId, CraftableItem> dictionary;
            switch (craftableItem.Profession)
            {
                case CraftingProfession.Blacksmithing:
                    dictionary = CraftingProfessions.BlacksmithingItems;
                    break;
                case CraftingProfession.Woodworking:
                    dictionary = CraftingProfessions.WoodworkingItems;
                    break;
                case CraftingProfession.Tailoring:
                    dictionary = CraftingProfessions.TailoringItems;
                    break;
                default:
                    return;
            }

            dictionary.AddOrUpdate(craftableItem.Item.Id, craftableItem);
            CharacterManager.Instance.ForeachOnlinePlayer(p => p.Flags.CraftableItemsSent.Clear());
        }
        
        public async Task ReloadQuest(ClientId clientRequesterId, string questIdString)
        {
            var quest = await ReloadInternal(clientRequesterId, "quest", questIdString, DatabaseOperations.FetchSingleQuest);
            if (quest is not null)
                QuestManager.ReloadQuest(quest);
        }
        
        private static async Task<T> ReloadInternal<T>(ClientId clientRequesterId, string reloadingElement, string elementIdString, Func<ushort, Task<T>> reloadCallback)
        {
            log.Info($"Reloading {reloadingElement} {elementIdString}.");
            if (!ushort.TryParse(elementIdString, out ushort elementId))
            {
                string msg = $"Failed to parse {reloadingElement} id: '{elementIdString}'.";
                log.Warn(msg);
                #if AO_DEBUG
                PacketSender.BroadcastConsoleMessage(msg, ConsoleMessage.DefaultMessage);
                #else
                PacketSender.ConsoleMessageToPlayer(clientRequesterId, msg, ConsoleMessage.DefaultMessage);
                #endif
                return default;
            }
            
            #if AO_DEBUG
            PacketSender.BroadcastConsoleMessage($"Reloading {reloadingElement} {elementId}.", ConsoleMessage.DefaultMessage);
            #else
            PacketSender.ConsoleMessageToPlayer(clientRequesterId, $"Reloading {reloadingElement} {elementId}.", ConsoleMessage.DefaultMessage);
            #endif

            var element = await reloadCallback(elementId);
            if (element is null)
            {
                string msg = $"Failed to reload {reloadingElement}: {elementId}.";
                log.Warn(msg);
                #if AO_DEBUG
                PacketSender.BroadcastConsoleMessage(msg, ConsoleMessage.DefaultMessage);
                #else
                PacketSender.ConsoleMessageToPlayer(clientRequesterId, msg, ConsoleMessage.DefaultMessage);
                #endif
                
                return default;
            }
            
            #if AO_DEBUG
            PacketSender.BroadcastConsoleMessage($"Successfully reloaded {reloadingElement} {elementId}.", ConsoleMessage.DefaultMessage);
            #else
            PacketSender.ConsoleMessageToPlayer(clientRequesterId, $"Successfully reloaded {reloadingElement} {elementId}.", ConsoleMessage.DefaultMessage);
            #endif
            
            return element;
        }
        
#if UNITY_EDITOR
        [SerializeField] private UnityEngine.UI.Text fpsText;
        private const float REFRESH_TIME = 1f;
        private int frameCounter;
        private float timeCounter, lastFramerate;
        private bool firstTime = true;
        
        private void FixedUpdate()
        {
            if (timeCounter < REFRESH_TIME)
            {
                timeCounter += Time.fixedDeltaTime;
                frameCounter++;
            }
            else
            {
                lastFramerate = frameCounter / timeCounter;
                frameCounter = 0;
                timeCounter = 0.0f;
                firstTime = false;
                fpsText.text = $"TICKS: {lastFramerate:0.00}";
            }
            
            if (!firstTime && lastFramerate < 25 && GameMangerLoaded)
                log.Warn($"TICK RATE DROPPED BELOW 25: {lastFramerate}");
        }
#endif
    }
}
