using System.Linq;
using AO.Players;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Requirements
{
    public sealed class ClassRequirement : IQuestRequirement
    {
        [JsonProperty("Classes")]
        private readonly ClassType[] classes;

        public bool DoesPlayerMeetRequirement(Player player) => classes.Contains(player.Class.ClassType);
    }
}