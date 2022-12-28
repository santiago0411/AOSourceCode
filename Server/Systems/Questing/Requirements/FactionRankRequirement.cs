using AO.Players;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Requirements
{
    public sealed class FactionRankRequirement : IQuestRequirement
    {
        [JsonProperty("Faction")]
        private readonly Faction faction;

        [JsonProperty("Rank")] 
        private readonly FactionRank rank;

        public bool DoesPlayerMeetRequirement(Player player) => player.Faction == faction && (int)player.FactionRank >= (int)rank;
    }
}