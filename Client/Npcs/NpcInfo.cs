using System.Collections.Generic;
using AO.Core.Ids;
using Newtonsoft.Json;

namespace AOClient.Npcs
{
    public class NpcInfo
    {
        [JsonProperty("Id")]
        public readonly NpcId Id;
        [JsonProperty("Name")]
        public readonly string Name;
        [JsonProperty("IsStatic")]
        public readonly bool IsStatic;
        [JsonProperty("HeadId")]
        public readonly byte HeadId;
        [JsonProperty("Animation")]
        public readonly ushort Animation;
        [JsonProperty("IsShort")]
        public readonly bool IsShort;
        [JsonProperty("Description")]
        public readonly string Description = string.Empty;

        public readonly List<QuestId> AvailableQuests = new();
        public readonly List<QuestId> TurnInableQuests = new();
    }
}
