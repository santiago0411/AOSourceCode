using System.Collections.ObjectModel;
using AOClient.Core;
using AOClient.Player;
using AOClient.Questing.Progress;
using AOClient.UI;
using AOClient.UI.Main.Questing;
using Newtonsoft.Json;

namespace AOClient.Questing.Goals
{
    public class KillPlayersGoal : IQuestGoal
    {
        [JsonProperty("Id")]
        public byte Id { get; private set; }

        [JsonProperty("StepOrder")]
        public byte StepOrder { get; private set; }
        
        [JsonProperty("Goal")]
        public ReadOnlyDictionary<Faction, ushort> PlayerKillsRequired { get; private set; }

        
        public void LoadGoal()
        {
            foreach (var (faction, killCount) in PlayerKillsRequired)
            {
                var goalItem = UIManager.GameUI.QuestWindow.QuestGoalsUI.GetGoalListItem();
                goalItem.SetGoal(Constants.FactionNames[faction], killCount);
            }
        }

        public void LoadGoal(Faction faction, GoalListItem goalItem)
        {
            goalItem.SetGoal(Constants.FactionNames[faction], PlayerKillsRequired[faction]);
        }

        public IQuestProgress GetNewProgress() => new KillPlayersProgress(this);
    }
}