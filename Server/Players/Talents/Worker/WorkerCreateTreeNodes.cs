using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MC = AO.Players.Talents.Worker.MiningNodesConstants;
using WCC = AO.Players.Talents.Worker.WoodCuttingNodesConstants;
using FC = AO.Players.Talents.Worker.FishingNodesConstants;
using BSC = AO.Players.Talents.Worker.BlacksmithingNodesConstants;
using WWC = AO.Players.Talents.Worker.WoodWorkingNodesConstants;
using TLC = AO.Players.Talents.Worker.TailoringNodesConstants;

namespace AO.Players.Talents.Worker
{
    public static class WorkerCreateTreeNodes
    {
        public static ReadOnlyDictionary<MiningTalent, TalentTreeNode> GetMiningNodes(IDictionary<string, object> dbRow)
        {
            var talents = new Dictionary<MiningTalent, TalentTreeNode>
            {
                { MiningTalent.FastMining, new TalentTreeNode((byte)MiningTalent.FastMining, Convert.ToByte(dbRow["fast_mining"]), MC.FAST_MINING_MAX_POINTS) },
                { MiningTalent.DropLessOre, new TalentTreeNode((byte)MiningTalent.DropLessOre, Convert.ToByte(dbRow["drop_less_ore"]), MC.DROP_LESS_ORE_MAX_POINTS) },
                { MiningTalent.MineSilver, new TalentTreeNode((byte)MiningTalent.MineSilver, Convert.ToByte(dbRow["mine_silver"]), MC.MINE_SILVER_MAX_POINTS) },
                { MiningTalent.MineGold, new TalentTreeNode((byte)MiningTalent.MineGold, Convert.ToByte(dbRow["mine_gold"]), MC.MINE_GOLD_MAX_POINTS) },
                { MiningTalent.SentinelChanceReductionMining, new TalentTreeNode((byte)MiningTalent.SentinelChanceReductionMining, Convert.ToByte(dbRow["sentinel_chance_reduction_mining"]), MC.SENTINEL_CHANCE_REDUCTION_MAX_POINTS) }
            };

            return new ReadOnlyDictionary<MiningTalent, TalentTreeNode>(talents);
        }
        
        public static ReadOnlyDictionary<WoodCuttingTalent, TalentTreeNode> GetWoodCuttingNodes(IDictionary<string, object> dbRow)
        {
            var talents = new Dictionary<WoodCuttingTalent, TalentTreeNode>
            {
                { WoodCuttingTalent.FastCutting, new TalentTreeNode((byte)WoodCuttingTalent.FastCutting, Convert.ToByte(dbRow["fast_cutting"]), WCC.FAST_CUTTING_MAX_POINTS) },
                { WoodCuttingTalent.DropLessWood, new TalentTreeNode((byte)WoodCuttingTalent.DropLessWood, Convert.ToByte(dbRow["drop_less_wood"]), WCC.DROP_LESS_WOOD_MAX_POINTS) },
                { WoodCuttingTalent.CutElficWood, new TalentTreeNode((byte)WoodCuttingTalent.CutElficWood, Convert.ToByte(dbRow["cut_elfic_wood"]), WCC.CUT_ELFIC_WOOD_MAX_POINTS) },
                { WoodCuttingTalent.SentinelChanceReductionWoodCutting, new TalentTreeNode((byte)WoodCuttingTalent.SentinelChanceReductionWoodCutting, Convert.ToByte(dbRow["sentinel_chance_reduction_woodcutting"]), WCC.SENTINEL_CHANCE_REDUCTION_MAX_POINTS) }
            };

            return new ReadOnlyDictionary<WoodCuttingTalent, TalentTreeNode>(talents);
        }
        
        public static ReadOnlyDictionary<FishingTalent, TalentTreeNode> GetFishingNodes(IDictionary<string, object> dbRow)
        {
            var talents = new Dictionary<FishingTalent, TalentTreeNode>
            {
                { FishingTalent.FishPejerrey, new TalentTreeNode((byte)FishingTalent.FishPejerrey, Convert.ToByte(dbRow["fish_pejerrey"]), FC.FISH_PEJERREY_MAX_POINTS) },
                { FishingTalent.FishHake, new TalentTreeNode((byte)FishingTalent.FishHake, Convert.ToByte(dbRow["fish_hake"]), FC.FISH_HAKE_MAX_POINTS) },
                { FishingTalent.FishSwordFish, new TalentTreeNode((byte)FishingTalent.FishSwordFish, Convert.ToByte(dbRow["fish_swordfish"]), FC.FISH_SWORDFISH_MAX_POINTS) },
                { FishingTalent.UseFishingNet, new TalentTreeNode((byte)FishingTalent.UseFishingNet, Convert.ToByte(dbRow["use_fishing_net"]), FC.USE_FISHING_NET_MAX_POINTS) },
                { FishingTalent.GalleyFishing, new TalentTreeNode((byte)FishingTalent.GalleyFishing, Convert.ToByte(dbRow["galley_fishing"]), FC.GALLEY_FISHING_MAX_POINTS) },
                { FishingTalent.SchoolFishing, new TalentTreeNode((byte)FishingTalent.SchoolFishing, Convert.ToByte(dbRow["school_fishing"]), FC.SCHOOL_FISHING_MAX_POINTS) },
                { FishingTalent.SentinelChanceReductionFishing, new TalentTreeNode((byte)FishingTalent.SentinelChanceReductionFishing, Convert.ToByte(dbRow["sentinel_chance_reduction_fishing"]), FC.SENTINEL_CHANCE_REDUCTION_MAX_POINTS) }
            };

            return new ReadOnlyDictionary<FishingTalent, TalentTreeNode>(talents);
        }
        
        public static ReadOnlyDictionary<BlacksmithingTalent, TalentTreeNode> GetBlacksmithingNodes(IDictionary<string, object> dbRow)
        {
            var talents = new Dictionary<BlacksmithingTalent, TalentTreeNode>
            {
                { BlacksmithingTalent.HelmetsShields, new TalentTreeNode((byte)BlacksmithingTalent.HelmetsShields, Convert.ToByte(dbRow["helmets_shields"]), BSC.HELMETS_SHIELDS_MAX_POINTS) },
                { BlacksmithingTalent.WeaponsStaves, new TalentTreeNode((byte)BlacksmithingTalent.WeaponsStaves, Convert.ToByte(dbRow["weapons_staves"]), BSC.WEAPONS_STAVES_MAX_POINTS) },
                { BlacksmithingTalent.Armors, new TalentTreeNode((byte)BlacksmithingTalent.Armors, Convert.ToByte(dbRow["armors"]), BSC.ARMORS_MAX_POINTS) },
                { BlacksmithingTalent.RingsMagical, new TalentTreeNode((byte)BlacksmithingTalent.RingsMagical, Convert.ToByte(dbRow["rings_magical"]), BSC.RINGS_MAGICAL_MAX_POINTS) }
            };

            return new ReadOnlyDictionary<BlacksmithingTalent, TalentTreeNode>(talents);
        }
        
        public static ReadOnlyDictionary<WoodWorkingTalent, TalentTreeNode> GetWoodWorkingNodes(IDictionary<string, object> dbRow)
        {
            var talents = new Dictionary<WoodWorkingTalent, TalentTreeNode>
            {
                { WoodWorkingTalent.ArrowsBows, new TalentTreeNode((byte)WoodWorkingTalent.ArrowsBows, Convert.ToByte(dbRow["arrows_bows"]), WWC.ARROWS_BOWS_MAX_POINTS) },
                { WoodWorkingTalent.BoltsCrossbows, new TalentTreeNode((byte)WoodWorkingTalent.BoltsCrossbows, Convert.ToByte(dbRow["bolts_crossbows"]), WWC.BOLTS_CROSSBOWS_MAX_POINTS) },
                { WoodWorkingTalent.Boat, new TalentTreeNode((byte)WoodWorkingTalent.Boat, Convert.ToByte(dbRow["boat"]), WWC.BOAT_MAX_POINTS) },
                { WoodWorkingTalent.Galley, new TalentTreeNode((byte)WoodWorkingTalent.Galley, Convert.ToByte(dbRow["galley"]), WWC.GALLEY_MAX_POINTS) },
                { WoodWorkingTalent.LuteFlutes, new TalentTreeNode((byte)WoodWorkingTalent.LuteFlutes, Convert.ToByte(dbRow["lute_flutes"]), WWC.LUTE_FLUTES_MAX_POINTS) },
                { WoodWorkingTalent.Magical, new TalentTreeNode((byte)WoodWorkingTalent.Magical, Convert.ToByte(dbRow["magical"]), WWC.MAGICAL_MAX_POINTS) }
            };

            return new ReadOnlyDictionary<WoodWorkingTalent, TalentTreeNode>(talents);
        }
        
        public static ReadOnlyDictionary<TailoringTalent, TalentTreeNode> GetTailoringNodes(IDictionary<string, object> dbRow)
        {
            var talents = new Dictionary<TailoringTalent, TalentTreeNode>
            {
                { TailoringTalent.WolfSkinning, new TalentTreeNode((byte)TailoringTalent.WolfSkinning, Convert.ToByte(dbRow["wolf_skinning"]), TLC.WOLF_SKINNING_MAX_POINTS) },
                { TailoringTalent.BearSkinning, new TalentTreeNode((byte)TailoringTalent.BearSkinning, Convert.ToByte(dbRow["bear_skinning"]), TLC.BEAR_SKINNING_MAX_POINTS) },
                { TailoringTalent.PolarBearSkinning, new TalentTreeNode((byte)TailoringTalent.PolarBearSkinning, Convert.ToByte(dbRow["polar_bear_skinning"]), TLC.POLAR_BEAR_SKINNING_MAX_POINTS) },
                { TailoringTalent.Hats, new TalentTreeNode((byte)TailoringTalent.Hats, Convert.ToByte(dbRow["hats"]), TLC.HATS_MAX_POINTS) },
                { TailoringTalent.Tunics, new TalentTreeNode((byte)TailoringTalent.Tunics, Convert.ToByte(dbRow["tunics"]), TLC.TUNICS_MAX_POINTS) }
            };

            return new ReadOnlyDictionary<TailoringTalent, TalentTreeNode>(talents);
        }
    }
}