namespace AOClient.Core.Utils
{
    public enum Heading
    {
        South,
        East,
        West,
        North
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
    
    public enum BodyPart
    {
        Head = 1,
        LeftLeg,
        RightLeg,
        LeftArm,
        RightArm,
        Chest
    }

    public enum SpellTarget
    {
        User = 1,
        Npc,
        Both,
        Terrain
    }

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

namespace AOClient.UI
{
    public enum Scene
    {
        Login,
        Register,
        CharacterScreen,
        CharacterCreation,
        Main,
        Dev
    }
}

namespace AOClient.UI.Main.Talents.Worker
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
        BoltsCrossBows,
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

namespace AOClient.Player
{
    public enum DefaultAnimation
    {
        HumanMale = 1,
        HumanFemale,
        ElfMale,
        ElfFemale,
        NelfMale,
        NelfFemale,
        DwarfMale,
        DwarfFemale,
        GnomeMale,
        GnomeFemale
    }
    
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
        Worker
    }

    public enum RaceType
    {
        Human = 1,
        Elf,
        NightElf,
        Dwarf,
        Gnome
    }

    public enum PlayerAttribute
    {
        Strength = 1,
        Agility,
        Intellect,
        Charisma,
        Constitution
    }

    public enum Gender
    {
        Female,
        Male
    }

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
        Tailoring
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

    public enum PlayerStat
    {
        CriminalsKilled,
        CitizensKilled,
        UsersKilled,
        NpcsKilled,
        Deaths,
        RemainingJailTime
    }
    
    public enum Resource
    {
        Health,
        Mana,
        Stamina,
        HungerAndThirst
    }
    
    public enum PlayerStatus
    {
        Died,
        Revived,
        UsedBoat,
        Mounted,
        ChangedFaction,
        ChangedGuildName,
        Meditate,
        SafeToggle
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
}