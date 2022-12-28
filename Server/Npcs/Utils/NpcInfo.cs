using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using AO.Core;
using AO.Core.Ids;
using Newtonsoft.Json;
using AO.Systems.Questing;
using JetBrains.Annotations;

namespace AO.Npcs.Utils
{
    public sealed class NpcInfo
    {
        [UsedImplicitly]
        public readonly NpcId Id;
        
        [UsedImplicitly]
        public readonly string Name;
        
        [UsedImplicitly]
        public readonly NpcType Type;
        
        [UsedImplicitly]
        public readonly bool HasAI;
        
        [UsedImplicitly]
        public readonly NpcPatrollingBehaviour PatrollingBehaviour;
        
        [UsedImplicitly]
        public readonly NpcTargetingBehaviour TargetingBehaviour;
        
        [UsedImplicitly]
        public readonly NpcAttackingBehaviour AttackingBehaviour;
        
        [UsedImplicitly]
        public readonly NpcFaction NpcFaction;
        
        [UsedImplicitly]
        public readonly int Health;
        
        [UsedImplicitly]
        public readonly byte Evasion;
        
        [UsedImplicitly]
        public readonly byte Defense;
        
        [UsedImplicitly]
        public readonly byte MagicDefense;
        
        [UsedImplicitly]
        public readonly bool Attackable;
        
        public bool WalksOnWater { get; private set; }
        public bool WalksOnGround { get; private set; } = true;
        public bool CanBeParalyzed { get; private set; } = true;
        public int MaxHit { get; private set; }
        public int MinHit { get; private set; }
        public int AttackPower { get; private set; }
        public byte? SkillToTame { get; private set; }
        public int XpAmount { get; private set; }
        public uint GoldAmount { get; private set; }
        public bool KeepsItems { get; private set; }
        public Players.ClassType Class { get; private set; }
        public bool Recruits { get; private set; }
        public bool Envenoms { get; private set; }
        public bool CasterOnly { get; private set; }
        public float PetAttackMod { get; private set; } = 1f;
        public float PetSpellMod { get; private set; } = 1f;
        public bool CanBeManualPetTarget { get; private set; } = true;

        /// <summary>List of sound ids the npc makes upon hitting something.</summary>
        public ReadOnlyCollection<ushort> Sounds { get; private set; }
        /// <summary>List of items the npc can drop upon death, quantity and their drop chance.</summary>
        public ReadOnlyCollection<NpcItemDrop> DroppableItems { get; private set; }
        /// <summary>List of spell ids the npc can cast and their cooldown.</summary>
        public ReadOnlyCollection<NpcSpell> Spells { get; private set; }
        /// <summary>List of items this npc sells if it's of type Trader.</summary>
        public ReadOnlyCollection<NpcInventorySlot> NpcInventory { get; private set; }
        /// <summary>List of quests this npc gives.</summary>
        public ReadOnlyCollection<Quest> GiveQuests { get; private set; }
        /// <summary>List of quests this npc turns in.</summary>
        public ReadOnlyCollection<QuestId> TurnInQuests { get; private set; }
        
        private NpcInfo() {}
        
        public void LoadProperties(List<NpcPropertyInfo> properties, List<NpcJsonPropertyInfo> jsonProperties)
        {
            LoadProps(properties);
            LoadJsonProps(jsonProperties);
        }

        private void LoadProps(List<NpcPropertyInfo> properties)
        {
            if (properties is null)
                return;
            
            foreach (var (property, value) in properties)
            {
                switch (property)
                {
                    case NpcProperty.WalksOnWater:
                        WalksOnWater = true;
                        break;
                    case NpcProperty.DoesntWalkOnGround:
                        WalksOnGround = false;
                        break;
                    case NpcProperty.CannotBeParalyzed:
                        CanBeParalyzed = false;
                        break;
                    case NpcProperty.MaxHit:
                        MaxHit = (int)value;
                        break;
                    case NpcProperty.MinHit:
                        MinHit = (int)value;
                        break;
                    case NpcProperty.AttackPower:
                        AttackPower = (int)value;
                        break;
                    case NpcProperty.SkillToTame:
                        SkillToTame = Convert.ToByte(value);
                        break;
                    case NpcProperty.XpAmount:
                        XpAmount = (int)value;
                        break;
                    case NpcProperty.GoldAmount:
                        GoldAmount = Convert.ToUInt32(value);
                        break;
                    case NpcProperty.KeepsItems:
                        KeepsItems = Convert.ToBoolean(value);
                        break;
                    case NpcProperty.Class:
                        if (Enum.TryParse<Players.ClassType>(value.ToString(CultureInfo.InvariantCulture), out var cl))
                            Class = cl;
                        break;
                    case NpcProperty.Recruits:
                        Recruits = true;
                        break;
                    case NpcProperty.Envenoms:
                        Envenoms = true;
                        break;
                    case NpcProperty.CasterOnly:
                        CasterOnly = true;
                        break;
                    case NpcProperty.PetAttackMod:
                        PetAttackMod = value;
                        break;
                    case NpcProperty.PetSpellMod:
                        PetSpellMod = value;
                        break;
                    case NpcProperty.CantBeManualPetTarget:
                        CanBeManualPetTarget = false;
                        break;
                }
            }
        }
        
        private void LoadJsonProps(List<NpcJsonPropertyInfo> jsonProperties)
        {
            var properties = jsonProperties is not null 
                ? jsonProperties.ToDictionary(info => info.Property, info => info.Value) 
                : new Dictionary<NpcProperty, string>();
            
            Sounds = properties.ContainsKey(NpcProperty.Sounds)
                ? JsonConvert.DeserializeObject<ReadOnlyCollection<ushort>>(properties[NpcProperty.Sounds])
                : new ReadOnlyCollection<ushort>(Array.Empty<ushort>());

            DroppableItems = properties.ContainsKey(NpcProperty.ItemsToDrop)
                ? ConvertJsonToNpcItems(properties[NpcProperty.ItemsToDrop])
                : new ReadOnlyCollection<NpcItemDrop>(Array.Empty<NpcItemDrop>());

            Spells = properties.ContainsKey(NpcProperty.Spells)
                ? ConvertJsonToNpcSpells(properties[NpcProperty.Spells])
                : new ReadOnlyCollection<NpcSpell>(Array.Empty<NpcSpell>());

            NpcInventory = properties.ContainsKey(NpcProperty.TraderInventory)
                ? ConvertJsonToNpcInventory(properties[NpcProperty.TraderInventory])
                : new ReadOnlyCollection<NpcInventorySlot>(Array.Empty<NpcInventorySlot>());

            GiveQuests = properties.ContainsKey(NpcProperty.GiveQuests)
                ? LoadGiveQuests(properties[NpcProperty.GiveQuests])
                : new ReadOnlyCollection<Quest>(Array.Empty<Quest>());
            
            TurnInQuests = properties.ContainsKey(NpcProperty.TurnInQuests)
                ? JsonConvert.DeserializeObject<ReadOnlyCollection<QuestId>>(properties[NpcProperty.TurnInQuests])
                : new ReadOnlyCollection<QuestId>(Array.Empty<QuestId>());
        }

        private static ReadOnlyCollection<NpcInventorySlot> ConvertJsonToNpcInventory(string jsonString)
        {
            var inventory = new NpcInventorySlot[Core.Utils.Constants.NPC_INVENTORY_SPACE];
            var aux = JsonConvert.DeserializeObject<List<object[]>>(jsonString);

            byte i = 0;
            foreach (var item in aux!)
            {
                ItemId itemId = Convert.ToUInt16(item[0]);
                ushort quantity = Convert.ToUInt16(item[1]);
                bool respawns = Convert.ToBoolean(Convert.ToByte(item[2]));
                inventory[i] = new NpcInventorySlot(i, GameManager.Instance.GetItem(itemId), quantity, respawns);
                i++;
            }

            return new ReadOnlyCollection<NpcInventorySlot>(inventory);
        }
        
        private static ReadOnlyCollection<NpcItemDrop> ConvertJsonToNpcItems(string jsonString)
        {
            var itemsList = new List<NpcItemDrop>();
            var aux = JsonConvert.DeserializeObject<List<object[]>>(jsonString);

            foreach (var item in aux!)
            {
                itemsList.Add(new NpcItemDrop
                (
                    Convert.ToUInt16(item[0]),
                    Convert.ToUInt16(item[1]),
                    Convert.ToSingle(item[2])
                ));
            }

            return new ReadOnlyCollection<NpcItemDrop>(itemsList);
        }

        private static ReadOnlyCollection<NpcSpell> ConvertJsonToNpcSpells(string jsonString)
        {
            var spellsList = new List<NpcSpell>();
            var aux = JsonConvert.DeserializeObject<List<object[]>>(jsonString);

            foreach (var item in aux!)
            {
                spellsList.Add(new NpcSpell
                (
                    Convert.ToUInt16(item[0]),
                    Convert.ToByte(item[1])
                ));
            }

            return new ReadOnlyCollection<NpcSpell>(spellsList);
        }

        private static ReadOnlyCollection<Quest> LoadGiveQuests(string questIdsJson)
        {
            var ids = JsonConvert.DeserializeObject<ushort[]>(questIdsJson);
            var quests = new Quest[ids!.Length];
            
            for (int i = 0; i < ids.Length; i++)
                quests[i] = QuestManager.GetQuest(ids[i]);

            return new ReadOnlyCollection<Quest>(quests);
        }
    }
}
