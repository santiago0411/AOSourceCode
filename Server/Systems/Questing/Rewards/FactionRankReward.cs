using AO.Players;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Rewards
{
    public sealed class FactionRankReward : IQuestReward
    {
        [JsonProperty("Rank")] 
        private readonly FactionRank rank;

        public void AssignReward(Player toPlayer)
        {
            toPlayer.FactionRank = rank;
        }
    }
}