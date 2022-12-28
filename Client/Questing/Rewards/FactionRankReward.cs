using AOClient.Core;
using AOClient.Player;
using AOClient.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace AOClient.Questing.Rewards
{
    public class FactionRankReward : IQuestReward
    {
        [JsonProperty("Rank")] private readonly FactionRank factionRank;
        
        public void AddRewardToPanel(Transform panelTransform)
        {
            var factionRankReward = UIManager.GameUI.QuestWindow.QuestRewardsUI.FactionRankRewardInstance;
            string rankName = GameManager.Instance.LocalPlayer.Faction == Faction.Chaos
                ? Constants.ChaosFactionNames[factionRank]
                : Constants.ImperialFactionNames[factionRank];
            
            factionRankReward.RankTextField.text = rankName;
            factionRankReward.transform.SetParent(panelTransform, false);
        }
    }
}