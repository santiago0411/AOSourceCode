using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Questing;
using AOClient.Questing.Progress;
using AOClient.UI;

namespace AOClient.Player.Utils
{
    public class QuestManager
    {
        private readonly Dictionary<QuestId, IQuestProgress[]> activeQuests = new();

        public void AddNewQuest(QuestId questId, bool autoComplete, byte startFromStep)
        {
            if (activeQuests.ContainsKey(questId))
                return;
            
            Quest quest = GameManager.Instance.GetQuest(questId);
            IQuestProgress[] progresses = quest.Goals.Select(g => g.GetNewProgress()).ToArray();
            activeQuests.Add(questId, progresses);
            
            foreach (var prog in progresses.Where(p => p.StepOrder < startFromStep))
                prog.FullyComplete();

            UIManager.GameUI.Console.WriteLine(Constants.QuestAccepted(quest.Name));
            
            if (autoComplete)
                UIManager.GameUI.QuestWindow.ShowCompleteButton();
            //else
                //UIManager.GameUI.QuestWindow.RemoveQuest(questId);
        }

        public void CompleteQuest(QuestId questId)
        {
            var progresses = activeQuests[questId];
            activeQuests.Remove(questId);
            progresses.ForEach(p => p.Dispose());
            UIManager.GameUI.QuestWindow.RemoveQuest(questId);
            UIManager.GameUI.Console.WriteLine(Constants.QUEST_COMPLETED);
        }

        public void UpdateProgress(QuestId questId, Packet packet)
        {
            var progresses = activeQuests[questId];
            while (packet.UnreadLength() > 0)
            {
                byte goalId = packet.ReadByte();
                progresses.First(p => p.GoalId == goalId).UpdateProgress(packet);
            }
        }

        public bool IsPlayerOnQuest(QuestId questId) => activeQuests.ContainsKey(questId);
        public IQuestProgress[] GetQuestProgresses(QuestId questId) => activeQuests[questId];
        public IEnumerable<QuestId> GetAllQuests() => activeQuests.Keys;
    }
}