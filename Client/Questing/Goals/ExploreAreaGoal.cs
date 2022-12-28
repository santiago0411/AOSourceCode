using AOClient.Questing.Progress;
using AOClient.UI;
using AOClient.UI.Main.Questing;
using Newtonsoft.Json;

namespace AOClient.Questing.Goals
{
    public class ExploreAreaGoal : IQuestGoal
    {
        [JsonProperty("Id")]
        public byte Id { get; private set; }
        
        [JsonProperty("StepOrder")]
        public byte StepOrder { get; private set; }

        [JsonProperty("Goal")] 
        public int[] ExploreAreaIds { get; private set; }

        
        public void LoadGoal()
        {
            foreach (var area in ExploreAreaIds)
            {
                var goalItem = UIManager.GameUI.QuestWindow.QuestGoalsUI.GetGoalListItem();
                goalItem.SetGoal($"Area Id {area}", 1f);   
            }
        }

        public void LoadGoal(int areaId, GoalListItem goalItem)
        {
            goalItem.SetGoal($"Area Id {areaId}", 1f);
        }

        public IQuestProgress GetNewProgress() => new ExploreAreaProgress(this);
    }
}