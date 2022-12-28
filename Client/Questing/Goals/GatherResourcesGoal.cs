using System.Collections.ObjectModel;
using AO.Core.Ids;
using AO.Core.Utils;
using AOClient.Core;
using AOClient.Questing.Progress;
using AOClient.UI;
using AOClient.UI.Main.Questing;
using Newtonsoft.Json;

namespace AOClient.Questing.Goals
{
    public class GatherResourcesGoal : IQuestGoal
    {
        [JsonProperty("Id")]
        public byte Id { get; private set; }

        [JsonProperty("StepOrder")]
        public byte StepOrder { get; private set; }
        
        [JsonProperty("Goal")]
        [JsonConverter(typeof(CustomReadOnlyDictionaryConverter<ItemId, ushort>))]
        public ReadOnlyDictionary<ItemId, ushort> ResourcesRequired { get; private set; }
        
        public void LoadGoal()
        {
            foreach (var (itemId, amount) in ResourcesRequired)
            {
                var goalItem = UIManager.GameUI.QuestWindow.QuestGoalsUI.GetGoalListItem();
                var item  = GameManager.Instance.GetItem(itemId);
                goalItem.SetGoal(item.Name, amount);
            }
        }

        public void LoadGoal(ItemId itemId, GoalListItem goalItem)
        {
            var item  = GameManager.Instance.GetItem(itemId);
            goalItem.SetGoal(item.Name, ResourcesRequired[itemId]);
        }

        public IQuestProgress GetNewProgress() => new GatherResourcesProgress(this);
    }
}