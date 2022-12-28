using System.Collections.Generic;
using AO.Core.Logging;
using AO.Network;
using AO.Systems.Professions;

namespace AO.Players.Talents.Worker
{
    public sealed class WorkerTalentTrees
    {
        public readonly TalentTree<MiningTalent> MiningTree;
        public readonly TalentTree<WoodCuttingTalent> WoodCuttingTree;
        public readonly TalentTree<FishingTalent> FishingTree;
        public readonly TalentTree<BlacksmithingTalent> BlacksmithingTree;
        public readonly TalentTree<WoodWorkingTalent> WoodWorkingTree;
        public readonly TalentTree<TailoringTalent> TailoringTree;

        private readonly Player owner;

        private readonly Dictionary<Profession, HashSet<TalentTreeNode>> leveledUpTalents = new()
        {
            { Profession.Mining, new HashSet<TalentTreeNode>() },
            { Profession.WoodCutting, new HashSet<TalentTreeNode>() },
            { Profession.Fishing, new HashSet<TalentTreeNode>() },
            { Profession.Blacksmithing, new HashSet<TalentTreeNode>() },
            { Profession.Woodworking, new HashSet<TalentTreeNode>() },
            { Profession.Tailoring, new HashSet<TalentTreeNode>() }
        };

        private static readonly LoggerAdapter log = new(typeof(WorkerTalentTrees));
        
        public WorkerTalentTrees(Player owner, IDictionary<string, object> dbRow)
        {
            this.owner = owner;
            MiningTree = new TalentTree<MiningTalent>(WorkerCreateTreeNodes.GetMiningNodes(dbRow));
            WoodCuttingTree = new TalentTree<WoodCuttingTalent>(WorkerCreateTreeNodes.GetWoodCuttingNodes(dbRow));
            FishingTree = new TalentTree<FishingTalent>(WorkerCreateTreeNodes.GetFishingNodes(dbRow));
            BlacksmithingTree = new TalentTree<BlacksmithingTalent>(WorkerCreateTreeNodes.GetBlacksmithingNodes(dbRow));
            WoodWorkingTree = new TalentTree<WoodWorkingTalent>(WorkerCreateTreeNodes.GetWoodWorkingNodes(dbRow));
            TailoringTree = new TalentTree<TailoringTalent>(WorkerCreateTreeNodes.GetTailoringNodes(dbRow));
        }

        public void WriteAllTalentsCurrentPoints(Packet packet)
        {
            packet.Write((byte)MiningTree.Count);
            foreach (var (talentId, talentNode) in MiningTree)
            {
                packet.Write((byte)talentId);
                packet.Write(talentNode.Points);
            }
            
            packet.Write((byte)WoodCuttingTree.Count);
            foreach (var (talentId, talentNode) in WoodCuttingTree)
            {
                packet.Write((byte)talentId);
                packet.Write(talentNode.Points);
            }
            
            packet.Write((byte)FishingTree.Count);
            foreach (var (talentId, talentNode) in FishingTree)
            {
                packet.Write((byte)talentId);
                packet.Write(talentNode.Points);
            }
            
            packet.Write((byte)BlacksmithingTree.Count);
            foreach (var (talentId, talentNode) in BlacksmithingTree)
            {
                packet.Write((byte)talentId);
                packet.Write(talentNode.Points);
            }
            
            packet.Write((byte)WoodWorkingTree.Count);
            foreach (var (talentId, talentNode) in WoodWorkingTree)
            {
                packet.Write((byte)talentId);
                packet.Write(talentNode.Points);
            }
            
            packet.Write((byte)TailoringTree.Count);
            foreach (var (talentId, talentNode) in TailoringTree)
            {
                packet.Write((byte)talentId);
                packet.Write(talentNode.Points);
            }
        }

        public void WriteLeveledUpTalents(Packet packet)
        {
            foreach (var (profession, skilledUpTalents) in leveledUpTalents)
            {
                packet.Write((byte)profession);
                packet.Write((byte)skilledUpTalents.Count);
                
                foreach (var talent in skilledUpTalents)
                {
                    packet.Write(talent.TalentId);
                    packet.Write(talent.Points);
                }
                
                skilledUpTalents.Clear();
            }
        }
        
        public CanSkillUpTalent SkillUpTalent(Profession profession, byte nodeId)
        {
            if (owner.AvailableTalentPoints <= 0)
                return CanSkillUpTalent.No;
            
            var canSkillUp = CanSkillUpTalentInternal(profession, nodeId, out var node);
            if (canSkillUp == CanSkillUpTalent.InvalidId)
                return canSkillUp;

            if (node.SkillUp())
            {
                leveledUpTalents[profession].Add(node);
                owner.AvailableTalentPoints--;
                return CanSkillUpTalent.Yes;
            }
            
            return CanSkillUpTalent.No;
        }
        
        public void CheckCanSkillUpTalent(Profession profession, byte nodeId)
        {
            if (owner.AvailableTalentPoints <= 0)
            {
                PacketSender.CanSkillUpTalentReturn(owner.Id, profession, nodeId, false);
                return;
            }
            
            var canSkillUp = CanSkillUpTalentInternal(profession, nodeId, out _);
            if (canSkillUp != CanSkillUpTalent.InvalidId)
                PacketSender.CanSkillUpTalentReturn(owner.Id, profession, nodeId, canSkillUp == CanSkillUpTalent.Yes);
        }

        private CanSkillUpTalent CanSkillUpTalentInternal(Profession profession, byte nodeId, out TalentTreeNode node)
        {
            switch (profession)
            {
                case Profession.Mining:
                    return TalentTree.CanLearnMiningNode(owner, MiningTree, (MiningTalent)nodeId, out node);
                case Profession.WoodCutting:
                    return TalentTree.CanLearnWoodCuttingNode(owner, WoodCuttingTree, (WoodCuttingTalent)nodeId, out node);
                case Profession.Fishing:
                    return TalentTree.CanLearnFishingNode(owner, FishingTree, (FishingTalent)nodeId, out node);
                case Profession.Blacksmithing:
                    return TalentTree.CanLearnBlacksmithingNode(owner, BlacksmithingTree, (BlacksmithingTalent)nodeId, out node);
                case Profession.Woodworking:
                    return TalentTree.CanLearnWoodWorkingNode(owner, WoodWorkingTree, (WoodWorkingTalent)nodeId, out node);
                case Profession.Tailoring:
                    return TalentTree.CanLearnTailoringNode(owner, TailoringTree, (TailoringTalent)nodeId, out node);
                default:
                    node = null;
                    log.Warn("Player {0} ({1}) sent an invalid profession to skill up!", owner.name, owner.CharacterInfo.CharacterId);
                    owner.Client.Disconnect();
                    return CanSkillUpTalent.InvalidId;
            }
        }
    }
}