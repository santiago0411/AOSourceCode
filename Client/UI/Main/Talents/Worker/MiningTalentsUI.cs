using UnityEngine;

namespace AOClient.UI.Main.Talents.Worker
{
    public class MiningTalentsUI : MonoBehaviour, ITalentTreeUI
    {
        [SerializeField] private WorkerTalentNodeUI fastMiningNode;
        [SerializeField] private WorkerTalentNodeUI dropLessOreNode;
        [SerializeField] private WorkerTalentNodeUI mineSilverNode;
        [SerializeField] private WorkerTalentNodeUI mineGoldNode;
        [SerializeField] private WorkerTalentNodeUI sentinelNode;
        
        public TalentNodeUIBase GetTalentNode(byte nodeId)
        {
            return (MiningTalent)nodeId switch
            {
                MiningTalent.FastMining => fastMiningNode,
                MiningTalent.DropLessOre => dropLessOreNode,
                MiningTalent.MineSilver => mineSilverNode,
                MiningTalent.MineGold => mineGoldNode,
                MiningTalent.SentinelChanceReductionMining => sentinelNode,
                _ => null
            };
        }
    }
}