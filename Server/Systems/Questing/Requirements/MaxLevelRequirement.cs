using AO.Players;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Requirements
{
    public sealed class MaxLevelRequirement : IQuestRequirement
    {
        [JsonProperty("MaxLevel")]
        private readonly byte maxLevel;

        public bool DoesPlayerMeetRequirement(Player player) => player.Level <= maxLevel;
    }
}