using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using AOClient.Core.Utils;
using AOClient.Network;
using UnityEngine;
using Newtonsoft.Json;
using AOClient.Npcs;
using AOClient.Player;
using AOClient.Player.Utils;
using AOClient.Questing;

namespace AOClient.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static bool GameManagerLoaded { get; private set; }

        public PlayerManager LocalPlayer { get; private set; }
        public Pool<Npc> NpcsPool { get; private set; }
        
        [SerializeField] private PlayerManager playerPrefab;
        [SerializeField] private Npc npcPrefab;
        [SerializeField] private WorldItem worldItemPrefab;
        
        private Sprite missingSprite;

        private Dictionary<ItemId, Item> items;
        private Dictionary<NpcId, NpcInfo> npcsInfo;
        private Dictionary<SpellId, Spell> spells;
        private Dictionary<QuestId, Quest> quests;

        private Pool<WorldItem> worldItems;

        private readonly Dictionary<ClientId, PlayerManager> players = new();
        
        private readonly Dictionary<short, Map> worldMaps = new();
        private readonly Dictionary<ushort, Sprite> itemGraphics = new();
        private readonly Dictionary<ushort, Sprite> npcsBodies = new();
        private readonly Dictionary<ushort, RuntimeAnimatorController> armorAnims = new();
        private readonly Dictionary<ushort, RuntimeAnimatorController> shieldAnims = new();
        private readonly Dictionary<ushort, RuntimeAnimatorController> weaponAnims = new();
        private readonly Dictionary<ushort, RuntimeAnimatorController> npcsAnims = new();
        private readonly Dictionary<byte, Sprite[]> headSprites = new();
        private readonly Dictionary<ushort, Sprite[]> helmSprites = new();
        private readonly Dictionary<ushort, ParticleSystem> particles = new();

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
        }

        public void Initialize()
        {
            // If it's loaded it means the player reconnected back into the world and a valid GameManager instance already exists
            if (GameManagerLoaded)
            {
                InitAlways();
                return;
            }
            
            InitAlways();
            FirstTimeInitialization();
            DebugLogger.Info("Successfully finished GameManager initialization.");
        }

        private void InitAlways()
        {
            // Set it back to false to await coroutine
            GameManagerLoaded = false;
            StartCoroutine(DestroyTilemap());
            LoadMaps();
            InitPools();
        }

        private void FirstTimeInitialization()
        {
            LoadGraphics();
            LoadItems();
            LoadSpells();
            LoadNpcs();
            LoadParticles();
            LoadQuests();
        }

        private static IEnumerator DestroyTilemap()
        {
            var tilemap = GameObject.Find("ObstaclesTreesDoors").GetComponent<UnityEngine.Tilemaps.Tilemap>();
            var nonNullTilesCount = tilemap.GetTilesBlock(tilemap.cellBounds).Count(x => x);

            while (Obstacle.SpawnedObstacles < nonNullTilesCount)
                yield return new WaitForFixedUpdate();
            
            Destroy(tilemap.gameObject);
            Obstacle.ResetSpawnsCount();
            GameManagerLoaded = true;
        }

        public PlayerManager GetPlayer(ClientId playerClientId) => players[playerClientId];
        public bool TryGetPlayer(ClientId playerClientId, out PlayerManager player) => players.TryGetValue(playerClientId, out player);
        public void RemovePlayer(ClientId playerClientId) => players.Remove(playerClientId);
        
        public void SpawnPlayer(ref SpawnPlayerInfo info)
        {
            if (players.ContainsKey(info.ClientId))
                return;
            
            bool isLocalPlayer = info.ClientId == Client.Instance.MyId;
            PlayerManager player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            ChangedMap(player, info.MapNumber);
            
            if (isLocalPlayer)
            {
                LocalPlayer = player;
                player.gameObject.AddComponent<PlayerController>();

                var mainCamera = Camera.main;
                if (mainCamera)
                {
                    mainCamera.transform.parent = player.transform;
                    mainCamera.rect = new Rect(0.013f, 0.052f, 0.683f, 0.7f);
                }
            }

            player.Initialize(ref info, isLocalPlayer);
            players.Add(info.ClientId, player);

            if (!isLocalPlayer)
                player.gameObject.SetActive(false);
        }
        
        public void ChangedMap(PlayerManager player, short mapNumber)
        {
            player.transform.SetParent(worldMaps[mapNumber].transform);
            player.CurrentMap = worldMaps[mapNumber];
        }

        public void CreateWorldItem(int instanceId, ItemId itemId, short mapNumber, Vector2 mapPosition)
        {
            //Instantiate the item in the world
            WorldItem worldItem = worldItems.GetObject();

            //Find the item according to the received Id
            Item itemInfo = items[itemId];
            worldItem.Initialize(instanceId, GetSprite(itemInfo.GraphicId));

            //Move the item to the proper world map and position
            var worldItemTransform = worldItem.transform;
            worldItemTransform.parent = worldMaps[mapNumber].transform;
            worldItemTransform.position = mapPosition;
        }

        public void DestroyWorldItem(int instanceId)
        {
            worldItems.FindObject(instanceId).ResetPoolObject();
        }

        public void CreateNpc(NpcId npcId, int instanceId, short mapNumber, Vector2 position)
        {
            NpcInfo info = npcsInfo[npcId];
            Npc npc = NpcsPool.GetObject();
            npc.Initialize(instanceId, info, mapNumber, position);
        }

        public void CreateParticle(ushort particleId, Vector2 position, Transform transformParent)
        {
            //TODO change to use real particleID
            ParticleSystem fx = Instantiate(particles[6], position, Quaternion.identity, transformParent);
            Destroy(fx.gameObject, 1f);
        }

        public Item GetItem(ItemId itemId) => items[itemId];
        public Spell GetSpell(SpellId spellId) => spells[spellId];
        public NpcInfo GetNpcInfo(NpcId npcId) => npcsInfo[npcId];
        public Map GetWorldMap(short mapId) => worldMaps[mapId];
        
        public Sprite GetSprite(ushort spriteId)
        {
            if (itemGraphics.TryGetValue(spriteId, out Sprite sprite))
                return sprite;

            return missingSprite;
        }

        public Sprite GetSpriteByItemId(ItemId itemId)
        {
            if (items.TryGetValue(itemId, out Item item))
                return GetSprite(item.GraphicId);

            return missingSprite;
        }

        public Sprite[] GetHeadSprites(byte headSpriteId) 
        { 
            if (headSprites.TryGetValue(headSpriteId, out Sprite[] sprites))
                return sprites;

            return headSprites[1];
        }

        public Sprite[] GetHelmSprites(ushort helmSpriteId) 
        {
            if (helmSprites.TryGetValue(helmSpriteId, out Sprite[] sprites))
                return sprites;

            return helmSprites[1];
        }

        public RuntimeAnimatorController GetArmorAnimation(ushort id) 
        {
            if (armorAnims.TryGetValue(id, out RuntimeAnimatorController anim))
                return anim;

            return armorAnims[1];
        }

        public RuntimeAnimatorController GetShieldAnimation(ushort id)
        {
            shieldAnims.TryGetValue(id, out RuntimeAnimatorController anim);
            return anim;
        }

        public RuntimeAnimatorController GetWeaponAnimation(ushort id)
        {
            weaponAnims.TryGetValue(id, out RuntimeAnimatorController anim);
            return anim;
        }
    
        public RuntimeAnimatorController GetNpcAnimation(ushort id) 
        {
            if (npcsAnims.TryGetValue(id, out RuntimeAnimatorController anim))
                return anim;

            return npcsAnims[1];
        }

        public Sprite GetNpcBody(ushort id) 
        {
            if (npcsBodies.TryGetValue(id, out Sprite sprite))
                return sprite;
            
            return npcsBodies[1];
        }

        public ParticleSystem GetParticles(ushort id) 
        {
            if (particles.TryGetValue(id, out ParticleSystem particle))
                return particle;

            return particles[1];
        }

        public Quest GetQuest(QuestId id)
        {
            if (quests.TryGetValue(id, out Quest quest))
                return quest;

            return null;
        }

        private void LoadMaps()
        {
            worldMaps.Clear();
            var maps = FindObjectsOfType<Map>();

            foreach (var map in maps)
                worldMaps[map.Number] = map;
        }

        private void InitPools()
        {
            worldItems = new Pool<WorldItem>(worldItemPrefab);
            NpcsPool = new Pool<Npc>(npcPrefab);
        }

        /// <summary>Loads all the sprites and animations from resources folder.</summary>
        private void LoadGraphics()
        {
            try
            {
                //Default sprite if something is missing
                missingSprite = Resources.Load<Sprite>("Graphics/MissingSprite");

                //Load all icons for items
                var itemSprites = Resources.LoadAll<Sprite>("Graphics/Items");

                //Order them by ID (its name)
                foreach (var sprite in itemSprites)
                {
                    ushort.TryParse(sprite.name.Split('_')[0], out ushort id);
                    itemGraphics.Add(id, sprite);
                }

                //Load all armor/body animations
                var armorAnimations = Resources.LoadAll<RuntimeAnimatorController>("Graphics/Animations/Armors");

                //Order them by ID (its name)
                foreach (var anim in armorAnimations)
                {
                    ushort.TryParse(anim.name.Split('_')[0], out ushort id);
                    armorAnims.Add(id, anim);
                }

                //Load all shields animations
                var shieldAnimations = Resources.LoadAll<RuntimeAnimatorController>("Graphics/Animations/Shields");

                //Order them by ID (its name)
                foreach (var anim in shieldAnimations)
                {
                    ushort.TryParse(anim.name.Split('_')[0], out ushort id);
                    shieldAnims.Add(id, anim);
                }

                //Load all weapons animations
                var weaponAnimations = Resources.LoadAll<RuntimeAnimatorController>("Graphics/Animations/Weapons");

                //Order them by ID (its name)
                foreach (var anim in weaponAnimations)
                {
                    ushort.TryParse(anim.name.Split('_')[0], out ushort id);
                    weaponAnims.Add(id, anim);
                }

                //Load all npcs animations
                var npcAnimations = Resources.LoadAll<RuntimeAnimatorController>("Graphics/Animations/Npc");

                //Order them by ID (its name)
                foreach (var anim in npcAnimations)
                {
                    ushort.TryParse(anim.name.Split('_')[0], out ushort id);
                    npcsAnims.Add(id, anim);
                }

                //Load all heads sprites
                var heads = Resources.LoadAll<Sprite>("Graphics/Heads");

                //Parse their names to get the ID and create arrays of all 4 sprites
                foreach (var head in heads)
                {
                    byte.TryParse(head.name.Split('_')[0], out byte id);

                    if (!headSprites.ContainsKey(id))
                    {
                        var headArray = heads.Where(x => x.name.Split('_')[0].Equals(head.name.Split('_')[0])).ToArray();
                        headSprites.Add(id, headArray);
                    }
                }

                //Load all helms sprites
                var helms = Resources.LoadAll<Sprite>("Graphics/Helms");

                //Parse their names to get the ID and create arrays of all 4 sprites
                foreach (var helm in helms)
                {
                    ushort.TryParse(helm.name.Split('_')[0], out ushort id);

                    if (!helmSprites.ContainsKey(id))
                    {
                        var helmArray = helms.Where(x => x.name.Split('_')[0].Equals(helm.name.Split('_')[0])).ToArray();
                        helmSprites.Add(id, helmArray);
                    }
                }

                //Load static npcs bodies (sprites)
                var npcsBodySprites = Resources.LoadAll<Sprite>("Graphics/Bodies");

                //Order them by ID (its name)
                foreach (var sprite in npcsBodySprites)
                {
                    if (sprite.name.Split('_')[1].Equals("0"))
                    {
                        ushort.TryParse(sprite.name.Split('_')[0], out ushort id);
                        npcsBodies.Add(id, sprite);
                    }
                }
                
                DebugLogger.Debug("Successfully loaded all graphics.");
            }
            catch (Exception ex)
            {
                DebugLogger.Error($"Error loading graphics: {ex}");
            }
        }

        private void LoadItems()
        {
            try
            {
                var textAsset = Resources.Load<TextAsset>("Items");
                var tmp = JsonConvert.DeserializeObject<Item[]>(textAsset.text);
                items = tmp!.ToDictionary(i => i.Id);
                DebugLogger.Debug("Successfully loaded items.");
            }
            catch (Exception ex)
            {
                DebugLogger.Error($"Error loading items: {ex}");
            }
        }

        private void LoadSpells()
        {
            try
            {
                var textAsset = Resources.Load<TextAsset>("Spells");
                var tmp = JsonConvert.DeserializeObject<Spell[]>(textAsset.text);
                spells = tmp!.ToDictionary(s => s.Id);
                DebugLogger.Debug("Successfully loaded spells.");
            }
            catch (Exception ex)
            {
                DebugLogger.Error($"Error loading spells: {ex}");
            }
        }

        private void LoadNpcs()
        {
            try
            {
                var textAsset = Resources.Load<TextAsset>("Npcs");
                var tmp = JsonConvert.DeserializeObject<NpcInfo[]>(textAsset.text);
                npcsInfo = tmp!.ToDictionary(n => n.Id);
                DebugLogger.Debug("Successfully loaded npcs.");
            }
            catch (Exception ex)
            {
                DebugLogger.Error($"Error loading npcs: {ex}");
            }
        }

        private void LoadParticles()
        {
            try
            {
                var tmp = Resources.LoadAll<ParticleSystem>("Particles");

                foreach (var particle in tmp)
                {
                    ushort.TryParse(particle.name.Split('(')[1].Split(')')[0], out ushort id);
                    particles.Add(id, particle);
                }
                
                DebugLogger.Debug("Successfully loaded particles.");
            }
            catch (Exception ex)
            {
                DebugLogger.Error($"Error loading particles: {ex}");
            }
        }

        private void LoadQuests()
        {
            try
            {
                var textAsset = Resources.Load<TextAsset>("Quests");
                var jss = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All, 
                    Converters = new List<JsonConverter>()
                    {
                        new ItemId.ItemIdJsonConverter(),
                        new NpcId.NpcIdJsonConverter(),
                        new SpellId.SpellIdJsonConverter(),
                        new QuestId.QuestIdJsonConverter(),
                    }
                };
                var tmp = JsonConvert.DeserializeObject<Quest[]>(textAsset.text, jss);
                quests = tmp!.ToDictionary(q => q.Id);
                DebugLogger.Debug("Successfully loaded quests.");
            }
            catch (Exception ex)
            {
                DebugLogger.Error($"Error loading quests: {ex}");
            }
        }
    }
}
