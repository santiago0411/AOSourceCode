using System.Reflection;
using AO.Core.Database;
using AO.Core.Ids;
using AO.Players;

namespace AO.Core.Utils
{
    public static class Constants
    {
        #region Player
        public const float PLAYER_MOVE_SPEED = 4f;
        public const byte PLAYER_INV_SPACE = 30;
        public const byte PLAYER_BANK_SPACE = 60;
        public const byte PLAYER_SPELLS_SPACE = 36;
        public const byte PLAYER_MAX_QUESTS = 20;
        public const byte PLAYER_MAX_DESC_LENGTH = 75;
        #endregion

        #region PlayerStats
        public const byte MAX_NEWBIE_LEVEL = 12;
        public const byte MAX_PLAYER_SKILL = 100;
        public const byte MAX_PLAYER_LEVEL = 50;
        public const ushort MAX_PLAYER_HIT_UNDER36 = 99;
        public const ushort MAX_PLAYER_HIT_OVER36 = 999;
        public const int MAX_KILLS = 100000;
        #endregion

        #region Npc
        public const byte NPC_INVENTORY_SPACE = 30;
        public const uint SELLING_PRICE_REDUCTION = 3;
        public const float DEFAULT_NPC_MOVE_SPEED = 2.5f;
        #endregion

        #region LevelUp
        public const byte WAR_ADDITIONAL_HP = 2;
        public const byte HUNTER_ADDITIONAL_HP = 1;
        public const byte DEFAULT_STAM_INCREASE = 15;
        public const byte WORKER_STAM_INCREASE = DEFAULT_STAM_INCREASE + 25;
        #endregion

        #region Combat
        public const Faction CITIZEN_IMPERIAL = Faction.Citizen | Faction.Imperial;
        public const Faction CRIMINAL_CHAOS = Faction.Criminal | Faction.Chaos;
        public const byte MIN_SKILL_TO_STAB = 10;
        public const byte VISION_RANGE_X = 8;
        public const byte VISION_RANGE_Y = 6;
        public static readonly float SWORD_ARMOR_PEN_MOD = 0.3f;
        public static readonly float MAZE_SHIELD_EVASION_MOD = 0.5f;
        public static readonly float AXE_CRIT_DAMAGE_MOD = 1.5f;
        public static readonly float AXE_PLAYER_CRIT_CHANCE = 15;
        public static readonly float AXE_NPC_CRIT_CHANCE = 15;
        #endregion

        #region Combat intervals and buff/debuffs tick rates
        public static readonly float PLAYER_CAN_ATTACK_TIME = 1.5f;
        public static readonly float PLAYER_CAN_USE_TIME = 0.3f;
        public static readonly float PLAYER_CAN_ATTACK_USE_TIME = 1.2f;
        public static readonly float PLAYER_CAN_CAST_SPELL_TIME = 1.4f;
        public static readonly float PLAYER_CAN_ATTACK_CAST_TIME = 1f;
        public static readonly float PLAYER_CAN_CAST_ATTACK_TIME = 1f;
        public static readonly float PLAYER_CAN_USE_BOW_TIME = 1.4f;
        public static readonly float PLAYER_WORK_INTERVAL = 1f;
        public static readonly float PLAYER_OWNS_NPC_TIME = 10f;
        public static readonly float NPC_CAN_ATTACK_TIME = 1.6f;
        public static readonly float NPC_CAN_CAST_SPELL_TIME = 3f;
        public static readonly float NPC_PARALYZE_TIME = 60f;
        public static readonly float PLAYER_PARALYZE_TIME = 30f;
        public static readonly float ATTRIBUTES_BUFF_DURATION = 60f;


        public static readonly float RECOVER_STAM_RESTING_TICK_RATE = 3f;
        public static readonly float RECOVER_STAM_NOT_RESTING_TICK_RATE = 6f;
        public static readonly float THIRST_TICK_RATE = 15 * 60;
        public static readonly float HUNGER_TICK_RATE = 13 * 60;
        public static readonly float VENOM_TICK_RATE = 2f;
        public static readonly float BLEEDING_TICK_RATE = 3f;
        #endregion
        
        #region Mailing
        public const byte MAIL_MAX_ITEMS = 10;
        public const byte MAIL_MAX_MAILS = 30;
        public const byte MAIL_MAX_SUBJECT = 25;
        public const byte MAIL_MAX_BODY = 250;
        public const float MAILS_CACHE_EXPIRATION_TIME = 2000f;
        #endregion
        
        #region Extras
        public const int MANA_RECOVER_PERCENTAGE = 6;
        #endregion

        #region StartingInventories
        public const string INVENTORY_MALE_SHORT_MAGIC = "[[0,42,150,false],[1,43,150,false],[2,38,1,true],[3,17,1,true],[4,34,100,false],[5,33,150,false]]";
        public const string INVENTORY_MALE_SHORT_NOT_MAGIC = "[[0,42,150,false],[1,43,150,false],[2,38,1,true],[3,17,1,true],[4,33,150,false]]";
        public const string INVENTORY_FEMALE_SHORT_MAGIC = "[[0,42,150,false],[1,43,150,false],[2,39,1,true],[3,17,1,true],[4,34,100,false],[5,33,150,false]]";
        public const string INVENTORY_FEMALE_SHORT_NOT_MAGIC = "[[0,42,150,false],[1,43,150,false],[2,39,1,true],[3,17,1,true],[4,33,150,false]]";
        public const string INVENTORY_MALE_TALL_MAGIC = "[[0,42,150,false],[1,43,150,false],[2,8,1,true],[3,17,1,true],[4,34,100,false],[5,33,150,false]]";
        public const string INVENTORY_MALE_TALL_NOT_MAGIC = "[[0,42,150,false],[1,43,150,false],[2,8,1,true],[3,17,1,true],[4,33,100,false]";
        public const string INVENTORY_FEMALE_TALL_MAGIC = "[[0,42,150,false],[1,43,150,false],[2,8,1,true],[3,17,1,true],[4,34,100,false],[5,33,150,false]]";
        public const string INVENTORY_FEMALE_TALL_NOT_MAGIC = "[[0,42,150,false],[1,43,150,false],[2,8,1,true],[3,17,1,true],[4,33,100,false]";
        public const string STARTING_SPELLS = "[[0,1]]";
        public const string EMPTY_SPELLS = "[]";
        #endregion

        #region ItemIds
        public static readonly ItemId GOLD_ID = 44;
        public static readonly ItemId PORGY_ID = 48;
        public static readonly ItemId PEJERREY_ID = 140;
        public static readonly ItemId HAKE_ID = 141;
        public static readonly ItemId SWORDFISH_ID = 142;
        public static readonly ItemId HIPPOCAMPUS_ID = 143;
        public static readonly ItemId WOOD_ID = 50;
        public static readonly ItemId ELFIC_WOOD_ID = 51;
        public static readonly ItemId IRON_ORE_ID = 52;
        public static readonly ItemId SILVER_ORE_ID = 53;
        public static readonly ItemId GOLD_ORE_ID = 54;
        public static readonly ItemId IRON_INGOT_ID = 55;
        public static readonly ItemId SILVER_INGOT_ID = 56;
        public static readonly ItemId GOLD_INGOT_ID = 57;
        #endregion

        #region Professions
        public const ushort FISHING_STAM_COST = 2;
        public const ushort WOODCUTTING_STAM_COST = 3;
        public const ushort MINING_STAM_COST = 4;
        public const ushort SMELTING_STAM_COST = 3;
        public const byte ORE_REQUIRED_IRON_INGOT = 14;
        public const byte ORE_REQUIRED_SILVER_INGOT = 20;
        public const byte ORE_REQUIRED_GOLD_INGOT = 35;
        public const float FISHING_NET_AMOUNT_MOD = 1.25f;
        #endregion
        
        #region Party
        public const byte MAX_PARTY_MEMBERS = 5;
        public static readonly float TWO_PLAYER_PARTY_BONUS = 1.2f;
        public static readonly float THREE_PLAYER_PARTY_BONUS = 1.4f;
        public static readonly float FOUR_PLAYER_PARTY_BONUS = 1.6f;
        public static readonly float FIVE_PLAYER_PARTY_BONUS = 1.8f;
        public static readonly float MAGICAL_NOMAGICAL_PARTY_BONUS = 0.2f;
        public static readonly float XP_DISTRIBUTION_RANGE = 25.5f; 
        #endregion

        public static async System.Threading.Tasks.Task Load(bool shouldExitOnFailure)
        {
            var log = log4net.LogManager.GetLogger(typeof(Constants));

            var (status, values) = await DatabaseOperations.FetchConstants();
            if (status != DatabaseResultStatus.Ok)
            {
                log.Error("Failed to load constants.");
                if (shouldExitOnFailure)
                    GameManager.CloseApplication();
                
                return;
            }

            if (values.Count == 0)
                return;
            
            // Load the fields using reflection so they can stay readonly and never be modified unless they are being reloaded from the database
            var fields = typeof(Constants).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
                if (values.TryGetValue(field.Name, out float v))
                    field.SetValue(null, v);
            
            log.Info("Successfully loaded constants.");
        }
    }
}
