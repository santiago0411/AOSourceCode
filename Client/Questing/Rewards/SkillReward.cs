using AOClient.Core;
using AOClient.Player;
using AOClient.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace AOClient.Questing.Rewards
{
    public class SkillReward : IQuestReward
    {
        [JsonProperty("Skill")] private readonly Skill skill;
        [JsonProperty("Increase")] private readonly byte increase;
        
        public void AddRewardToPanel(Transform panelTransform)
        {
            var skillReward = UIManager.GameUI.QuestWindow.QuestRewardsUI.SkillRewardInstance;
            skillReward.SkillText.text = $"{Constants.SkillsNames[skill]} +{increase}";
            skillReward.transform.SetParent(panelTransform, false);
        }
    }
}