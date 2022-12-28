using System.Collections.ObjectModel;
using AO.Core.Ids;
using AO.Players;
using AO.Systems.Questing.Progress;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Goals
{
    public sealed class KillPlayersGoal : IQuestGoal
    {
        [JsonProperty("Id")]
        public byte Id { get; private set; }

        [JsonProperty("StepOrder")]
        public byte StepOrder { get; private set; }
        
        [JsonProperty("Goal")]
        public ReadOnlyDictionary<Faction, ushort> PlayerKillsRequired { get; private set; }

        public IQuestProgress GetNewProgress(QuestId questId) => new KillPlayersProgress(questId, this);
    }
}