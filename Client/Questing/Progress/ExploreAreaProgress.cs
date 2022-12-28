using System.Collections.Generic;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Questing.Goals;
using AOClient.UI;

namespace AOClient.Questing.Progress
{
    public sealed class ExploreAreaProgress : IQuestProgress
    {
        public byte GoalId => goal.Id;
        public byte StepOrder => goal.StepOrder;

        private readonly ExploreAreaGoal goal;
        private readonly Dictionary<int, bool> exploredAreas = new();

        public ExploreAreaProgress(ExploreAreaGoal goal)
        {
            this.goal = goal;
            exploredAreas.InitializeKeys(goal.ExploreAreaIds);
        }

        public void Dispose()
        {
            exploredAreas.Clear();
        }

        public void UpdateProgress(Packet packet)
        {
            var areaId = packet.ReadInt();
            var explored = packet.ReadBool();
            if (explored)
            {
                exploredAreas[areaId] = true;
                UIManager.GameUI.Console.WriteLine($"Area {areaId} explorada!");
            }
        }

        public void LoadGoalAndProgress()
        {
            foreach (var (areaId, explored) in exploredAreas)
            {
                var goalItem = UIManager.GameUI.QuestWindow.QuestGoalsUI.GetGoalListItem();
                goal.LoadGoal(areaId, goalItem);
                goalItem.SetProgressSlider(explored ? 1f : 0f);
            }
        }

        public void FullyComplete()
        {
            var tmp = new List<int>(exploredAreas.Keys);
            foreach (var areaId in tmp)
                exploredAreas[areaId] = true;
        }
    }
}