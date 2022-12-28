using AO.Players;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Requirements
{
    public sealed class MinLevelRequirement : IQuestRequirement
    {
        [JsonProperty("MinLevel")]
        private readonly byte minLevel;
        
        public bool DoesPlayerMeetRequirement(Player player) => player.Level >= minLevel;
    }
}