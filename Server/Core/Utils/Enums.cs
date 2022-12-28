namespace AO.Players
{
    public enum GameMasterRank
    {
        GameMaster
    }
    
    public enum ClassType 
    {
        Mage = 1,
        Druid,
        Cleric,
        Bard,
        Paladin,
        Assassin,
        Warrior,
        Hunter,
        Worker,
        Length
    }

    /// <summary>All the races in the game. Can't name it Race cause Race is the class.</summary>
    public enum RaceType
    {
        Human = 1,
        Elf,
        NightElf,
        Dwarf,
        Gnome,
        Length
    }

    public enum Gender
    {
        Female,
        Male,
        Both
    }

    /// <summary>Player attributes.</summary>
    public enum Attribute
    {
        Strength = 1,
        Agility,
        Intellect,
        Charisma,
        Constitution
    }

    /// <summary>Player skills.</summary>
    public enum Skill
    {
        Magic,
        ArmedCombat,
        RangedWeapons,
        UnarmedCombat,
        Stabbing,
        CombatTactics,
        MagicResistance,
        ShieldDefense,
        Meditation,
        Survival,
        AnimalTaming,
        Hiding,
        Trading,
        Thieving,
        Leadership,
        Sailing,
        HorseRiding,
        Mining,
        Blacksmithing,
        Woodcutting,
        Woodworking,
        Fishing,
        Tailoring,
        Length // This is used to check the client assigned skills on char creation
    }
    
    public enum ClickRequest
    {
        NoRequest,
        CastSpell,
        ProjectileAttack,
        TameAnimal,
        PetChangeTarget,
        Steal,
        InviteToParty,
        Mine,
        CutWood,
        Fish,
        Smelt,
        CraftBlacksmithing
    }

    /// <summary>Player resources.</summary>
    public enum Resource
    {
        Health,
        Mana,
        Stamina,
        HungerAndThirst
    }

    /// <summary>Game factions.</summary>
    [System.Flags]
    public enum Faction : byte
    {
        Citizen = 0x00,
        Criminal = 0x01,
        Imperial = 0x02,
        Chaos = 0x04
    }

    public enum FactionRank
    {
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten
    }

    /// <summary>Player statistics.</summary>
    public enum PlayerStat
    {
        CriminalsKilled,
        CitizensKilled,
        UsersKilled,
        NpcsKilled,
        Deaths,
        RemainingJailTime
    }

    /// <summary>Used to notify the client of a status change.</summary>
    public enum PlayerStatus
    {
        Died,
        Revived,
        UsedBoat,
        Mounted,
        ChangedFaction,
        ChangedGuildName,
        Meditate,
    }

    [System.Flags]
    public enum PlayerMovementInputs
    {
        Empty = 0x00,
        MoveUp = 0x01,
        MoveDown = 0x02,
        MoveLeft = 0x04,
        MoveRight = 0x08
    }

    public enum PlayerInput
    {
        GrabItem,
        Attack,
        SafeToggle,
        RessToggle,
        Exit,
        Meditate,
        StartParty,
        LeaveParty
    }

    namespace Talents
    {
        public enum CanSkillUpTalent
        {
            Yes,
            No,
            InvalidId
        }
    }
    
    namespace Talents.Worker
    {
        public enum MiningTalent
        {
            FastMining,
            DropLessOre,
            MineSilver,
            MineGold,
            SentinelChanceReductionMining
        }

        public enum WoodCuttingTalent
        {
            FastCutting,
            DropLessWood,
            CutElficWood,
            SentinelChanceReductionWoodCutting
        }

        public enum FishingTalent
        {
            FishPejerrey,
            FishHake,
            FishSwordFish,
            UseFishingNet,
            GalleyFishing,
            SchoolFishing,
            SentinelChanceReductionFishing
        }

        public enum BlacksmithingTalent
        {
            HelmetsShields,
            WeaponsStaves,
            Armors,
            RingsMagical
        }

        public enum WoodWorkingTalent
        {
            ArrowsBows,
            BoltsCrossbows,
            Boat,
            Galley,
            LuteFlutes,
            Magical
        }

        public enum TailoringTalent
        {
            WolfSkinning,
            BearSkinning,
            PolarBearSkinning,
            Hats,
            Tunics
        }
    }
}

namespace AO.Core.Utils
{
    public enum CharactersTableColumn
    {
        Id = 1,
        AccountId,
        Username,
        Class,
        Race,
        Gender,
        HeadId,
        Magic,
        ArmedCombat,
        RangedWeapons,
        UnarmedCombat,
        Stabbing,
        CombatTactics,
        MagicResistance,
        ShieldDefense,
        Meditation,
        Survival,
        AnimalTaming,
        Hiding,
        Trading,
        Thieving,
        Leadership,
        Sailing,
        HorseRiding,
        Mining,
        Blacksmithing,
        WoodCutting,
        WoodWorking,
        Fishing,
        Tailoring,
        Level,
        CurrentExperience,
        AssignableSkills,
        TalentPoints,
        MaxHealth,
        CurrentHealth,
        MaxMana,
        CurrentMana,
        MaxStamina,
        CurrentStamina,
        MaxHunger,
        CurrentHunger,
        MaxThirst,
        CurrentThirst,
        Hit,
        Faction,
        HasGuild,
        GuildName,
        CriminalsKilled,
        CitizensKilled,
        UsersKilled,
        NpcsKilled,
        Deaths,
        RemainingJailTime,
        Bans,
        IsBanned,
        BannedUntil,
        Gold,
        BankGold,
        Inventory,
        Bank,
        Mailbox,
        Spells,
        QuestsProgresses,
        QuestsCompleted,
        Map,
        XPos,
        YPos,
        IsGm
    }

    public enum WorkerTalentsTableColumn
    {
        CharacterId = 1,
        FastMining,
        DropLessOre,
        MineSilver,
        MineGold,
        SentinelMining,
        FastCutting,
        DropLessWood,
        CutElficWood,
        SentinelWoodCutting,
        FishPejerrey,
        FishHake,
        FishSwordfish,
        UseFishingNet,
        GalleyFishing,
        SchoolFishing,
        SentinelFishing,
        HelmetsShields,
        WeaponsStaves,
        Armors,
        RingsMagical,
        ArrowsBows,
        BoltsCrossbows,
        Boat,
        Galley,
        LuteFlutes,
        Magical,
        WolfSkinning,
        BearSkinning,
        PolarBearSkinning,
        Hats,
        Tunics
    }

    public enum LoginRegisterMessage
    {
        LoginOk,
        AccountAlreadyLoggedIn,
        InvalidAccountOrPassword,
        RegisterOk,
        AccountAlreadyExists,
        EmailAlreadyUsed,
        Error = -1
    }

    public enum CreateCharacterMessage
    {
        Ok,
        InvalidName,
        NameAlreadyInUse,
        NotAllSkillsAreAssigned,
        Error = -1
    }

    /// <summary> Heading direction of a player or npc.</summary>
    public enum Heading
    {
        South,
        East,
        West,
        North
    }

    /// <summary>Body parts a hit can happen.</summary>
    public enum BodyPart
    {
        Head = 1,
        LeftLeg,
        RightLeg,
        LeftArm,
        RightArm,
        Chest
    }
        
    /// <summary>Types of console messages. Used to change font size, color, type, etc.</summary>
    public enum ConsoleMessage
    {
        DefaultMessage,
        Warning,
        PlayerClickCitizen,
        PlayerClickCriminal,
        PlayerClickImperial,
        PlayerClickChaos,
        PlayerClickGameMaster,
        Combat
    }
    
    /// <summary>Mostly used to write combat messages in console and avoid sending tons of strings over the net. Also used as a generic message type to set flags on the client.</summary>
    public enum MultiMessage : byte
    {
        //// COMBAT ////
        AttackerAppliedBleed,
        AttackerEnvenomed,
        BlockedWithShieldOther,
        BlockedWithShieldPlayer,
        CantAttackInSafeZone,
        CantAttackOwnPet,
        CantAttackSpirit,
        CantAttackThatNpc,
        CantAttackYourself,
        KilledNpc,
        KilledPlayer,
        NoAmmo,
        NpcHitPlayer,
        NpcKilledPlayer,
        NpcDamageSpellPlayer,
        NpcEnvenomedPlayer,
        NpcSwing,
        PlayerAttackedSwing,
        PlayerDamageSpellNpc,
        PlayerDamageSpellEnemy,
        EnemyDamageSpellPlayer,
        PlayerGotStabbed,
        PlayerHitByPlayer,
        PlayerHitNpc,
        PlayerHitPlayer,
        PlayerKilled,
        PlayerSwing,
        StabbedNpc,
        StabbedPlayer,
        TargetGotBled,
        TargetGotEnvenomed,
        TooFarToAttack,
        YouAreBleeding,
        YouAreEnvenomed,
        ////////////////
        
        ///// NPC //////
        AlreadyTamedThatNpc,
        CantSummonPetInSafeZone,
        AlreadyHaveAPet,
        CantTameNpc,
        CantTameNpcInCombat,
        DisplayInfo,
        FailedToTameNpc,
        NoNpcToTame,
        NpcAlreadyHasOwner,
        ShowNpcDescription,
        SuccessfullyTamedNpc,
        TooFarToTame,
        PetIgnoreCommand,
        ////////////////
        
        //// ITEMS  ////
        BlackPotionOne,
        BlackPotionTwo,
        CantUseAxeAndShield,
        CantUseClass,
        CantUseFaction,
        CantUseGender,
        CantUseRace,
        CantUseWhileMeditating,
        CantUseWeaponLikeThat,
        InventoryFull,
        ItemOnlyNewbies,
        MustEquipItemFirst,
        NotEnoughMoney,
        NotEnoughSkillToUse,
        CantUseTalent,
        NoSpaceToDropItem,
        PlayerDroppedItemTo,
        PlayerGotItemDropped,
        ////////////////
        
        //// SPELLS ////
        CantCastDead,
        CantCastOnSpirit,
        CantLearnMoreSpells,
        InvalidTarget,
        MagicItemNotEquipped,      
        MagicItemNotPowerfulEnough,
        NotEnoughMana,
        NotEnoughSkillToCast,      
        NotEnoughStaminaToCast,    
        NpcImmuneToSpell,
        NpcsOnlySpell,
        NpcHealedPlayer,
        PlayerHealedNpc,
        PlayerHealed,
        PlayerGotHealed,
        PlayerSelfHeal,
        SpellAlreadyLearned,
        SpellMessage,
        SpellSelfMessage,
        SpellTargetMessage,        
        StaffNotEquipped,
        StaffNotPowerfulEnough,  
        TargetRessToggledOff,
        TooFarToCast,
        UsersOnlySpell,
        ////////////////
        
        /// FACTION ////
        CantAttackCitizenWithSafeOn,
        CantHelpNpcCitizen,
        CantHelpNpcFaction,
        ChaosCantAttackChaosNpc,
        ChaosCantHelpCitizen,
        CitizenAttackedCitizen,
        CitizenAttackedCitizenPet,
        CitizenAttackedImperialNpc,
        CitizenAttackedNpcFightingCitizen,        
        CitizenSafeOnCantAttackCitizenPet,        
        CitizenSafeOnCantAttackImperialNpc,       
        CitizenSafeOnCantAttackNpcFightingCitizen,
        HelpCriminalsToggleSafeOff,
        HelpNpcsToggleSafeOff,
        ImperialCantAttackCitizenPet,
        ImperialCantAttackImperialNpc,
        ImperialCantAttackNpcFightingCitizen,     
        ImperialCantHelpCriminal,
        ImperialsCantAttackCitizens,
        ////////////////
        
        // PROFESSION //
        NoDepositToMine,
        CantMineThat,
        NoForgeToSmelt,
        CantSmeltThat,
        NoHammerEquipped,
        NoHandsawEquipped,
        NoSewingKitEquipped,
        NoTreeToCut,
        CantCutThatTree,
        NoWaterToFish,
        NotEnoughMaterials,
        NotEnoughOre,
        StartWorking,
        StopWorking,
        TooFarFromAnvil,
        TooFarToCutWood,
        TooFarToFish,
        TooFarToMine,
        TooFarToSmelt,
        ////////////////
        
        /// LEVELING ///
        IncreasedHit,
        IncreasedHp,
        IncreasedMana,
        IncreasedSkillPoints,
        IncreasedStamina,
        LeveledUp,
        ReachedMaxLevel,
        ////////////////
        
        //// PARTY ////
        PartyIsFull,
        PlayerAlreadyInParty,
        PlayerDifferentFaction,
        PlayerInvitedToParty,
        YouInvitedPlayerToParty,
        ///////////////
        
        /// QUESTING ///
        MustChooseQuestReward,
        NotAllStepsAreCompleted,
        QuestLogFull,
        QuestRequirementsNotMet,
        ////////////////
        
        /// MAILING  ///
        CantSendMailRightNow,
        CharacterDoesntExist,
        RecipientInboxFull,
        MailSentSuccessfully,
        NewMailReceived,
        ////////////////
        
        //// MISC /////
        CantMeditate,
        CantMeditateDead,
        ClickedOnPlayer,
        ClickedOnWorldItem,
        ExitingCancelled,
        ExitingInTenSeconds,
        FinishedMeditating,
        ManaRecovered,
        NotEnoughStamina,
        ObstacleClick,
        SkillLevelUp,
        TalentPointsObtained,
        StoppedMeditating,
        TooTiredToFight,
        YouAreDead,
        DescriptionChanged,
        DescriptionTooLong,
        DescriptionInvalid
        ///////////////
    }

    public enum Reload
    {
        All,
        Constants,
        Attributes,
        Classes,
        Races,
        Levels,
        Item,
        Spell,
        Npc,
        CraftableItem,
        Quest
    }
}

namespace AO.Items
{
    /// <summary>Types of in-game items.</summary>
    public enum ItemType
    {
        Armor = 1,
        Weapon = 2,
        Shield = 3,
        Helmet = 4,
        Gold = 5,
        Container = 7,
        Key = 9,
        Consumable = 10,
        Ring = 14,
        Scroll = 19,
        Boat = 25,
        Mount = 26,
        Arrow = 27,
        Miscellaneous = 28
    }

    /// <summary>Properties of in-game items.</summary>
    public enum ItemProperty
    {
        Imperial = 1,
        Chaos = 2,
        Envenoms = 8,
        MaxHit = 9,
        MinHit = 10,
        WeaponType = 11,
        MagicPower = 12,
        MagicDamageBonus = 13,
        Reinforcement = 14,
        IndexClosedKey = 16,
        MaxModifier = 27,
        MinModifier = 28,
        EffectDuration = 29,
        Paralyzes = 30,
        Dwarf = 31,
        Gnome = 32,
        Human = 33,
        Elf = 34,
        NightElf = 35,
        Price = 36,
        Animation = 42,
        MaxDef = 43,
        MinDef = 44,
        MagicResistance = 45,
        ConsumableType = 46,
        Gender = 47,
        SpellIndex = 50,
        SkillToUse = 53,
        Mage = 54,
        Druid = 55,
        Cleric = 56,
        Bard = 57,
        Paladin = 58,
        Assassin = 59,
        Warrior = 60,
        Hunter = 61,
        Worker = 63,
        MinMr = 65,
        MaxMr = 66,
        MiscellaneousType = 67,
        AppliesBleed = 68,
        Grababble = 69,
        Falls = 70,
        MaxStacks = 71,
        IsNewbie = 72,
        GiveQuest = 73,
        IsGalley = 74
    }

    /// <summary>Types of consumable items.</summary>
    public enum ConsumableType
    {
        Food = 1,
        Drink = 2,
        EmptyBottle = 3,
        FilledBottle = 4,
        BluePotion = 5,
        RedPotion = 6,
        GreenPotion = 7,
        YellowPotion = 8,
        VioletPotion = 9,
        BlackPotion = 10
    }

    /// <summary>Types of miscellaneous items.</summary>
    public enum MiscellaneousType
    {
        Door = 1,
        Sign = 2,
        Wood = 3,
        ElficWood = 4,
        BlacksmithingHammer = 5,
        Key = 6,
        Anvil = 7,
        Forge = 8,
        WolfSkin = 9,
        PolarWolfSkin = 10,
        IronOre = 11,
        SilverOre = 12,
        GoldOre = 13,
        IronIngot = 14,
        SilverIngot = 15,
        GoldIngot = 16,
        Handsaw = 17,
        BearSkin = 18,
        PolarBearSkin = 19,
        SewingKit = 20,
        QuestGiver = 21
    }

    public enum WeaponType
    {
        Sword = 1,
        Axe = 2,
        Maze = 3,
        Dagger = 4,
        Bow = 5,
        Crossbow = 6,
        Staff = 7,
        FishingRod = 8,
        FishingNet = 9,
        LumberjackAxe = 10,
        MiningPick = 11
    }
}

namespace AO.Systems.Professions
{
    public enum Profession
    {
        Mining,
        WoodCutting,
        Fishing,
        Blacksmithing,
        Woodworking,
        Tailoring
    }
    
    public enum CollectionProfession
    {
        Mining = Profession.Mining,
        WoodCutting = Profession.WoodCutting,
        Fishing = Profession.Fishing
    }
    
    public enum CraftingProfession
    {
        Blacksmithing = Profession.Blacksmithing,
        Woodworking = Profession.Woodworking,
        Tailoring = Profession.Tailoring
    }
}

namespace AO.Spells
{
    /// <summary>Types of spells.</summary>
    public enum SpellType
    {
        Damages = 1,
        Heals,
        ModsUser,
        SummonNpc,
        Metamorphosis
    }

    /// <summary>What can the spell be casted on.</summary>
    public enum SpellTarget
    {
        User = 1,
        Npc,
        Both,
        Terrain
    }

    /// <summary>Properties of the spells.</summary>
    public enum SpellProperty
    {
        MinMod = 1,
        MaxMod,
        TargetMessage,
        SelfMessage,
        ModType,
        NpcId,
        NpcAmount,
        Mimetizes,
        Duration,
        MageIgnoresMr
    }

    /// <summary>Types of spells that modify some flag.</summary>
    public enum SpellModType
    {
        Strength = 1,
        Agility,
        Invisibility,
        Paralyze,
        Immobilize,
        RemovesParalysis,
        Envenom,
        RemovesEnvenomed,
        Revive
    }
}

namespace AO.Npcs
{
    public enum NpcType
    {
        Regular = 1,
        Priest = 2,
        Bank = 3,
        Auctioneer = 4,
        Gambler = 5,
        Trader = 6,
        Factional = 7
    }

    public enum NpcFaction
    {
        Neutral = 0,
        Imperial = 1,
        Chaos = 2
    }

    public enum NpcProperty
    {
        WalksOnWater = 3,
        DoesntWalkOnGround = 4,
        CannotBeParalyzed = 5,
        MaxHit = 9,
        MinHit = 10,
        AttackPower = 11,
        Spells = 12,
        Sounds = 13,
        SkillToTame = 14,
        XpAmount = 15,
        ItemsToDrop = 16,
        GoldAmount = 17,
        TraderInventory = 19,
        KeepsItems = 20,
        Class = 21,
        Recruits = 22,
        Envenoms = 23,
        CasterOnly = 24,
        PetAttackMod = 25,
        PetSpellMod = 26,
        GiveQuests = 27,
        TurnInQuests = 28,
        CantBeManualPetTarget = 29
    }

    public enum NpcPatrollingBehaviour
    {
        Static,
        Basic
    }

    public enum NpcTargetingBehaviour
    {
        NoTargeting,
        BasicHostile
    }
    
    public enum NpcAttackingBehaviour
    {
        NoAttacking,
        BasicHostile
    }

    public enum NpcIdEnum : ushort
    {
        Medusa = 1,
        Pociones = 2,
        Serpiente = 3,
        Escorpion = 4,
        Murcielago = 5,
        Lobo = 6,
        Sacerdote = 7,
        Mago = 8,
        PocionesNewbie = 9,
        PescadorNix = 10
    }
}

namespace AO.World
{
    public enum TriggerType
    { 
        SafeZone,
        UnsafeZone,
        Arena,
        AntiBlock,
        InvalidNpcPosition,
        CantInvis,
        QuestExploreArea
    }

    public enum ZoneType
    {
        SafeZone,
        UnsafeZone,
        Arena
    }
}

namespace AO.Systems.Questing
{
    public enum Repeatable
    {
        No,
        Daily,
        Weekly
    }
}