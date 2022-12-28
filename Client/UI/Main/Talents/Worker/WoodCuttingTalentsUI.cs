using UnityEngine;

namespace AOClient.UI.Main.Talents.Worker
{
    public class WoodCuttingTalentsUI : MonoBehaviour, ITalentTreeUI
    {
        [SerializeField] private WorkerTalentNodeUI fastCuttingNode;
        [SerializeField] private WorkerTalentNodeUI dropLessWoodNode;
        [SerializeField] private WorkerTalentNodeUI cutElficWoodNode;
        [SerializeField] private WorkerTalentNodeUI sentinelNode;
        
        public TalentNodeUIBase GetTalentNode(byte nodeId)
        {
            return (WoodCuttingTalent)nodeId switch
            {
                WoodCuttingTalent.FastCutting => fastCuttingNode,
                WoodCuttingTalent.DropLessWood => dropLessWoodNode,
                WoodCuttingTalent.CutElficWood => cutElficWoodNode,
                WoodCuttingTalent.SentinelChanceReductionWoodCutting => sentinelNode,
                _ => null
            };
        }
    }
}