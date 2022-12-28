namespace AO.Players.Talents.Worker
{
    public static class MiningNodesConstants
    {
        public const byte FAST_MINING_MAX_POINTS = 3;
        public const byte DROP_LESS_ORE_MAX_POINTS = 3;
        public const byte MINE_SILVER_MAX_POINTS = 1;
        public const byte MINE_GOLD_MAX_POINTS = 1;
        public const byte SENTINEL_CHANCE_REDUCTION_MAX_POINTS = 1;
        
        public const byte POINTS_NEEDED_FOR_FAST_MINING_NODE = 0;
        public const byte POINTS_NEEDED_FOR_DROP_LESS_ORE_NODE = 0;
        public const byte POINTS_NEEDED_FOR_MINE_SILVER_NODE = 3;
        public const MiningTalent NODE_NEEDED_FOR_MINE_GOLD = MiningTalent.MineSilver; 
        public const byte POINTS_NEEDED_FOR_MINE_GOLD_NODE = 6;
        public const byte POINTS_NEEDED_SENTINEL_NODE_MINING = 8;
        
        public const float MINING_SPEED_MOD_1 = 1.1f;
        public const float MINING_SPEED_MOD_2 = 1.25f;
        public const float MINING_SPEED_MOD_3 = 1.5f;

        public const byte ORE_DROP_CONSTANT = 8;
    }

    public static class WoodCuttingNodesConstants
    {
        public const byte FAST_CUTTING_MAX_POINTS = 3;
        public const byte DROP_LESS_WOOD_MAX_POINTS = 3;
        public const byte CUT_ELFIC_WOOD_MAX_POINTS = 1;
        public const byte SENTINEL_CHANCE_REDUCTION_MAX_POINTS = 1;
        
        public const byte POINTS_NEEDED_FOR_FAST_CUTTING_NODE = 0;
        public const byte POINTS_NEEDED_FOR_DROP_LESS_WOOD_NODE = 0;
        public const byte POINTS_NEEDED_FOR_CUT_ELFIC_WOOD_NODE = 3;
        public const byte POINTS_NEEDED_SENTINEL_NODE_WOOD_CUTTING = 7;
        
        public const float WOOD_CUTTING_SPEED_MOD_1 = 1.1f;
        public const float WOOD_CUTTING_SPEED_MOD_2 = 1.25f;
        public const float WOOD_CUTTING_SPEED_MOD_3 = 1.5f;

        public const byte WOOD_DROP_CONSTANT = 8;
    }

    public static class FishingNodesConstants
    {
        public const byte FISH_PEJERREY_MAX_POINTS = 1;
        public const byte FISH_HAKE_MAX_POINTS = 1;
        public const byte FISH_SWORDFISH_MAX_POINTS = 1;
        public const byte USE_FISHING_NET_MAX_POINTS = 1;
        public const byte GALLEY_FISHING_MAX_POINTS = 1;
        public const byte SCHOOL_FISHING_MAX_POINTS = 1;
        public const byte SENTINEL_CHANCE_REDUCTION_MAX_POINTS = 1;
        
        public const byte POINTS_NEEDED_FOR_PEJERREY_NODE = 0;
        public const byte POINTS_NEEDED_FOR_HAKE_NODE = 1;
        public const FishingTalent NODE_NEEDED_FOR_HAKE_NODE = FishingTalent.FishPejerrey;
        public const byte POINTS_NEEDED_FOR_SWORDFISH_NODE = 2;
        public const FishingTalent NODE_NEEDED_FOR_SWORDFISH_NODE = FishingTalent.FishHake;
        public const byte POINTS_NEEDED_FOR_FISHING_NET_NODE = 0;
        public const byte POINTS_NEEDED_FOR_GALLEY_NODE = 2;
        public const FishingTalent NODE_NEEDED_FOR_GALLEY_NODE = FishingTalent.UseFishingNet;
        public const byte POINTS_NEEDED_FOR_SCHOOL_FISHING_NODE = 4;
        public const FishingTalent NODE_NEEDED_FOR_SCHOOL_FISHING_NODE = FishingTalent.GalleyFishing;
        public const byte POINTS_NEEDED_FOR_SENTINEL_NODE_FISHING = 6;
    }

    public static class BlacksmithingNodesConstants
    {
        public const byte HELMETS_SHIELDS_MAX_POINTS = 1;
        public const byte WEAPONS_STAVES_MAX_POINTS = 1;
        public const byte ARMORS_MAX_POINTS = 1;
        public const byte RINGS_MAGICAL_MAX_POINTS = 1;
        
        public const byte POINTS_NEEDED_FOR_HELMETS_NODE = 0;
        public const byte POINTS_NEEDED_FOR_WEAPONS_NODE = 0;
        public const byte POINTS_NEEDED_FOR_ARMORS_NODE = 0;
        public const byte POINTS_NEEDED_FOR_MAGICAL_NODE = 2;
    }

    public static class WoodWorkingNodesConstants
    {
        public const byte ARROWS_BOWS_MAX_POINTS = 1;
        public const byte BOLTS_CROSSBOWS_MAX_POINTS = 1;
        public const byte BOAT_MAX_POINTS = 1;
        public const byte GALLEY_MAX_POINTS = 1;
        public const byte LUTE_FLUTES_MAX_POINTS = 1;
        public const byte MAGICAL_MAX_POINTS = 1;

        public const byte POINTS_NEEDED_FOR_ARROWS_NODE = 0;
        public const byte POINTS_NEEDED_FOR_BOLTS_NODE = 0;
        public const byte POINTS_NEEDED_FOR_BOAT_NODE = 0;
        public const byte POINTS_NEEDED_FOR_GALLEY_NODE = 1;
        public const WoodWorkingTalent NODE_NEEDED_FOR_GALLEY_NODE = WoodWorkingTalent.Boat;
        public const byte POINTS_NEEDED_FOR_FLUTES_NODE = 0;
        public const byte POINTS_NEEDED_FOR_MAGICAL_NODE = 2;
        public const WoodWorkingTalent NODE_NEEDED_FOR_MAGICAL_NODE = WoodWorkingTalent.LuteFlutes;
    }

    public static class TailoringNodesConstants
    {
        public const byte WOLF_SKINNING_MAX_POINTS = 1;
        public const byte BEAR_SKINNING_MAX_POINTS = 1;
        public const byte POLAR_BEAR_SKINNING_MAX_POINTS = 1;
        public const byte HATS_MAX_POINTS = 1;
        public const byte TUNICS_MAX_POINTS = 1;

        public const byte POINTS_NEEDED_FOR_WOLF_NODE = 0;
        public const byte POINTS_NEEDED_FOR_BEAR_NODE = 1;
        public const TailoringTalent NODE_NEEDED_FOR_BEAR_NODE = TailoringTalent.WolfSkinning;
        public const byte POINTS_NEEDED_FOR_POLAR_BEAR_NODE = 1;
        public const TailoringTalent NODE_NEEDED_FOR_POLAR_BEAR_NODE = TailoringTalent.WolfSkinning;
        public const byte POINTS_NEEDED_FOR_HATS_NODE = 2;
        public const TailoringTalent NODE_NEEDED_FOR_HATS_NODE = TailoringTalent.PolarBearSkinning;
        public const byte POINTS_NEEDED_FOR_TUNICS_NODE = 2;
        public const TailoringTalent NODE_NEEDED_FOR_TUNICS_NODE = TailoringTalent.BearSkinning;
    }
}