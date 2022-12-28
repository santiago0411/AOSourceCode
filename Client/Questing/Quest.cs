using System.Collections.ObjectModel;
using AO.Core.Ids;
using AOClient.Questing.Goals;
using AOClient.Questing.Rewards;
using Newtonsoft.Json;

namespace AOClient.Questing
{
    public class Quest
    {
        [JsonProperty("Id")]
        public readonly QuestId Id;
        
        [JsonProperty("Name")]
        public readonly string Name;
        
        [JsonProperty("Description")]
        public readonly string Description;
        
        [JsonProperty("Goals")]
        public readonly ReadOnlyCollection<IQuestGoal> Goals;

        [JsonProperty("Rewards")] 
        public readonly ReadOnlyCollection<IQuestReward> Rewards;
    }
}