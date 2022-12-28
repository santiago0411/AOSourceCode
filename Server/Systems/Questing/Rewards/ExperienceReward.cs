using AO.Players;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Rewards
{
    public sealed class ExperienceReward : IQuestReward
    {
        [JsonProperty("Experience")]
        private readonly uint experience;
        
        public void AssignReward(Player toPlayer) => PlayerMethods.AddExperience(toPlayer, experience);
    }
}