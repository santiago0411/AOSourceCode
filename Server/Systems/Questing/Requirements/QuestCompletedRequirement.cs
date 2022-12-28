using AO.Core.Ids;
using AO.Players;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Requirements
{
    public sealed class QuestCompletedRequirement : IQuestRequirement
    {
        [JsonProperty("QuestIds")]
        private readonly QuestId[] questsIds;

        public bool DoesPlayerMeetRequirement(Player player) => player.QuestManager.AllQuestsCompleted(questsIds);
    }
}