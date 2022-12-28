using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AO.Core.Ids;
using AO.Core.Logging;
using AO.Core.Utils;
using AO.Items;
using AO.Npcs.Utils;
using AO.Players;
using AO.Players.Utils;
using AO.Spells;
using AO.Systems.Mailing;
using AO.Systems.Professions;
using AO.Systems.Questing;
using SqlKata.Execution;
using Attribute = AO.Players.Attribute;

namespace AO.Core.Database
{
	public static class DatabaseOperations
    {
        public const string ITEMS_TABLE = "items";
        public const string NPCS_TABLE = "npcs";
        public const string QUESTS_TABLE = "quests";
        public const string SPELLS_TABLE = "spells";

        private static readonly LoggerAdapter log = new(typeof(DatabaseOperations));

		public static async Task<DatabaseResult<uint>> FetchAccountId(string accountName, DbConnection connection, CancellationToken token)
        {
            try
            {
                var result = await DatabaseManager.BeginQuery(connection)
                    .From("accounts")
                    .Where("name", accountName)
                    .Select("id")
                    .FirstOrDefaultAsync<uint>(cancellationToken: token);

                return new DatabaseResult<uint>(DatabaseResultStatus.Ok, result);
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return new DatabaseResult<uint>(DatabaseResultStatus.CancellationRequested, default);
                
                log.Error("Error fetching an account id from database. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<uint>(DatabaseResultStatus.OperationFailed, default);
            }
        }

        public static async Task<DatabaseResult<string>> FetchAccountEmail(string accountName, DbConnection connection, CancellationToken token)
        {
            try
            {
                var result = await DatabaseManager.BeginQuery(connection)
                    .From("accounts")
                    .Where("name", accountName)
                    .Select("email")
                    .FirstOrDefaultAsync<string>(cancellationToken: token);
                
                return new DatabaseResult<string>(DatabaseResultStatus.Ok, result);
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return new DatabaseResult<string>(DatabaseResultStatus.CancellationRequested, null);
                
                log.Error("Error fetching an email from database. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<string>(DatabaseResultStatus.OperationFailed, null);
            }
        }

        public static async Task<CreateCharacterMessage> IsCharacterNameAvailable(string characterName, Transaction transaction, CancellationToken token)
        {
            try
            {
                var result = await transaction.BeginQuery()
                    .From("characters")
                    .Where("username", characterName)
                    .Select("id")
                    .FirstOrDefaultAsync<CharacterId?>(transaction.DbTransaction, cancellationToken: token);
                
                return result is null ? CreateCharacterMessage.Ok : CreateCharacterMessage.NameAlreadyInUse;
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                    return CreateCharacterMessage.Error;
                
                log.Error("Error fetching a username from database. {0}\n{1}", ex.Message, ex.StackTrace);
                await transaction.RollbackTransactionAsync();
                DatabaseManager.OnDatabaseOperationFailed();
                return CreateCharacterMessage.Error;
            }
        }

        public static async Task<LoginRegisterMessage> VerifyLoginInfo(string accountName, string passwordHash, DbConnection connection, CancellationToken token)
        {
            try
            {
                var dbPassword = await DatabaseManager.BeginQuery(connection)
                    .From("accounts")
                    .Where("name", accountName)
                    .Select("password")
                    .FirstOrDefaultAsync<string>(cancellationToken: token);

                if (!string.IsNullOrEmpty(dbPassword))
                    return passwordHash.Equals(dbPassword) ? LoginRegisterMessage.LoginOk : LoginRegisterMessage.InvalidAccountOrPassword;

                return LoginRegisterMessage.InvalidAccountOrPassword;
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested) 
                    return LoginRegisterMessage.Error;
                
                log.Error("Error trying to fetch account data from database to login. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return LoginRegisterMessage.Error;
            }
        }

		public static async Task<LoginRegisterMessage> WriteNewAccount(string accountName, string passwordHash, string email, DbConnection connection)
        {
            try
            {
                var result = await DatabaseManager.BeginQuery(connection)
                    .From("accounts")
                    .InsertAsync(new
                    {
                        name = accountName,
                        password = passwordHash,
                        email
                    });
                return result > 0 ? LoginRegisterMessage.RegisterOk : LoginRegisterMessage.Error;
            }
            catch (Exception ex)
            {
                log.Error("Error writing a new account to database. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return LoginRegisterMessage.Error;
            }
        }

        public static async Task<DatabaseResult<CharacterId, string>> FetchCharacterIdAndName(string characterName, DbConnection connection)
        {
            try
            {
                var result = await DatabaseManager.BeginQuery(connection)
                    .From("characters")
                    .Where("username", characterName)
                    .Select("id", "username")
                    .FirstOrDefaultAsync<(CharacterId, string)>();

                return new DatabaseResult<CharacterId, string>(DatabaseResultStatus.Ok, result.Item1, result.Item2);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching character info. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<CharacterId, string>(DatabaseResultStatus.OperationFailed, default, null);
            }
        }

		public static async Task<DatabaseResult<List<AOCharacterInfo>>> FetchCharacters(uint accountId)
        {
            try
            {
                var result = await DatabaseManager.BeginQuery()
                    .From("characters AS c")
                    .Where("account_id", accountId)
                    .LeftJoin("game_masters AS gm", "c.id", "gm.character_id")
                    .Select("c.id AS CharacterId", "c.username AS CharacterName", "gm.rank AS GameMasterRank")
                    .GetAsync<AOCharacterInfo>();

                return new DatabaseResult<List<AOCharacterInfo>>(DatabaseResultStatus.Ok, (List<AOCharacterInfo>)result);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching characters of account: {0} from database. {1}\n{2}", accountId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<List<AOCharacterInfo>>(DatabaseResultStatus.OperationFailed, null);
            }
        }

		public static async Task<DatabaseResult<CharacterId>> WriteNewCharacter(Dictionary<CharactersTableColumn, object> values, Transaction transaction)
        {
            AoDebug.Assert(values.Count >= Queries.CREATE_CHARACTER_QUERY_PARAMS_COUNT, "CreateCharacter list of values had less parameters than the amount required.");

            try
            {
                var newId = await transaction.BeginQuery()
                    .From("characters")
                    .InsertGetIdAsync<CharacterId>(Queries.GetInsertCharacterObject(values), transaction.DbTransaction);
                
                if ((byte)values[CharactersTableColumn.Class] == (byte)ClassType.Worker)
                {
                    await transaction.BeginQuery()
                        .From("characters_worker_talents")
                        .InsertAsync(new { character_id = newId }, transaction.DbTransaction);
                }

                return new DatabaseResult<CharacterId>(DatabaseResultStatus.Ok, newId);
            }
			catch (Exception ex)
            {
				log.Error("Error writing a new character to the database. {0}\n{1}", ex.Message, ex.StackTrace);
                await transaction.RollbackTransactionAsync();
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<CharacterId>(DatabaseResultStatus.OperationFailed, default);
            }
		}

		public static async Task<DatabaseResultStatus> WriteNewbieInventoryAndSpells(string inventory, string spells, CharacterId charId, Transaction transaction)
        {
            try
            {
                await transaction.BeginQuery()
                    .From("characters")
                    .Where("id", charId.AsPrimitiveType())
                    .UpdateAsync(new
                    {
                        inventory,
                        spells
                    }, transaction.DbTransaction);

                return DatabaseResultStatus.Ok;
            }
            catch (Exception ex)
            {
                log.Error("Error writing a newbie inventory and spells to the database. {0}\n{1}", ex.Message, ex.StackTrace);
                await transaction.RollbackTransactionAsync();
                DatabaseManager.OnDatabaseOperationFailed();
                return DatabaseResultStatus.OperationFailed;
            }
        }

        public static async Task<DatabaseResultStatus> WriteTemplateCharacter(CharacterId characterId, int hit, Transaction transaction)
        {
            try
            {
                await transaction.BeginQuery()
                    .From("characters")
                    .Where("id", characterId.AsPrimitiveType())
                    .UpdateAsync(new
                    {
                        level = 45,
                        talent_points = 50,
                        hit,
                        gold = 10000000
                    }, transaction.DbTransaction);

                return DatabaseResultStatus.Ok;
            }
			catch (Exception ex)
			{
                log.Error("Error writing a template character to the database. {0}\n{1}", ex.Message, ex.StackTrace);
                await transaction.RollbackTransactionAsync();
                DatabaseManager.OnDatabaseOperationFailed();
                return DatabaseResultStatus.OperationFailed;
			}
		}

		public static async Task<DatabaseResultStatus> UpdateCharacter(PlayerSaveData playerSaveData, Transaction transaction)
        {
            try
            {
                
                await transaction.BeginQuery()
                    .From("characters")
                    .Where("id", playerSaveData.CharId.AsPrimitiveType())
                    .UpdateAsync(playerSaveData.PlayerData, transaction.DbTransaction);

                if (playerSaveData.WorkerTalentsData is null) 
                    return DatabaseResultStatus.Ok;
                
                await transaction.BeginQuery()
                    .From("characters_worker_talents")
                    .Where("character_id", playerSaveData.CharId.AsPrimitiveType())
                    .UpdateAsync(playerSaveData.WorkerTalentsData, transaction.DbTransaction);

                return DatabaseResultStatus.Ok;
            }
            catch (Exception ex)
            {
                log.Error("Error updating a character to the database. {0}\n{1}", ex.Message, ex.StackTrace);
                await transaction.RollbackTransactionAsync();
                DatabaseManager.OnDatabaseOperationFailed();
                return DatabaseResultStatus.OperationFailed;
            }
        }

		public static async Task<DatabaseResult<IDictionary<string, object>>> FetchCharacterInfo(CharacterId charId)
        {
            try
            {
                var result = await DatabaseManager.BeginQuery()
                    .From("characters AS c")
                    .LeftJoin("characters_worker_talents AS cwt", "c.id", "cwt.character_id")
                    .Where("id", charId.AsPrimitiveType())
                    .Select("*")
                    .FirstOrDefaultAsync<object>();

                return new DatabaseResult<IDictionary<string, object>>(DatabaseResultStatus.Ok, (IDictionary<string, object>)result);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching character info from database. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<IDictionary<string, object>>(DatabaseResultStatus.OperationFailed, null);
            }
        }

        public static async Task<DatabaseResult<Dictionary<string, float>>> FetchConstants()
        {
            try
            {
                var result = await DatabaseManager.BeginQuery()
                    .From("game_constants")
                    .Select("const_name", "value")
                    .GetAsync<(string, float)>();

                return new DatabaseResult<Dictionary<string, float>>(DatabaseResultStatus.Ok,
                    result.ToDictionary(x => x.Item1, x => x.Item2));
            }
            catch (Exception ex)
            {
                log.Error("Error fetching constants. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<Dictionary<string, float>>(DatabaseResultStatus.OperationFailed, null);
            }
        }
        
        public static async Task<List<Item>> FetchAllItems()
        {
            var conn = DatabaseManager.GetConnection();
            try
            {
                await conn.OpenAsync();
                var result = (await DatabaseManager.BeginQuery(conn)
                    .From("items")
                    .Select("id", "name", "item_type")
                    .GetAsync<ItemInfo>()).ToArray();

                Dictionary<ItemId, List<ItemPropertyInfo>> itemsProperties = await FetchAllItemsProperties(conn);
                var items = new List<Item>(result.Length);

                foreach (var itemInfo in result)
                    items.Add(Item.CreateNewItem(itemInfo, itemsProperties.ContainsKey(itemInfo.Id) ? itemsProperties[itemInfo.Id] : null));
                
                return items;
            }
            catch (Exception ex)
            {
                log.Error("Error fetching items from database. {0}\n{1}", ex.Message, ex.StackTrace);
                GameManager.CloseApplication();
                return null;
            }
        }

        public static async Task<Item> FetchSingleItem(ushort itemId)
        {
            var conn = DatabaseManager.GetConnection();
            try
            {
                await conn.OpenAsync();
                var itemInfo = await DatabaseManager.BeginQuery(conn)
                    .From("items")
                    .Where("id", itemId)
                    .Select("id", "name", "item_type")
                    .FirstAsync<ItemInfo>();

                var properties = await FetchItemProperties(itemInfo.Id, conn);
                return properties is null ? null : Item.CreateNewItem(itemInfo, properties);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching item {0} from database. {1}\n{2}", itemId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

		private static async Task<Dictionary<ItemId, List<ItemPropertyInfo>>> FetchAllItemsProperties(DbConnection connection)
		{
            try
            {
                var result = await DatabaseManager.BeginQuery(connection)
                    .From("items_properties_values")
                    .OrderBy("item_id")
                    .Select("item_id", "item_property_id AS Property", "property_value AS Value")
                    .GetAsync<ItemPropertyInfo>();

                return result.GroupBy(info => info.ItemId)
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
            catch (Exception ex)
            {
                log.Error("Error fetching items properties from database. {0}\n{1}", ex.Message, ex.StackTrace);
                GameManager.CloseApplication();
                return null;
            }
        }

        private static async Task<List<ItemPropertyInfo>> FetchItemProperties(ItemId itemId, DbConnection conn)
        {
            try
            {
                return (await DatabaseManager.BeginQuery(conn)
                    .From("items_properties_values")
                    .Where("item_id", itemId)
                    .Select("item_id", "item_property_id AS Property", "property_value AS Value")
                    .GetAsync<ItemPropertyInfo>()).ToList();
            }
            catch (IndexOutOfRangeException)
            {
                // This exception is thrown when the table has no data matching item_id
                return null;
            }
            catch (Exception ex)
            {
                log.Error("Error fetching properties of item {0} from database. {1}\n{2}", itemId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
            }
        }

		public static async Task<List<Spell>> FetchAllSpells()
        {
            var conn = DatabaseManager.GetConnection();
            try
            {
                await conn.OpenAsync();
                var result = (await DatabaseManager.BeginQuery(conn)
                    .From("spells")
                    .Select("*")
                    .GetAsync<SpellInfo>()).ToArray();

                Dictionary<SpellId, List<SpellPropertyInfo>> spellsProperties = await FetchAllSpellsProperties(conn);
                var spells = new List<Spell>(result.Length);
                
                foreach (var spellInfo in result)
                    spells.Add(Spell.CreateNewSpell(spellInfo, spellsProperties[spellInfo.Id]));

                return spells;
            }
            catch (Exception ex)
            {
                log.Error("Error fetching spells from database. {0}\n{1}", ex.Message, ex.StackTrace);
                GameManager.CloseApplication();
                return null;
            }
        }
        
        public static async Task<Spell> FetchSingleSpell(ushort spellId)
        {
            var conn = DatabaseManager.GetConnection();
            try
            {
                await conn.OpenAsync();
                var spellInfo = await DatabaseManager.BeginQuery(conn)
                    .From("spells")
                    .Where("id", spellId)
                    .Select("*")
                    .FirstAsync<SpellInfo>();

                var properties = await FetchSpellProperties(spellInfo.Id, conn);
                return properties is null ? null : Spell.CreateNewSpell(spellInfo, properties);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching spell {0} from database. {1}\n{2}", spellId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

		private static async Task<Dictionary<SpellId, List<SpellPropertyInfo>>> FetchAllSpellsProperties(DbConnection connection)
		{
            try
            {
                var result = await DatabaseManager.BeginQuery(connection)
                    .From("spells_properties_values")
                    .OrderBy("spell_id")
                    .Select("spell_id", "spell_property_id AS Property", "property_value AS Value")
                    .GetAsync<SpellPropertyInfo>();

                return result.GroupBy(info => info.SpellId)
                    .ToDictionary(g => (SpellId)g.Key, g => g.ToList());
            }
            catch (Exception ex)
            {
                log.Error("Error fetching spells properties from database. {0}\n{1}", ex.Message, ex.StackTrace);
                GameManager.CloseApplication();
                return null;
            }
        }
        
        private static async Task<List<SpellPropertyInfo>> FetchSpellProperties(SpellId spellId, DbConnection conn)
        {
            try
            {
                return (await DatabaseManager.BeginQuery(conn)
                    .From("spells_properties_values")
                    .Where("spell_id", spellId)
                    .Select("spell_id", "spell_property_id AS Property", "property_value AS Value")
                    .GetAsync<SpellPropertyInfo>()).ToList();
            }
            catch (IndexOutOfRangeException)
            {
                // This exception is thrown when the table has no data matching spell_id
                return null;
            }
            catch (Exception ex)
            {
                log.Error("Error fetching properties of spell {0} from database. {1}\n{2}", spellId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
            }
        }

		public static async Task<List<NpcInfo>> FetchAllNpcsInfo()
        {
            var conn = DatabaseManager.GetConnection();
            try
            {
                await conn.OpenAsync();
                var (properties, jsonProperties) = await FetchAllNpcsProperties(conn);

                var result = (await DatabaseManager.BeginQuery(conn)
                    .From("npcs")
                    .Select("*")
                    .GetAsync<NpcInfo>()).ToArray();
                
                var npcs = new List<NpcInfo>(result.Length);

                foreach (var npcInfo in result)
                {
                    properties.TryGetValue(npcInfo.Id, out var props);
                    jsonProperties.TryGetValue(npcInfo.Id, out var jsonProps);
                    npcInfo.LoadProperties(props, jsonProps);
                    npcs.Add(npcInfo);
                }

                return npcs;
            }
            catch (Exception ex)
            {
                log.Error("Error fetching npcs info from database. {0}\n{1}", ex.Message, ex.StackTrace);
                GameManager.CloseApplication();
                return null;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public static async Task<NpcInfo> FetchSingleNpcInfo(ushort npcId)
        {
            var conn = DatabaseManager.GetConnection();
            try
            {
                await conn.OpenAsync();
                var properties = await FetchNpcProperties(npcId, conn);
                var jsonProperties = await FetchNpcJsonProperties(npcId, conn);
                
                var npcInfo = await DatabaseManager.BeginQuery()
                    .From("npcs")
                    .Where("id", npcId)
                    .Select("*")
                    .FirstOrDefaultAsync<NpcInfo>();
                
                npcInfo?.LoadProperties(properties, jsonProperties);
                return npcInfo;
            }
            catch (Exception ex)
            {
                log.Error("Error fetching info for npc {0} from database. {1}\n{2}", npcId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        private static async Task<(Dictionary<NpcId, List<NpcPropertyInfo>>, Dictionary<NpcId, List<NpcJsonPropertyInfo>>)> FetchAllNpcsProperties(DbConnection connection)
        {
            try
            {
                var propsQuery = NpcPropertiesQuery(connection).From("npcs_properties_values");
                var npcsProperties = (await propsQuery.GetAsync<NpcPropertyInfo>())
                    .GroupBy(x => x.NpcId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var jsonPropsQuery = NpcPropertiesQuery(connection).From("npcs_properties_strings");
                var npcsJsonProperties = (await jsonPropsQuery.GetAsync<NpcJsonPropertyInfo>())
                    .GroupBy(x => x.NpcId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                return (npcsProperties, npcsJsonProperties);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching all npcs properties from database. {0}\n{1}", ex.Message, ex.StackTrace);
                await connection.CloseAsync();
                GameManager.CloseApplication();
                return (null, null);
            }
        }

        private static SqlKata.Query NpcPropertiesQuery(DbConnection connection)
        {
            return DatabaseManager.BeginQuery(connection)
                .OrderBy("npc_id")
                .Select("npc_id", "npc_property_id AS Property", "property_value AS Value");
        }
        
        private static async Task<List<NpcPropertyInfo>> FetchNpcProperties(NpcId npcId, DbConnection connection)
        {
            try
            {
                return (await DatabaseManager.BeginQuery(connection)
                        .From("npcs_properties_values")
                        .Where("npc_id")
                        .Select("npc_id", "npc_property_id AS Property", "property_value AS Value")
                        .GetAsync<NpcPropertyInfo>())
                    .ToList();
            }
            catch (IndexOutOfRangeException)
            {
                // This exception is thrown when the table has no data matching npc_id
                return null;
            }
            catch (Exception ex)
            {
                log.Error("Error fetching properties of npc: {0} from database. {1}\n{2}", npcId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
            }
        }

		private static async Task<List<NpcJsonPropertyInfo>> FetchNpcJsonProperties(NpcId npcId, DbConnection connection)
		{
            try
            {
                var result = await DatabaseManager.BeginQuery(connection)
                    .From("npcs_properties_strings")
                    .Where("npc_id")
                    .Select("npc_id", "npc_property_id AS Property", "property_value AS Value")
                    .GetAsync<NpcJsonPropertyInfo>();

                return result.ToList();
            }
            catch (IndexOutOfRangeException)
            {
                // This exception is thrown when the table has no data matching npc_id
                return null;
            }
            catch (Exception ex)
            {
                log.Error("Error fetching json properties of npc: {0} from database. {1}\n{2}", npcId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
            }
        }

        public static async Task FetchAndLoadCraftableItems()
        {
            var conn = DatabaseManager.GetConnection();
            try
            {
                await conn.OpenAsync();
                var requirements = await FetchAllCraftableItemsRequirements(conn);
                var craftableItems = await DatabaseManager.BeginQuery()
                    .From("craftable_items")
                    .Select("*")
                    .GetAsync<CraftableItem>();

                foreach (var craftableItem in craftableItems)
                {
                    craftableItem.LoadRequirements(requirements[craftableItem.Id]);

                    switch (craftableItem.Profession)
                    {
                        case CraftingProfession.Blacksmithing:
                            CraftingProfessions.BlacksmithingItems.Add(craftableItem.Item.Id, craftableItem);
                            break;
                        case CraftingProfession.Woodworking:
                            CraftingProfessions.WoodworkingItems.Add(craftableItem.Item.Id, craftableItem);
                            break;
                        case CraftingProfession.Tailoring:
                            CraftingProfessions.TailoringItems.Add(craftableItem.Item.Id, craftableItem);
                            break;
                        default:
                            log.Warn("Unidentified profession {0} in craftable item {1}", craftableItem.Profession, craftableItem.Id);
                            break;
                    }
                }

                log.Info("Successfully loaded craftable items.");
                CollectionProfessions.SetItems();
            }
            catch (Exception ex)
            {
                log.Error("Error fetching craftable items from database. {0}\n{1}", ex.Message, ex.StackTrace);
                GameManager.CloseApplication();
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
        
        public static async Task<CraftableItem> FetchSingleCraftableItem(ushort craftableId)
        {
            var conn = DatabaseManager.GetConnection();
            try
            {
                await conn.OpenAsync();
                var requirements = await FetchCraftableItemRequirements(craftableId, conn);

                if (requirements is null)
                    return null;
                
                var craftableItem = await DatabaseManager.BeginQuery()
                    .From("craftable_items")
                    .Where("id", craftableId)
                    .Select("*")
                    .FirstOrDefaultAsync<CraftableItem>();
                
                craftableItem?.LoadRequirements(requirements);
                return craftableItem;
            }
            catch (Exception ex)
            {
                log.Error("Error fetching info for craftable item {0} from database. {1}\n{2}", craftableId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        private static async Task<Dictionary<ushort, List<(ItemId, ushort)>>> FetchAllCraftableItemsRequirements(DbConnection connection)
        {
            try
            {
                var result = (await DatabaseManager.BeginQuery(connection)
                        .From("craftable_items_requirements")
                        .OrderBy("craftable_item_id")
                        .Select("craftable_item_id", "required_item_id", "required_amount")
                        .GetAsync<(ushort, ushort, ushort)>())
                    .GroupBy(x => x.Item1);

                var dictionary = new Dictionary<ushort, List<(ItemId, ushort)>>();
                foreach (var group in result)
                {
                    var list = group.Select(x => ((ItemId)x.Item2, x.Item3)).ToList();
                    dictionary.Add(group.Key, list);
                }

                return dictionary;
            }
            catch (Exception ex)
            {
                log.Error("Error fetching requirements for craftable items from database. {0}\n{1}", ex.Message, ex.StackTrace);
                GameManager.CloseApplication();
                return null;
            }
        }

		private static async Task<List<(ItemId, ushort)>> FetchCraftableItemRequirements(ushort craftableId, DbConnection connection)
        {
            try
            {
                return (await DatabaseManager.BeginQuery(connection)
                        .From("craftable_items_requirements")
                        .Where("craftable_item_id", craftableId)
                        .Select("required_item_id", "required_amount")
                        .GetAsync<(ItemId, ushort)>())
                    .ToList();
            }
			catch (Exception ex)
			{
				log.Error("Error fetching requirements for craftable item {0} from database. {1}\n{2}", craftableId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
			}
        }

        public static async Task<Dictionary<QuestId, Quest>> FetchAllQuests()
        {
            try
            {
                var result = (await DatabaseManager.BeginQuery()
                    .From("quests")
                    .Select("id", "repeatable", "requirements", "goals", "rewards")
                    .GetAsync<QuestInfo>()).ToArray();
                
                var quests = new Dictionary<QuestId, Quest>(result.Length);
                
                foreach (var questInfo in result)
                    quests.Add(questInfo.Id, new Quest(questInfo));

                return quests;
            }
            catch (Exception ex)
            {
                log.Error("Error fetching quests from database. {0}\n{1}", ex.Message, ex.StackTrace);
                GameManager.CloseApplication();
                return null;
            }
        }

        public static async Task<Quest> FetchSingleQuest(ushort questId)
        {
            try
            {
                return await DatabaseManager.BeginQuery()
                    .From("quests")
                    .Where("id")
                    .Select("id", "repeatable", "requirements", "goals", "rewards")
                    .FirstOrDefaultAsync<Quest>();
            }
            catch (Exception ex)
            {
                log.Error("Error fetching quest {0} from database. {1}\n{2}", questId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
            }
        }

		public static async Task<Dictionary<Attribute, byte>> FetchAttributes(bool closeOnError)
        {
            try
            {
                return (await DatabaseManager.BeginQuery()
                        .From("attributes")
                        .Select("id", "base_value")
                        .GetAsync<(byte, byte)>())
                    .ToDictionary(x => (Attribute)x.Item1, x => x.Item2);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching attributes from database. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                if (closeOnError)
                    GameManager.CloseApplication();
                return null;
            }
        }

		public static async Task<Dictionary<ClassType, Class>> FetchClasses(bool closeOnError)
        {
            try
            {
                return (await DatabaseManager.BeginQuery()
                        .From("classes")
                        .Select("*")
                        .GetAsync<Class>())
                    .ToDictionary(c => c.ClassType);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching classes from database. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                if (closeOnError)
                    GameManager.CloseApplication();
                return null;
            }
        }

		public static async Task<Dictionary<RaceType, Race>> FetchRaces(bool closeOnError)
        {
            try
            {
                return (await DatabaseManager.BeginQuery()
                        .From("races")
                        .Select("id AS raceType", "default_anim_m AS defaultAnimMale", "default_anim_f AS defaultAnimFemale")
                        .GetAsync<Race>())
                    .ToDictionary(c => c.RaceType);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching races from database. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                if (closeOnError)
                    GameManager.CloseApplication();
                return null;
            }
        }

		public static async Task<IEnumerable<(byte, sbyte)>> FetchRaceAttributes(byte raceId)
        {
            try
            {
                return await DatabaseManager.BeginQuery()
                    .From("races_attributes")
                    .Where("race_id", raceId)
                    .Select("attribute_id", "attribute_value")
                    .GetAsync<(byte, sbyte)>();
            }
            catch (Exception ex)
            {
                log.Error("Error fetching attributes of race: {0} from database. {1}\n{2}", raceId, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
            }
        }

		public static async Task<IEnumerable<byte>> FetchRaceHeads(byte race, byte gender)
		{
            try
            {
                return await DatabaseManager.BeginQuery()
                    .From("heads")
                    .Where("race_id", race)
                    .Where("gender", gender)
                    .Select("head_id")
                    .GetAsync<byte>();
            }
            catch (Exception ex)
            {
                log.Error("Error fetching heads of race: {0} and gender: {1} from database. {2}\n{3}", race, gender, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return null;
            }
        }

		public static async Task<Dictionary<byte, PlayerLevelInfo>> FetchLevels(bool closeOnError)
		{
            try
            {
                return (await DatabaseManager.BeginQuery()
                        .From("levels")
                        .Select("level", "max_xp", "max_skill")
                        .GetAsync<PlayerLevelInfo>())
                    .ToDictionary(info => info.Level);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching levels from database. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                if (closeOnError)
                    GameManager.CloseApplication();
                return null;
            }
        }
        
        public static async Task<DatabaseResult<ulong>> FetchIdByName(string table, string elementName)
        {
            try
            {
                var result = await DatabaseManager.BeginQuery()
                    .From(table)
                    .Where("name", elementName)
                    .Select("id")
                    .FirstOrDefaultAsync<ulong>();
                
                return new DatabaseResult<ulong>(DatabaseResultStatus.Ok, result);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching id of {0} in table {1}. {2}\n{3}", elementName, table, ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<ulong>(DatabaseResultStatus.OperationFailed, default);
            }
        }
        
        public static async Task<DatabaseResult<uint>> WriteMail(Mail mail, DbConnection connection)
        {
            try
            {
                var result = await DatabaseManager.BeginQuery(connection)
                    .From("mails")
                    .InsertGetIdAsync<uint>(mail);

                return new DatabaseResult<uint>(DatabaseResultStatus.Ok, result);
            }
            catch (Exception ex)
            {
                log.Error("Error writing mail to database. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<uint>(DatabaseResultStatus.OperationFailed, default);
            }
        }
        
        public static async Task<DatabaseResult<uint>> WriteMail(Mail mail, Transaction transaction)
        {
            try
            {
                var result = await transaction.BeginQuery()
                    .From("mails")
                    .InsertGetIdAsync<uint>(mail, transaction.DbTransaction);

                return new DatabaseResult<uint>(DatabaseResultStatus.Ok, result);
            }
            catch (Exception ex)
            {
                log.Error("Error writing mails to database. {0}\n{1}", ex.Message, ex.StackTrace);
                await transaction.RollbackTransactionAsync();
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<uint>(DatabaseResultStatus.OperationFailed, default);
            }
        }

        public static async Task<DatabaseResult<byte>> FetchCharacterMailCount(CharacterId characterId, DbConnection connection)
        {
            try
            {
                var result =  await DatabaseManager.BeginQuery(connection)
                    .From("mails")
                    .Where("recipient_character_id", characterId.AsPrimitiveType())
                    .CountAsync<byte>();

                return new DatabaseResult<byte>(DatabaseResultStatus.Ok, result);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching character mail count. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<byte>(DatabaseResultStatus.OperationFailed, default);
            }
        }

        public static async Task<DatabaseResult<List<Mail>>> FetchCharactersMail(CharacterId characterId)
        {
            
            try
            {
                var result = (List<Mail>)await DatabaseManager.BeginQuery()
                    .From("mails")
                    .Where("recipient_character_id", characterId.AsPrimitiveType())
                    .Select("*")
                    .GetAsync<Mail>();

                return new DatabaseResult<List<Mail>>(DatabaseResultStatus.Ok, result);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching mails. {0}\n{1}", ex.Message, ex.StackTrace);
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<List<Mail>>(DatabaseResultStatus.OperationFailed, null);
            }
        }
        
        public static async Task<DatabaseResultStatus> UpdateMail(Mail mail, Transaction transaction)
        {
            try
            {
                await transaction.BeginQuery()
                    .From("mails")
                    .Where("id", mail.Id)
                    .UpdateAsync(mail, transaction.DbTransaction);

                return DatabaseResultStatus.Ok;
            }
            catch (Exception ex)
            {
                log.Error("Error updating mail. {0}\n{1}", ex.Message, ex.StackTrace);
                await transaction.RollbackTransactionAsync();
                DatabaseManager.OnDatabaseOperationFailed();
                return DatabaseResultStatus.OperationFailed;
            }
        }
        
        public static async Task<DatabaseResultStatus> DeleteMails(IEnumerable<uint> mailIds, Transaction transaction)
        {
            try
            {
                await transaction.BeginQuery()
                    .From("mails")
                    .WhereIn("id", mailIds)
                    .DeleteAsync(transaction.DbTransaction);

                return DatabaseResultStatus.Ok;
            }
            catch (Exception ex)
            {
                log.Error("Error deleting mails. {0}\n{1}", ex.Message, ex.StackTrace);
                await transaction.RollbackTransactionAsync();
                DatabaseManager.OnDatabaseOperationFailed();
                return DatabaseResultStatus.OperationFailed;
            }
        }

        public static async Task<DatabaseResult<int>> DeleteExpiredMails(Transaction transaction)
        {
            try
            {
                var result = await transaction.BeginQuery()
                    .From("mails")
                    .Where("expiration_date", "<", DateTime.Now)
                    .Where(q =>
                        q.WhereTrue("has_been_returned")
                            .OrWhereTrue("has_been_opened")
                            .OrWhereNull("items_json"))
                    .DeleteAsync(transaction.DbTransaction);

                return new DatabaseResult<int>(DatabaseResultStatus.Ok, result);
            }
            catch (Exception ex)
            {
                log.Error("Error deleting expired mails. {0}\n{1}", ex.Message, ex.StackTrace);
                await transaction.RollbackTransactionAsync();
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<int>(DatabaseResultStatus.OperationFailed, default);
            }
        }

        public static async Task<DatabaseResult<List<Mail>>> FetchMailsToReturn(Transaction transaction)
        {
            try
            {
                var result = (List<Mail>)await transaction.BeginQuery()
                    .From("mails")
                    .Where("expiration_date", "<", DateTime.Now)
                    .WhereNotNull("items_json")
                    .WhereFalse("has_been_returned")
                    .WhereFalse("has_been_opened")
                    .Select("*")
                    .GetAsync<Mail>(transaction.DbTransaction);

                return new DatabaseResult<List<Mail>>(DatabaseResultStatus.Ok, result);
            }
            catch (Exception ex)
            {
                log.Error("Error fetching mails to return. {0}\n{1}", ex.Message, ex.StackTrace);
                await transaction.RollbackTransactionAsync();
                DatabaseManager.OnDatabaseOperationFailed();
                return new DatabaseResult<List<Mail>>(DatabaseResultStatus.OperationFailed, null);
            }
        }

		public static async Task<bool> TestConnection()
	    {
            try
            {
                var _ = await DatabaseManager.BeginQuery()
                    .From("items")
                    .CountAsync<int>();
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Error connecting to database. {0}\n{1}", ex.Message, ex.StackTrace);
                return false;
            }
        }
	}
}