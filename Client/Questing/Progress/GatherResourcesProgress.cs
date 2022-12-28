using System.Collections.Generic;
using AO.Core.Ids;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Questing.Goals;
using AOClient.UI;

namespace AOClient.Questing.Progress
{
    public sealed class GatherResourcesProgress : IQuestProgress
    {
        public byte GoalId => goal.Id;
        public byte StepOrder => goal.StepOrder;
        
        private readonly GatherResourcesGoal goal;
        private readonly Dictionary<ItemId, ushort> resourcesGathered = new();

        public GatherResourcesProgress(GatherResourcesGoal goal)
        {
            this.goal = goal;
            resourcesGathered.InitializeKeys(goal.ResourcesRequired.Keys);
        }
        
        public void Dispose()
        {
            resourcesGathered.Clear();
        }

        public void UpdateProgress(Packet packet)
        {
            var itemId = packet.ReadUShort();
            resourcesGathered[itemId] = packet.ReadUShort();
            var currentAmount = resourcesGathered[itemId];
            var requiredAmount = goal.ResourcesRequired[itemId];
            UIManager.GameUI.Console.WriteLine($"{GameManager.Instance.GetItem(itemId).Name}: {(currentAmount > requiredAmount ? requiredAmount : currentAmount)}/{requiredAmount} conseguidos.");
        }

        public void LoadGoalAndProgress()
        {
            foreach (var (itemId, amount) in resourcesGathered)
            {
                var goalItem = UIManager.GameUI.QuestWindow.QuestGoalsUI.GetGoalListItem();
                goal.LoadGoal(itemId, goalItem);
                goalItem.SetProgressSlider(amount);
            }
        }

        public void FullyComplete()
        {
            var tmp = new List<ItemId>(resourcesGathered.Keys);
            foreach (var itemId in tmp)
                resourcesGathered[itemId] = goal.ResourcesRequired[itemId];
        }
    }
}