using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Network;
using AO.Systems.Questing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AO.Players.Utils
{
    public class PlayerQuestManager
    {
        public int QuestsCount => activeQuests.Count;
        
        private readonly Dictionary<QuestId, QuestSteps> activeQuests = new(Constants.PLAYER_MAX_QUESTS);
        private readonly HashSet<QuestId> questsCompleted;

        public PlayerQuestManager(Player player, string questsProgressesJson, string questsCompletedJson)
        {
            DeserializeCurrentProgresses(player, questsProgressesJson);
            questsCompleted = JsonConvert.DeserializeObject<HashSet<QuestId>>(questsCompletedJson);
        }

        public bool IsOnQuest(QuestId questId) => activeQuests.ContainsKey(questId);

        public void AddQuest(QuestId questId, QuestSteps steps) => activeQuests.Add(questId, steps);

        public QuestSteps GetQuestSteps(QuestId questId) => activeQuests[questId];

        public void RemoveQuest(QuestId questId) => activeQuests.Remove(questId);

        public void AddQuestCompleted(QuestId questId) => questsCompleted.Add(questId);
        
        public bool QuestCompleted(QuestId questId) => questsCompleted.Contains(questId);

        public bool AllQuestsCompleted(QuestId[] questsIds) => questsIds.All(questsCompleted.Contains);

        public bool HasCompletedQuest(QuestId questId) => questsCompleted.Contains(questId);

        public void DisposeAllQuests()
        {
            foreach (var quest in activeQuests.Values)
                quest.DisposeCurrentProgresses();
        }

        public string SerializeCurrentProgresses()
        {
            var toSerialize = new List<object>(Constants.PLAYER_MAX_QUESTS);
            foreach (var (questId, questSteps) in activeQuests)
                toSerialize.Add(new
                {
                    QuestId = questId,
                    CurrentStep = questSteps.CurrentStep,
                    Progresses = questSteps.CurrentProgresses
                });
            
            return JsonConvert.SerializeObject(toSerialize);
        }

        public string SerializeCompletedQuests()
        {
            return JsonConvert.SerializeObject(questsCompleted);
        }

        private void DeserializeCurrentProgresses(Player player, string currentProgressesJson)
        {
            JArray progresses = JArray.Parse(currentProgressesJson);
            foreach (var progressObj in progresses)
            {
                var questId = progressObj.Value<ushort>("QuestId");
                var currentStep = progressObj.Value<byte>("CurrentStep");
                var steps = QuestManager.GetQuest(questId).GetNewSteps(player, currentStep);
                var questProgresses = progressObj.Value<JArray>("Progresses");
                activeQuests.Add(questId, steps);
                foreach (var progress in steps.CurrentProgresses)
                {
                    var obj = questProgresses!.First(p => p.Value<byte>("Id") == progress.Id);
                    progress.LoadExistingProgress(obj.Value<JObject>("Progress"));
                }
            }
        }

        public void ReportAllQuests(Player player)
        {
            foreach (var (questId, activeStep) in activeQuests)
            {
                var turnInNpcs = QuestManager.GetQuestTurnInNpcs(questId);
                PacketSender.QuestAssigned(player.Id, questId, turnInNpcs.Count > 0, turnInNpcs, activeStep.CurrentStep);
                foreach (var progress in activeStep.CurrentProgresses)
                    progress.SendAllProgressUpdate();
            }
        }
    }
}