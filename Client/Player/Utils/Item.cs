using AO.Core.Ids;
using Newtonsoft.Json;

namespace AOClient.Player.Utils
{ 
    public class Item
    {
        [JsonProperty("Id")]
        public readonly ItemId Id;
        [JsonProperty("Name")]
        public readonly string Name;
        [JsonProperty("Description")]
        public readonly string Description = string.Empty;
        [JsonProperty("GraphicId")]
        public readonly ushort GraphicId;
        [JsonProperty("ItemType")]
        public readonly ItemType ItemType;
        [JsonProperty("AnimationId")]
        public readonly ushort AnimationId;
    }
}

