using AO.Core.Ids;
using Newtonsoft.Json;

namespace AOClient.Player.Utils
{ 
    public class Spell
    {
        public byte Slot = 0;
        [JsonProperty("Id")]
        public readonly SpellId Id;
        [JsonProperty("Name")]
        public readonly string Name;
        [JsonProperty("Description")]
        public readonly string Description;
        [JsonProperty("MagicWords")]
        public readonly string MagicWords;
        [JsonProperty("Sound")]
        public readonly ushort Sound;
        [JsonProperty("Particle")]
        public readonly ushort Particle;
        [JsonProperty("MinSkill")]
        public readonly byte MinSkill;
        [JsonProperty("ManaRequired")]
        public readonly short ManaRequired;
        [JsonProperty("StamRequired")]
        public readonly short StamRequired;
        [JsonProperty("Message")]
        public readonly string Message;
        [JsonProperty("TargetMessage")]
        public readonly string TargetMessage;
        [JsonProperty("SelfMessage")]
        public readonly string SelfMessage;
    }
}