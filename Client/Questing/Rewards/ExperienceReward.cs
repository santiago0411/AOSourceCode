using AOClient.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace AOClient.Questing.Rewards
{
    public class ExperienceReward : IQuestReward
    {
        [JsonProperty("Experience")] private uint experience;
        
        public void AddRewardToPanel(Transform panelTransform)
        {
            var expReward = UIManager.GameUI.QuestWindow.QuestRewardsUI.ExperienceRewardInstance;
            expReward.ExperienceValueText.text = experience.ToString();
            expReward.transform.SetParent(panelTransform, false);
        }
    }
}