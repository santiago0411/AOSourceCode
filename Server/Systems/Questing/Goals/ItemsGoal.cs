﻿using System.Collections.ObjectModel;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Systems.Questing.Progress;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Goals
{
    public sealed class ItemsGoal : IQuestGoal
    {
        [JsonProperty("Id")]
        public byte Id { get; private set; }

        [JsonProperty("StepOrder")]
        public byte StepOrder { get; private set; }
        
        [JsonProperty("Goal")]
        [JsonConverter(typeof(CustomReadOnlyDictionaryConverter<ItemId, ushort>))]
        public ReadOnlyDictionary<ItemId, ushort> ItemsRequired { get; private set; }

        public IQuestProgress GetNewProgress(QuestId _) => new ItemProgress(this);
    }
}