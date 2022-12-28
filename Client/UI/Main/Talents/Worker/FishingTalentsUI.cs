using UnityEngine;

namespace AOClient.UI.Main.Talents.Worker
{
    public class FishingTalentsUI : MonoBehaviour, ITalentTreeUI
    {
        [SerializeField] private WorkerTalentNodeUI fishPejerreyNode;
        [SerializeField] private WorkerTalentNodeUI fishHakeNode;
        [SerializeField] private WorkerTalentNodeUI fishSwordFishNode;
        [SerializeField] private WorkerTalentNodeUI useFishingNetNode;
        [SerializeField] private WorkerTalentNodeUI galleyNode;
        [SerializeField] private WorkerTalentNodeUI schoolFishingNode;
        [SerializeField] private WorkerTalentNodeUI sentinelNode;
        
        public TalentNodeUIBase GetTalentNode(byte nodeId)
        {
            return (FishingTalent)nodeId switch
            {
                FishingTalent.FishPejerrey => fishPejerreyNode,
                FishingTalent.FishHake => fishHakeNode,
                FishingTalent.FishSwordFish => fishSwordFishNode,
                FishingTalent.UseFishingNet => useFishingNetNode,
                FishingTalent.GalleyFishing => galleyNode,
                FishingTalent.SchoolFishing => schoolFishingNode,
                FishingTalent.SentinelChanceReductionFishing => sentinelNode,
                _ => null
            };
        }
    }
}