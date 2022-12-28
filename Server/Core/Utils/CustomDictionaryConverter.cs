using System;
using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using Newtonsoft.Json;

namespace AO.Core.Utils
{
    public class CustomDictionaryConverter<TKey, TValue> : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Dictionary<TKey, TValue>);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, ((IDictionary<TKey, TValue>)value).ToList());

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var result = serializer.Deserialize<Dictionary<string, TValue>>(reader);
            if (result is null)
                return null;

            return typeof(TKey) switch
            {
                { } npcIdType when npcIdType == typeof(NpcId) => NpcId.ParseDictionary(result),
                { } itemIdType when itemIdType == typeof(ItemId) => ItemId.ParseDictionary(result),
                { } spellId when spellId == typeof(SpellId) => SpellId.ParseDictionary(result),
                { } questId when questId == typeof(QuestId) => QuestId.ParseDictionary(result),
                _ => null
            };
        }
    }
}