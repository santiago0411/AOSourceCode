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
    public class KillNpcsGoal : IQuestGoal
    {
        [JsonProperty("Id")]
        public byte Id { get; private set; }

        [JsonProperty("StepOrder")]
        public byte StepOrder { get; private set; }
        
        [JsonProperty("Goal")]
        [JsonConverter(typeof(CustomReadOnlyDictionaryConverter<NpcId, ushort>))]
        public ReadOnlyDictionary<NpcId, ushort> NpcsKillsRequired { get; private set; }
        
        public void LoadGoal()
        {
            foreach (var (npcId, killCount) in NpcsKillsRequired)
            {
                var goalItem = UIManager.GameUI.QuestWindow.QuestGoalsUI.GetGoalListItem();
                var npcInfo = GameManager.Instance.GetNpcInfo(npcId);
                goalItem.SetGoal(npcInfo.Name, killCount);
            }
        }

        public void LoadGoal(NpcId npcId, GoalListItem goalItem)
        {
            var npcInfo = GameManager.Instance.GetNpcInfo(npcId);
            goalItem.SetGoal(npcInfo.Name, NpcsKillsRequired[npcId]);
        }

        public IQuestProgress GetNewProgress() => new KillNpcsProgress(this);
    }
}