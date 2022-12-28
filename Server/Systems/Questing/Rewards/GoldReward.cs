using AO.Players;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Rewards
{
    public sealed class GoldReward : IQuestReward
    {
        [JsonProperty("Gold")]
        private readonly uint goldAmount;

        public void AssignReward(Player toPlayer) => PlayerMethods.AddGold(toPlayer, goldAmount);
    }
}
