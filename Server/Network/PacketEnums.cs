namespace AO.Network
{
    public enum ServerPackets : short
    {
        Welcome,
        LoginReturn,
        RegisterAccountReturn,
        GetRacesAttributesReturn,
        GetCharactersReturn,
        CreateCharacterReturn,
        SpawnPlayer,
        PlayerMaxResources,
        PlayerPrivateInfo,
        PlayerSkills,
        ChatBroadcast,
        PlayerPosition,
        PlayerRangeChanged,
        PlayerUpdatePosition,
        PlayerDisconnected,
        PlayerResources,
        PlayerIndividualResource,
        PlayerStats,
        PlayerTalentsPoints,
        PlayerLeveledUpTalents,
        PlayerGainedXp,
        PlayerAttributes,
        PlayerGold,
        ClickRequest,
        WorldItemSpawned,
        WorldItemDestroyed,
        PlayerInventory,
        PlayerUpdateInventory,
        PlayerSwapInventorySlots,
        PlayerEquippedItems,
        OnPlayerItemEquippedChanged,
        EndEnterWorld,
        ConsoleMessage,
        UpdatePlayerSpells,
        MovePlayerSpell,
        SayMagicWords,
        NpcSpawn,
        NpcPosition,
        NpcRangeChanged,
        NpcFacing,
        NpcStartTrade,
        NpcInventoryUpdate,
        NpcDespawned,
        UpdatePlayerStatus,
        MultiMessage,
        PlayerInputReturn,
        CreateParticle,
        OpenCraftingWindow,
        DoorState,
        QuestAssigned,
        QuestProgressUpdate,
        QuestCompleted,
        NpcQuests,
        CanSkillUpTalentReturn,
        OnYouJoinedParty,
        OnPlayerJoinedParty,
        OnPlayerLeftParty,
        OnCanEditPercentagesChanged,
        OnExperiencePercentageChanged,
        OnPartyLeaderChanged,
        OnPartyGainedExperience,
        OnPartyMemberGainedExperience,
        FetchMailsReturn,
        RemoveMailItem,
        PlayerDescriptionChanged,
// #if AO_DEBUG
        DebugNpcPath
// #endif
    }

    public enum ClientPackets : short
    {
        WelcomeReceived,
        Login,
        RegisterAccount,
        GetRacesAttributes,
        GetCharacters,
        CreateCharacter,
        BeginEnterWorld,
        PlayerChat,
        PlayerMovementInputs,
        PlayerInput,
        PlayerItemAction,
        PlayerDropItem,
        PlayerLeftClick,
        PlayerSwappedItemSlot,
        NpcTrade,
        EndNpcTrade,
        PlayerSelectedSpell,
        PlayerLeftClickRequest,
        MovePlayerSpell,
        SkillsChanged,
        DropGold,
        CraftItem,
        CloseCraftingWindow,
        SelectQuest,
        SelectQuestItemReward,
        AcceptQuest,
        CompleteQuest,
        CanSkillUpTalent,
        SkillUpTalents,
        ChangePartyPercentages,
        KickPartyMember,
        SendMail,
        FetchMails,
        CollectMailItem,
        DeleteMail
    }
}
