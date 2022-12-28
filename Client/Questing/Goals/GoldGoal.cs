using AOClient.Questing.Progress;
using AOClient.UI;
using AOClient.UI.Main.Questing;
using Newtonsoft.Json;

namespace AOClient.Questing.Goals
{
    public class GoldGoal : IQuestGoal
    {
        [JsonProperty("Id")]
        public byte Id { get; private set; }

        [JsonProperty("StepOrder")]
        public byte StepOrder { get; private set; }

        [JsonProperty("Goal")] 
        public uint Gold { get; private set; }
        
        public void LoadGoal()
        {
            var goalItem = UIManager.GameUI.QuestWindow.QuestGoalsUI.GetGoalListItem();
            goalItem.SetGoal("Oro", Gold);
        }

        public void LoadGoal(GoalListItem goalItem)
        {
            goalItem.SetGoal("Oro", Gold);
        }

        public IQuestProgress GetNewProgress() => new GoldProgress(this);
    }
}