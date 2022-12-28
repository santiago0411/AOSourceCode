using AO.Core.Ids;
using AO.Systems.Questing.Progress;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Goals
{
    public sealed class ExploreAreaGoal : IQuestGoal
    {
        [JsonProperty("Id")]
        public byte Id { get; private set; }
        
        [JsonProperty("StepOrder")]
        public byte StepOrder { get; private set; }

        [JsonProperty("Goal")] 
        public int[] ExploreAreaIds { get; private set; }
        
        public IQuestProgress GetNewProgress(QuestId questId) => new ExploreAreaProgress(questId, this);
    }
}