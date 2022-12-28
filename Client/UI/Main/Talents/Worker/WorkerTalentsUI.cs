using AOClient.Core;
using AOClient.Core.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Talents.Worker
{
    public class WorkerTalentsUI : MonoBehaviour
    {
        [Header("Buttons")] 
        [SerializeField] private Button showMiningButton;
        [SerializeField] private Button showWoodCuttingButton;
        [SerializeField] private Button showFishingButton;
        [SerializeField] private Button showBlacksmithingButton;
        [SerializeField] private Button showWoodWorkingButton;
        [SerializeField] private Button showTailoringButton;

        [Header("Text")] 
        [SerializeField] private TextMeshProUGUI displayingTreeText;
        
        [Header("Talent Trees")]
        [SerializeField] private MiningTalentsUI miningTalents;
        [SerializeField] private WoodCuttingTalentsUI woodCuttingTalents;
        [SerializeField] private FishingTalentsUI fishingTalents;

        private GameObject treeOnDisplay;
        
        private void Start()
        {
            miningTalents.gameObject.SetActive(false);
            woodCuttingTalents.gameObject.SetActive(false);
            fishingTalents.gameObject.SetActive(false);

            showMiningButton.onClick.AddListener(() => SwapActiveTree(miningTalents.gameObject, Constants.MINING_TREE_NAME));
            showWoodCuttingButton.onClick.AddListener(() => SwapActiveTree(woodCuttingTalents.gameObject, Constants.WOODCUTTING_TREE_NAME));
            showFishingButton.onClick.AddListener(() => SwapActiveTree(fishingTalents.gameObject, Constants.FISHING_TREE_NAME));
            
            showMiningButton.onClick.Invoke();
        }

        private void SwapActiveTree(GameObject newTree, string treeName)
        {
            if (treeOnDisplay)
                treeOnDisplay.SetActive(false);
            treeOnDisplay = newTree;
            treeOnDisplay.SetActive(true);
            displayingTreeText.text = treeName;
        }

        private TalentNodeUIBase GetNode(Profession profession, byte nodeId)
        {
            return profession switch
            {
                Profession.Mining => miningTalents.GetTalentNode(nodeId),
                Profession.WoodCutting => woodCuttingTalents.GetTalentNode(nodeId),
                Profession.Fishing => fishingTalents.GetTalentNode(nodeId),
                Profession.Blacksmithing => null,
                Profession.Woodworking => null,
                Profession.Tailoring => null,
                _ => null
            };
        }

        public void SetNodeCurrentPoints(Profession profession, byte nodeId, byte currentPoints)
        {
            var node = GetNode(profession, nodeId);
            if (node)
                node.UpdateCurrentPoints(currentPoints);
        }

        public void CanSkillUpReturn(Profession profession, byte nodeId, bool canSkillUp)
        {
            var node = GetNode(profession, nodeId);
            if (node)
                node.OnCanSkillUpReturn(canSkillUp);
        }
    }
}