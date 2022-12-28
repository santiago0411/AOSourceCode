using AOClient.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace AOClient.Questing.Rewards
{
    public class TalentPointReward : IQuestReward
    {
        [JsonProperty("Points")] private readonly byte points;
        
        public void AddRewardToPanel(Transform panelTransform)
        {
            var talentReward = UIManager.GameUI.QuestWindow.QuestRewardsUI.TalentPointRewardInstance;
            talentReward.PointsText.text = $"Puntos de talento +{points}";
            talentReward.transform.SetParent(panelTransform, false);
        }
    }
}