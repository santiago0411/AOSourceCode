using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AO.Core.Ids;
using Newtonsoft.Json;

namespace AO.Core.Utils
{
    public class CustomReadOnlyDictionaryConverter<TKey, TValue> : JsonConverter
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
                { } npcIdType when npcIdType == typeof(NpcId) => new ReadOnlyDictionary<NpcId, TValue>(NpcId.ParseDictionary(result)),
                { } itemIdType when itemIdType == typeof(ItemId) => new ReadOnlyDictionary<ItemId, TValue>(ItemId.ParseDictionary(result)),
                { } spellId when spellId == typeof(SpellId) => new ReadOnlyDictionary<SpellId, TValue>(SpellId.ParseDictionary(result)),
                { } questId when questId == typeof(QuestId) => new ReadOnlyDictionary<QuestId, TValue>(QuestId.ParseDictionary(result)),
                _ => null
            };
        }
    }
}