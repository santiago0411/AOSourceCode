using System;
using System.Collections.Generic;
using System.Linq;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Logging;
using AO.Systems.Questing.Goals;
using AO.Systems.Questing.Requirements;
using AO.Systems.Questing.Rewards;
using Newtonsoft.Json;

namespace AO.Systems.Questing
{
    public static class QuestJsonUtils
    {
        private static readonly LoggerAdapter log = new(typeof(QuestJsonUtils));
        
        private static readonly JsonSerializerSettings jss = new()
        {
            TypeNameHandling = TypeNameHandling.All, 
            Converters = new List<JsonConverter>()
            {
                new ItemId.ItemIdJsonConverter(),
                new NpcId.NpcIdJsonConverter(),
                new SpellId.SpellIdJsonConverter(),
                new QuestId.QuestIdJsonConverter(),
            }
        };

        public static (IQuestRequirement[], IQuestGoal[][], IQuestReward[]) DeserializeQuestJsons(QuestId questId, string requirementsJson, string goalsJson, string rewardsJson)
        {
            try
            {
                var requirements = JsonConvert.DeserializeObject<IQuestRequirement[]>(requirementsJson ?? string.Empty, jss);
                var goals = JsonConvert.DeserializeObject<IQuestGoal[]>(goalsJson, jss);
                var orderedGoals = goals!.GroupBy(goal => goal.StepOrder).Select(group => group.ToArray()).ToArray();
                var rewards = JsonConvert.DeserializeObject<IQuestReward[]>(rewardsJson, jss);
                return (requirements ?? Array.Empty<IQuestRequirement>(), orderedGoals, rewards);
            }
            catch (Exception ex)
            {
                AoDebug.Assert(false, "Failed to deserialize quest.");
                log.Error("Error deserializing quest {0}. {1}", questId, ex.Message);
                GameManager.CloseApplication();
                return (null, null, null);
            }
        }
    }
}