using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using AO.Core.Logging;
using AO.Players;

namespace AO.Items
{
    public class Item
    {
        public readonly ItemId Id;
        public readonly string Name;
        public readonly ItemType Type;

        #region Properties
        protected bool ImperialOnly { get; private set; }
        protected bool ChaosOnly { get; private set; }
        public bool Envenoms { get; private set; }
        public bool AppliesBleed { get; private set; }
        public int MaxHit { get; private set; }
        public int MinHit { get; private set; }
        public WeaponType WeaponType { get; private set; }
        public int MagicPower { get; private set; }
        public int MagicDamageBonus { get; private set; }
        public int Reinforcement { get; private set; }
        protected int IndexClosedKey { get; private set; }
        protected int MaxModifier { get; private set; }
        protected int MinModifier { get; private set; }
        protected int EffectDuration { get; private set; }
        public bool Paralyzes { get; private set; }
        public int Price { get; private set; }
        public int MaxDef { get; private set; }
        public int MinDef { get; private set; }
        protected int MagicResistance { get; private set; }
        protected Gender Gender { get; private set; } = Gender.Both;
        protected ushort SpellIndex { get; private set; }
        protected int SkillToUse { get; private set; }
        protected int MinMr { get; private set; }
        protected int MaxMr { get; private set; }
        public MiscellaneousType MiscellaneousType { get; private set; }
        public bool WeaponIsTool { get; private set; }
        public bool IsRangedWeapon { get; private set; }
        public bool Grabbable { get; private set; } = true;
        public bool Falls { get; private set; } = true;
        public ushort MaxStacks { get; private set; } = 10000;
        public bool IsNewbie { get; private set; }
        public bool IsGalley { get; private set; }
        
        protected ushort QuestId { get; private set; }
        #endregion
        
        /// <summary>Contains the classes that cannot equip or use this item.</summary>
        protected readonly HashSet<ClassType> NotAllowedClasses = new();
        /// <summary>Contains the races that cannot equip or use this item.</summary>
        protected readonly HashSet<RaceType> NotAllowedRaces = new();
        
        private static readonly LoggerAdapter log = new(typeof(Item));

        protected Item(ItemInfo itemInfo)
        {
            (Id, Name, Type) = itemInfo;
        }

        public static Item CreateNewItem(ItemInfo itemInfo, List<ItemPropertyInfo> properties)
        {
            Item item;

            switch (itemInfo.ItemType)
            {
                case ItemType.Armor:
                    item = new Armor(itemInfo);
                    break;
                case ItemType.Weapon:
                    item = new Weapon(itemInfo);
                    break;
                case ItemType.Shield:
                    item = new Shield(itemInfo);
                    break;
                case ItemType.Helmet:
                    item = new Helmet(itemInfo);
                    break;
                case ItemType.Gold:
                    item = new Gold(itemInfo);
                    break;
                case ItemType.Container:
                    return null;
                case ItemType.Key:
                    return null;
                case ItemType.Consumable:
                    var consumableProp = properties.First(info => info.Property == ItemProperty.ConsumableType);
                    item = new Consumable(itemInfo, (ConsumableType)consumableProp.Value);
                    break;
                case ItemType.Ring:
                    item = new Ring(itemInfo);
                    break;
                case ItemType.Scroll:
                    item = new Scroll(itemInfo);
                    break;
                case ItemType.Boat:
                    item = new Boat(itemInfo);
                    break;
                case ItemType.Mount:
                    return null;
                case ItemType.Arrow:
                    item = new Arrow(itemInfo);
                    break;
                case ItemType.Miscellaneous:
                    var miscProp = properties.First(info => info.Property == ItemProperty.MiscellaneousType);
                    item = new Miscellaneous(itemInfo, (MiscellaneousType)miscProp.Value);
                    break;
                default:
                    log.Warn("Unknown ItemType: {0}' on item {1}", itemInfo.ItemType, itemInfo.Name);
                    return null;
            }
            
            if (properties is not null)
                item.LoadProperties(properties);
            
            return item;
        }

        /// <summary>Tries to use the item on the player.</summary>
        public virtual bool Use(Player player)
        {
            //Override this method when inheriting
            return false;
        }

        /// <summary>Tries to equip the item to the player.</summary>
        public virtual bool Equip(Player player)
        {
            //Override this method when inheriting
            return false;
        }
        
        private void LoadProperties(List<ItemPropertyInfo> properties)
        {
            foreach (var (property, value) in properties)
            {
                switch (property)
                {
                    case ItemProperty.Imperial:
                        ImperialOnly = true;
                        break;
                    case ItemProperty.Chaos:
                        ChaosOnly = true;
                        break;
                    case ItemProperty.Envenoms:
                        Envenoms = true;
                        break;
                    case ItemProperty.MaxHit:
                        MaxHit = value;
                        break;
                    case ItemProperty.MinHit:
                        MinHit = value;
                        break;
                    case ItemProperty.WeaponType:
                        WeaponType = (WeaponType)value;
                        SetWeaponType();
                        break;
                    case ItemProperty.MagicPower:
                        MagicPower = value;
                        break;
                    case ItemProperty.MagicDamageBonus:
                        MagicDamageBonus = value;
                        break;
                    case ItemProperty.Reinforcement:
                        Reinforcement = value;
                        break;
                    case ItemProperty.IndexClosedKey:
                        IndexClosedKey = value;
                        break;
                    case ItemProperty.MaxModifier:
                        MaxModifier = value;
                        break;
                    case ItemProperty.MinModifier:
                        MinModifier = value;
                        break;
                    case ItemProperty.EffectDuration:
                        EffectDuration = value;
                        break;
                    case ItemProperty.Paralyzes:
                        Paralyzes = true;
                        break;
                    case ItemProperty.Dwarf:
                        NotAllowedRaces.Add(RaceType.Dwarf);
                        break;
                    case ItemProperty.Gnome:
                        NotAllowedRaces.Add(RaceType.Gnome);
                        break;
                    case ItemProperty.Human:
                        NotAllowedRaces.Add(RaceType.Human);
                        break;
                    case ItemProperty.Elf:
                        NotAllowedRaces.Add(RaceType.Elf);
                        break;
                    case ItemProperty.NightElf:
                        NotAllowedRaces.Add(RaceType.NightElf);
                        break;
                    case ItemProperty.Price:
                        Price = value;
                        break;
                    case ItemProperty.MaxDef:
                        MaxDef = value;
                        break;
                    case ItemProperty.MinDef:
                        MinDef = value;
                        break;
                    case ItemProperty.MagicResistance:
                        MagicResistance = value;
                        break;
                    case ItemProperty.Gender:
                        Gender = (Gender)value;
                        break;
                    case ItemProperty.SpellIndex:
                        SpellIndex = (ushort)value;
                        break;
                    case ItemProperty.SkillToUse:
                        SkillToUse = value;
                        break;
                    case ItemProperty.Mage:
                        NotAllowedClasses.Add(ClassType.Mage);
                        break;
                    case ItemProperty.Druid:
                        NotAllowedClasses.Add(ClassType.Druid);
                        break;
                    case ItemProperty.Cleric:
                        NotAllowedClasses.Add(ClassType.Cleric);
                        break;
                    case ItemProperty.Bard:
                        NotAllowedClasses.Add(ClassType.Bard);
                        break;
                    case ItemProperty.Paladin:
                        NotAllowedClasses.Add(ClassType.Paladin);
                        break;
                    case ItemProperty.Assassin:
                        NotAllowedClasses.Add(ClassType.Assassin);
                        break;
                    case ItemProperty.Warrior:
                        NotAllowedClasses.Add(ClassType.Warrior);
                        break;
                    case ItemProperty.Hunter:
                        NotAllowedClasses.Add(ClassType.Hunter);
                        break;
                    case ItemProperty.Worker:
                        NotAllowedClasses.Add(ClassType.Worker);
                        break;
                    case ItemProperty.MinMr:
                        MinMr = value;
                        break;
                    case ItemProperty.MaxMr:
                        MaxMr = value;
                        break;
                    case ItemProperty.MiscellaneousType:
                        MiscellaneousType = (MiscellaneousType)value;
                        break;
                    case ItemProperty.AppliesBleed:
                        AppliesBleed = true;
                        break;
                    case ItemProperty.Grababble:
                        Grabbable = false;
                        break;
                    case ItemProperty.Falls:
                        Falls = false;
                        break;
                    case ItemProperty.MaxStacks:
                        MaxStacks = (ushort)value;
                        break;
                    case ItemProperty.IsNewbie:
                        IsNewbie = true;
                        break;
                    case ItemProperty.GiveQuest:
                        QuestId = (ushort)value;
                        break;
                    case ItemProperty.IsGalley:
                        IsGalley = true;
                        break;
                }
            }
        }

        private void SetWeaponType()
        {
            switch (WeaponType)
            {
                case WeaponType.FishingRod:
                case WeaponType.FishingNet:
                case WeaponType.LumberjackAxe:
                case WeaponType.MiningPick:
                    WeaponIsTool = true;
                    break;
                case WeaponType.Bow:
                case WeaponType.Crossbow:
                    IsRangedWeapon = true;
                    break;
            }
        }
    }
}
