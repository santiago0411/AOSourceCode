using AOClient.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace AOClient.Questing.Rewards
{
    public class GoldReward : IQuestReward
    {
        [JsonProperty("Gold")] private uint gold;
        
        public void AddRewardToPanel(Transform panelTransform)
        {
            var goldReward = UIManager.GameUI.QuestWindow.QuestRewardsUI.GoldRewardInstance;
            goldReward.AmountField.text = gold.ToString();
            goldReward.transform.SetParent(panelTransform, false);
        }
    }
}