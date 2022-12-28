using AO.Players;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Requirements
{
    public sealed class SkillRequirement : IQuestRequirement
    {
        [JsonProperty("Skill")]
        private readonly Skill skill;
        
        [JsonProperty("SkillLevel")]
        private readonly byte skillLevel;

        public bool DoesPlayerMeetRequirement(Player player)
        {
            return player.Skills[skill] >= skillLevel;
        }
    }
}