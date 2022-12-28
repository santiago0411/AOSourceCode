using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AO.Core.Database;
using AO.Core.Ids;
using AO.Core.Logging;
using UnityEngine;
using Newtonsoft.Json;
using AO.Core.Utils;
using AO.Items;
using AO.Network;
using AO.Players;
using AO.Players.Utils;
using AO.Spells;
using AO.Systems.Mailing;
using Attribute = AO.Players.Attribute;

namespace AO.Core
{
    public sealed class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance { get; private set; }

        private static readonly LoggerAdapter log = new(typeof(CharacterManager));
        private static readonly Regex nameRegex = new("^[a-zA-Z]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(20));
        private static readonly Regex descriptionRegex = new("^[a-zA-Z0-9 .,-_!?¡¿]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(20));
        
        public ReadOnlyDictionary<Attribute, byte> BaseAttributesValues { get; private set; }
        public ReadOnlyDictionary<ClassType, Class> Classes { get; private set; }
        public ReadOnlyDictionary<RaceType, Race> Races { get; private set; }
        public ReadOnlyDictionary<byte, PlayerLevelInfo> Levels { get; private set; }

        private readonly Dictionary<CharacterId, Player> onlinePlayers = new();

        [SerializeField] private GameObject playerPrefab;

        public static int SavingPlayersCount => playersToSave.Count;
        private static readonly Queue<PlayerSaveData> playersToSave = new();

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

        private void Start()
        {
            InvokeRepeating(nameof(SavePlayerCoroutine), 1f, 0.5f);
        }

        public async void SavePlayerCoroutine()
        {
            if (!DatabaseManager.DatabaseActive)
            {
                DatabaseManager.DatabaseActive = await DatabaseOperations.TestConnection();
                if (!DatabaseManager.DatabaseActive)
                    return;
                
                NetworkManager.AcceptingClients = true;
                log.Info("Database connection has been reestablished. Server is now accepting clients again.");
            }

            if (playersToSave.Count > 0)
                await SaveNextPlayer();
        }

        public static void EnqueuePlayerToSave(Player player)
        {
            foreach (var data in playersToSave)
                if (data.CharId == player.CharacterInfo.CharacterId)
                {
                    data.Update(player);
                    return;
                }
            
            playersToSave.Enqueue(new PlayerSaveData(player));
        }

        public static bool IsSavingPlayer(CharacterId charId)
        {
            foreach (var data in playersToSave)
                if (data.CharId == charId)
                    return true;

            return false;
        }

        public Player InstantiatePlayerPrefab(GameMasterRank? gameMasterRank)
        {
            var prefabInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            return gameMasterRank is null
                ? prefabInstance.AddComponent<Player>()
                : prefabInstance.AddComponent<GameMaster>();
        }

        public void ForeachOnlinePlayer(Action<Player> action)
        {
            foreach (var player in onlinePlayers.Values)
                action(player);
        }

        public void AddOnlinePlayer(Player player)
        {
            onlinePlayers.Add(player.CharacterInfo.CharacterId, player);
        }

        public void RemoveOnlinePlayer(Player player)
        {
            onlinePlayers.Remove(player.CharacterInfo.CharacterId);
        }

        public bool TryGetOnlinePlayer(CharacterId characterId, out Player player)
        {
            return onlinePlayers.TryGetValue(characterId, out player);
        }

        public bool TryGetOnlinePlayer(string characterName, out Player player)
        {
            foreach (var p in onlinePlayers.Values)
                if (p.Username.Equals(characterName, StringComparison.OrdinalIgnoreCase))
                {
                    player = p;
                    return true;
                }

            player = null;
            return false;
        }
        
        public void SendAllPlayersToNewPlayer(ClientId newClientId)
        {
            // Send all players to the new player
            foreach (Player player in onlinePlayers.Values)
            {
                if (player.Id == newClientId) 
                    continue;
                
                PacketSender.SpawnPlayer(newClientId, player);

                if (player.NearbyPlayers.Contains(newClientId))
                    PacketSender.PlayerRangeChanged(newClientId, player.Id, true);
            }
        }

        public void SendNewPlayerToAllPlayers(Player newPlayer)
        {
            // Send the new player to all players (including themselves)
            foreach (Player player in onlinePlayers.Values)
            {
                PacketSender.SpawnPlayer(player.Id, newPlayer);
                
                if (newPlayer.NearbyPlayers.Contains(player.Id))
                    PacketSender.PlayerRangeChanged(player.Id, newPlayer.Id, true);
            }
        }

        public void TryChangeDescription(Player player, string description)
        {
            if (description.Length > Constants.PLAYER_MAX_DESC_LENGTH)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.DescriptionTooLong);
                return;
            }
            
            try
            {
                if (descriptionRegex.IsMatch(description))
                {
                    player.Description = description;
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.DescriptionChanged);
                    PacketSender.PlayerDescriptionChanged(player);
                    return;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                log.Warn("Player {0} regex for description '{1}' timed out.", player.Username, player.Description);
            }
            
            PacketSender.SendMultiMessage(player.Id, MultiMessage.DescriptionInvalid);
        }

        public static InventorySlot[] ConvertJsonToInventory(string jsonString)
        {
            var aux = JsonConvert.DeserializeObject<List<object[]>>(jsonString);
            AoDebug.Assert(aux is not null);
            
            InventorySlot[] inventory = new InventorySlot[Constants.PLAYER_INV_SPACE];
            foreach (var item in aux!)
            {
                byte slot = Convert.ToByte(item[0]);
                Item o = GameManager.Instance.GetItem(Convert.ToUInt16(item[1]));
                inventory[slot] = new InventorySlot(slot, o, Convert.ToUInt16(item[2]), Convert.ToBoolean(item[3]));
            }

            return inventory;
        }

        public static void ConvertJsonToSpells(string jsonString, Spell[] spells)
        {
            var aux = JsonConvert.DeserializeObject<List<object[]>>(jsonString);
            AoDebug.Assert(aux is not null);    
            
            foreach (var item in aux!)
            {
                int slot = Convert.ToInt32(item[0]);
                ushort spellIndex = Convert.ToUInt16(item[1]);
                spells[slot] = GameManager.Instance.GetSpell(spellIndex);
            }
        }
        
        public static string ConvertInventoryToJson(Player player)
        {
            var toSerialize = player.Inventory.Where(s => s is not null)
                .Select(slot => new object[] {slot.Slot, slot.Item.Id, slot.Quantity, Convert.ToByte(slot.Equipped)});
            
            return JsonConvert.SerializeObject(toSerialize);
        }

        public static string ConvertSpellsToJson(Player player)
        {
            var toSerialize = player.Spells.Where(s => s is not null)
                .Select(spell => new object[] {Array.IndexOf(player.Spells, spell), spell.Id});
            
            return JsonConvert.SerializeObject(toSerialize);
        }

        public static string GetNewbieInventory(RaceType race, Gender gender, bool isMagicClass)
        {
            string inventory = Constants.INVENTORY_MALE_TALL_MAGIC;

            switch (race)
            {
                case RaceType.Human:
                case RaceType.Elf:
                case RaceType.NightElf:
                    inventory = gender switch
                    {
                        Gender.Female => isMagicClass
                            ? Constants.INVENTORY_FEMALE_TALL_MAGIC
                            : Constants.INVENTORY_FEMALE_TALL_NOT_MAGIC,
                        Gender.Male => isMagicClass
                            ? Constants.INVENTORY_MALE_TALL_MAGIC
                            : Constants.INVENTORY_MALE_TALL_NOT_MAGIC,
                        _ => inventory
                    };
                    break;
                case RaceType.Dwarf:
                case RaceType.Gnome:
                    inventory = gender switch
                    {
                        Gender.Female => isMagicClass
                            ? Constants.INVENTORY_FEMALE_SHORT_MAGIC
                            : Constants.INVENTORY_FEMALE_SHORT_NOT_MAGIC,
                        Gender.Male => isMagicClass
                            ? Constants.INVENTORY_MALE_SHORT_MAGIC
                            : Constants.INVENTORY_MALE_SHORT_NOT_MAGIC,
                        _ => inventory
                    };
                    break;
            }

            return inventory;
        }

        public CharacterInitialResources CalculateInitialResourcesValues(ClassType @class, RaceType race)
        {
            ushort mana;
            switch (@class)
            {
                //Mana
                case ClassType.Mage:
                {
                    byte intellect = Races[race].Attributes[Attribute.Intellect];
                    mana = (ushort)(intellect * 3);
                    break;
                }
                case ClassType.Druid:
                case ClassType.Cleric:
                case ClassType.Bard:
                case ClassType.Paladin:
                case ClassType.Assassin:
                    mana = 50;
                    break;
                default:
                    mana = 0;
                    break;
            }
            
            var initialResources = new CharacterInitialResources
            {
                Health = 20,
                Mana = mana,
                Stamina = 40, // Add modifier per class?
                Hunger = 100,
                Thirst = 100
            };
            
            return initialResources;
        }

        public async Task<bool> ValidateSentCharacterValues(CharacterCreationSpec characterSpec)
        {
            var client = characterSpec.Client;
            var characterName = characterSpec.CharacterName;

            bool nameIsValid = false;
            
            try
            {
                //TODO check for blacklisted name
                nameIsValid = !string.IsNullOrEmpty(characterName) && nameRegex.IsMatch(characterName);
            }
            catch (RegexMatchTimeoutException)
            {
            }
            
            if (!nameIsValid)
            {
                PacketSender.CreateCharacterReturn(client.Id, CreateCharacterMessage.InvalidName);
                return false;
            }

            var message = await DatabaseOperations.IsCharacterNameAvailable(characterName, characterSpec.Transaction, characterSpec.CancellationTokenSource.Token);

            if (message != CreateCharacterMessage.Ok)
            {
                PacketSender.CreateCharacterReturn(client.Id, message);
                return false;
            }

            if (characterSpec.Class is < 1 or >= (byte)ClassType.Length)
            {
                client.Disconnect();
                return false; //Edited packet
            }

            if (characterSpec.Race is < 1 or >= (byte)RaceType.Length)
            {
                client.Disconnect();
                return false; //Edited packet
            }

            if (characterSpec.Gender != 0 && characterSpec.Gender != 1)
            {
                client.Disconnect();
                return false; //Edited packet
            }

            var heads = characterSpec.Gender == 0
                ? Races[(RaceType)characterSpec.Race].FemaleHeads
                : Races[(RaceType)characterSpec.Race].MaleHeads;

            if (!heads.Contains(characterSpec.HeadId))
            {
                client.Disconnect();
                return false; //Edited packet
            }

            short aux = 0;
            foreach (var (key, value) in characterSpec.Skills)
            {
                // If skill doesn't exist
                if ((byte)key >= (byte)Skill.Length)
                {
                    client.Disconnect();
                    return false; //Edited packet
                }
                
                aux += value;
            }

            if (aux > 10)
            {
                client.Disconnect();
                return false; //Edited packet
            }
            
            if (aux < 10)
            {
                PacketSender.CreateCharacterReturn(client.Id, CreateCharacterMessage.NotAllSkillsAreAssigned);
                return false;
            }

            return true;
        }

        public async Task LoadAttributesBaseValues(bool closeOnError)
        {
            var attributes = await DatabaseOperations.FetchAttributes(closeOnError);
            BaseAttributesValues = new ReadOnlyDictionary<Attribute, byte>(attributes);
            log.Info("Successfully loaded attributes base values.");
        }

        public async Task LoadClasses(bool closeOnError)
        {
            var classes = await DatabaseOperations.FetchClasses(closeOnError);
            Classes = new ReadOnlyDictionary<ClassType, Class>(classes);
            log.Info("Successfully loaded classes.");
        }

        public async Task LoadRaces(bool closeOnError)
        {
            var races = await DatabaseOperations.FetchRaces(closeOnError);
            Races = new ReadOnlyDictionary<RaceType, Race>(races);
            log.Info("Successfully loaded races.");
        }

        public async Task LoadLevels(bool closeOnError)
        {
            var levels = await DatabaseOperations.FetchLevels(closeOnError);
            Levels = new ReadOnlyDictionary<byte, PlayerLevelInfo>(levels);
            log.Info("Successfully loaded levels.");
        }
        
        private static async Task SaveNextPlayer()
        {
            var data = playersToSave.Dequeue();
            var result = await BeginSavingPlayer(data);
            if (!result)
                OnSavingPlayerFailed(data);
            else
                log.Debug("Successfully saved player {0} ({1}) to database.", data.Name, data.CharId);
        }

        private static async Task<bool> BeginSavingPlayer(PlayerSaveData playerSaveData)
        {
            Transaction transaction = await DatabaseManager.BeginTransactionAsync();
            if (transaction is null)
                return false;
            
            var status = await DatabaseOperations.UpdateCharacter(playerSaveData, transaction);
            if (status != DatabaseResultStatus.Ok)
                return false;
            
            // If there is not mail to update or delete, commit and return
            if (playerSaveData.MailsToUpdate.Count == 0 && playerSaveData.MailsToDelete.Count == 0)
                return await transaction.CommitTransactionAsync();

            bool success = await MailManager.UpdateAndDeletePlayerMails(playerSaveData.MailsToUpdate, playerSaveData.MailsToDelete, transaction);
            if (!success)
                return false;

            return await transaction.CommitTransactionAsync();
        }

        private static void OnSavingPlayerFailed(PlayerSaveData playerSaveData)
        {
            log.Warn("Failed to save player to database. Server is now not accepting any new connections.");
            NetworkManager.AcceptingClients = false;
            DatabaseManager.DatabaseActive = false;
            // TODO send notification that the db has failed
            playersToSave.Enqueue(playerSaveData);
        }
    }
}