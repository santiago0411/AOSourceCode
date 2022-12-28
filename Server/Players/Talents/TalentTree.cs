using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AO.Core.Logging;
using AO.Players.Talents.Worker;
using MC = AO.Players.Talents.Worker.MiningNodesConstants;
using WCC = AO.Players.Talents.Worker.WoodCuttingNodesConstants;
using FC = AO.Players.Talents.Worker.FishingNodesConstants;
using BSC = AO.Players.Talents.Worker.BlacksmithingNodesConstants;
using WWC = AO.Players.Talents.Worker.WoodWorkingNodesConstants;
using TLC = AO.Players.Talents.Worker.TailoringNodesConstants;

namespace AO.Players.Talents
{
    public class TalentTree<T> where T : Enum
    {
        public int Count => talents.Count;
        public byte PointsInTree => (byte)talents.Values!.Sum(t => t.Points);
        
        private readonly ReadOnlyDictionary<T, TalentTreeNode> talents;

        public TalentTree(ReadOnlyDictionary<T, TalentTreeNode> talents)
        {
            this.talents = talents;
        }

        public TalentTreeNode GetNode(T nodeId)
        {
            return talents[nodeId];
        }

        public IEnumerator<(T, TalentTreeNode)> GetEnumerator()
        {
            foreach (var (talentId, talentNod) in talents)
                yield return (talentId, talentNod);
        }
    }

    public static class TalentTree
    {
        private static readonly LoggerAdapter log = new(typeof(TalentTree));
        
        public static CanSkillUpTalent CanLearnMiningNode(Player player, TalentTree<MiningTalent> tree, MiningTalent nodeId, out TalentTreeNode node)
        {
            switch (nodeId)
            {
                case MiningTalent.FastMining:
                    node = tree.GetNode(MiningTalent.FastMining);
                    return CanSkillUpTalent.Yes;
                case MiningTalent.DropLessOre:
                    node = tree.GetNode(MiningTalent.DropLessOre);
                    return CanSkillUpTalent.Yes;
                case MiningTalent.MineSilver:
                    node = tree.GetNode(MiningTalent.MineSilver);
                    return tree.PointsInTree >= MC.POINTS_NEEDED_FOR_MINE_SILVER_NODE ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                case MiningTalent.MineGold:
                    node = tree.GetNode(MiningTalent.MineGold);
                    return tree.PointsInTree >= MC.POINTS_NEEDED_FOR_MINE_GOLD_NODE && tree.GetNode(MC.NODE_NEEDED_FOR_MINE_GOLD).Acquired
                        ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                case MiningTalent.SentinelChanceReductionMining:
                    node = tree.GetNode(MiningTalent.SentinelChanceReductionMining);
                    return tree.PointsInTree >= MC.POINTS_NEEDED_SENTINEL_NODE_MINING ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                default:
                    node = null;
                    log.Warn("Player {0} ({1}) sent an invalid MiningTalent to skill up!", player.Username, player.Id);
                    player.Client.Disconnect();
                    return CanSkillUpTalent.InvalidId;
            }
        }

        public static CanSkillUpTalent CanLearnWoodCuttingNode(Player player, TalentTree<WoodCuttingTalent> tree, WoodCuttingTalent nodeId, out TalentTreeNode node)
        {
            switch (nodeId)
            {
                case WoodCuttingTalent.FastCutting:
                    node = tree.GetNode(WoodCuttingTalent.FastCutting);
                    return CanSkillUpTalent.Yes;
                case WoodCuttingTalent.DropLessWood:
                    node = tree.GetNode(WoodCuttingTalent.DropLessWood);
                    return CanSkillUpTalent.Yes;
                case WoodCuttingTalent.CutElficWood:
                    node = tree.GetNode(WoodCuttingTalent.CutElficWood);
                    return tree.PointsInTree >= WCC.POINTS_NEEDED_FOR_CUT_ELFIC_WOOD_NODE ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                case WoodCuttingTalent.SentinelChanceReductionWoodCutting:
                    node = tree.GetNode(WoodCuttingTalent.SentinelChanceReductionWoodCutting);
                    return tree.PointsInTree >= WCC.POINTS_NEEDED_SENTINEL_NODE_WOOD_CUTTING ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                default:
                    node = null;
                    log.Warn("Player {0} ({1}) sent an invalid WoodCuttingTalent to skill up!", player.Username, player.Id);
                    player.Client.Disconnect();
                    return CanSkillUpTalent.InvalidId;
            }
        }

        public static CanSkillUpTalent CanLearnFishingNode(Player player, TalentTree<FishingTalent> tree, FishingTalent nodeId, out TalentTreeNode node)
        {
            switch (nodeId)
            {
                case FishingTalent.FishPejerrey:
                    node = tree.GetNode(FishingTalent.FishPejerrey);
                    return CanSkillUpTalent.Yes;
                case FishingTalent.FishHake:
                    node = tree.GetNode(FishingTalent.FishHake);
                    return tree.PointsInTree >= FC.POINTS_NEEDED_FOR_HAKE_NODE && tree.GetNode(FC.NODE_NEEDED_FOR_HAKE_NODE).Acquired
                        ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                case FishingTalent.FishSwordFish:
                    node = tree.GetNode(FishingTalent.FishSwordFish);
                    return tree.PointsInTree >= FC.POINTS_NEEDED_FOR_SWORDFISH_NODE && tree.GetNode(FC.NODE_NEEDED_FOR_SWORDFISH_NODE).Acquired
                        ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                case FishingTalent.UseFishingNet:
                    node = tree.GetNode(FishingTalent.UseFishingNet);
                    return CanSkillUpTalent.Yes;
                case FishingTalent.GalleyFishing:
                    node = tree.GetNode(FishingTalent.GalleyFishing);
                    return tree.PointsInTree >= FC.POINTS_NEEDED_FOR_GALLEY_NODE && tree.GetNode(FC.NODE_NEEDED_FOR_GALLEY_NODE).Acquired
                        ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                case FishingTalent.SchoolFishing:
                    node = tree.GetNode(FishingTalent.SchoolFishing);
                    return tree.PointsInTree >= FC.POINTS_NEEDED_FOR_SCHOOL_FISHING_NODE && tree.GetNode(FC.NODE_NEEDED_FOR_SCHOOL_FISHING_NODE).Acquired
                        ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                case FishingTalent.SentinelChanceReductionFishing:
                    node = tree.GetNode(FishingTalent.SentinelChanceReductionFishing);
                    return tree.PointsInTree >= FC.POINTS_NEEDED_FOR_SENTINEL_NODE_FISHING ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                default:
                    node = null;
                    log.Warn("Player {0} ({1}) sent an invalid FishingTalent to skill up!", player.Username, player.Id);
                    player.Client.Disconnect();
                    return CanSkillUpTalent.InvalidId;
            }
        }

        public static CanSkillUpTalent CanLearnBlacksmithingNode(Player player, TalentTree<BlacksmithingTalent> tree, BlacksmithingTalent nodeId, out TalentTreeNode node)
        {
            switch (nodeId)
            {
                case BlacksmithingTalent.HelmetsShields:
                    node = tree.GetNode(BlacksmithingTalent.HelmetsShields);
                    return CanSkillUpTalent.Yes;
                case BlacksmithingTalent.WeaponsStaves:
                    node = tree.GetNode(BlacksmithingTalent.WeaponsStaves);
                    return CanSkillUpTalent.Yes;
                case BlacksmithingTalent.Armors:
                    node = tree.GetNode(BlacksmithingTalent.Armors);
                    return CanSkillUpTalent.Yes;
                case BlacksmithingTalent.RingsMagical:
                    node = tree.GetNode(BlacksmithingTalent.RingsMagical);
                    return tree.PointsInTree >= BSC.POINTS_NEEDED_FOR_MAGICAL_NODE ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                default:
                    node = null;
                    log.Warn("Player {0} ({1}) sent an invalid BlacksmithingTalent to skill up!", player.Username, player.Id);
                    player.Client.Disconnect();
                    return CanSkillUpTalent.InvalidId;
            }
        }

        public static CanSkillUpTalent CanLearnWoodWorkingNode(Player player, TalentTree<WoodWorkingTalent> tree, WoodWorkingTalent nodeId, out TalentTreeNode node)
        {
            switch (nodeId)
            {
                case WoodWorkingTalent.ArrowsBows:
                    node = tree.GetNode(WoodWorkingTalent.ArrowsBows);
                    return CanSkillUpTalent.Yes;
                case WoodWorkingTalent.BoltsCrossbows:
                    node = tree.GetNode(WoodWorkingTalent.BoltsCrossbows);
                    return CanSkillUpTalent.Yes;
                case WoodWorkingTalent.Boat:
                    node = tree.GetNode(WoodWorkingTalent.Boat);
                    return CanSkillUpTalent.Yes;
                case WoodWorkingTalent.Galley:
                    node = tree.GetNode(WoodWorkingTalent.Galley);
                    return tree.PointsInTree >= WWC.POINTS_NEEDED_FOR_GALLEY_NODE && tree.GetNode(WWC.NODE_NEEDED_FOR_GALLEY_NODE).Acquired
                        ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                case WoodWorkingTalent.LuteFlutes:
                    node = tree.GetNode(WoodWorkingTalent.LuteFlutes);
                    return CanSkillUpTalent.Yes;
                case WoodWorkingTalent.Magical:
                    node = tree.GetNode(WoodWorkingTalent.Magical);
                    return tree.PointsInTree >= WWC.POINTS_NEEDED_FOR_MAGICAL_NODE && tree.GetNode(WWC.NODE_NEEDED_FOR_MAGICAL_NODE).Acquired
                        ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                default:
                    node = null;
                    log.Warn("Player {0} ({1}) sent an invalid WoodWorkingTalent to skill up!", player.Username, player.Id);
                    player.Client.Disconnect();
                    return CanSkillUpTalent.InvalidId;
            }
        }

        public static CanSkillUpTalent CanLearnTailoringNode(Player player, TalentTree<TailoringTalent> tree, TailoringTalent nodeId, out TalentTreeNode node)
        {
            switch (nodeId)
            {
                case TailoringTalent.WolfSkinning:
                    node = tree.GetNode(TailoringTalent.WolfSkinning);
                    return CanSkillUpTalent.Yes;
                case TailoringTalent.BearSkinning:
                    node = tree.GetNode(TailoringTalent.BearSkinning);
                    return tree.PointsInTree >= TLC.POINTS_NEEDED_FOR_BEAR_NODE && tree.GetNode(TLC.NODE_NEEDED_FOR_BEAR_NODE).Acquired
                        ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                case TailoringTalent.PolarBearSkinning:
                    node = tree.GetNode(TailoringTalent.PolarBearSkinning);
                    return tree.PointsInTree >= TLC.POINTS_NEEDED_FOR_POLAR_BEAR_NODE && tree.GetNode(TLC.NODE_NEEDED_FOR_POLAR_BEAR_NODE).Acquired
                        ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                case TailoringTalent.Hats:
                    node = tree.GetNode(TailoringTalent.Hats);
                    return tree.PointsInTree >= TLC.POINTS_NEEDED_FOR_HATS_NODE && tree.GetNode(TLC.NODE_NEEDED_FOR_HATS_NODE).Acquired
                        ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                case TailoringTalent.Tunics:
                    node = tree.GetNode(TailoringTalent.Tunics);
                    return tree.PointsInTree >= TLC.POINTS_NEEDED_FOR_TUNICS_NODE && tree.GetNode(TLC.NODE_NEEDED_FOR_TUNICS_NODE).Acquired
                        ? CanSkillUpTalent.Yes : CanSkillUpTalent.No;
                default:
                    node = null;
                    log.Warn("Player {0} ({1}) sent an invalid TailoringTalent to skill up!", player.Username, player.Id);
                    player.Client.Disconnect();
                    return CanSkillUpTalent.InvalidId;
            }
        }
    }
}