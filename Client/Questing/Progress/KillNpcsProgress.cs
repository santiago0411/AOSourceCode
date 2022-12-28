using System.Collections.Generic;
using AO.Core.Ids;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Questing.Goals;
using AOClient.UI;

namespace AOClient.Questing.Progress
{
    public sealed class KillNpcsProgress : IQuestProgress
    {
        public byte GoalId => goal.Id;
        public byte StepOrder => goal.StepOrder;
        
        private readonly KillNpcsGoal goal;
        private readonly Dictionary<NpcId, ushort> kills = new();

        public KillNpcsProgress(KillNpcsGoal goal)
        {
            this.goal = goal;
            kills.InitializeKeys(goal.NpcsKillsRequired.Keys);
        }

        public void Dispose()
        {
            kills.Clear();
        }

        public void UpdateProgress(Packet packet)
        {
            var npcId = packet.ReadUShort();
            kills[npcId] = packet.ReadUShort();
            UIManager.GameUI.Console.WriteLine($"{GameManager.Instance.GetNpcInfo(npcId).Name}: {kills[npcId]}/{goal.NpcsKillsRequired[npcId]}");
        }

        public void LoadGoalAndProgress()
        {
            foreach (var (npcId, killCount) in kills)
            {
                var goalItem = UIManager.GameUI.QuestWindow.QuestGoalsUI.GetGoalListItem();
                goal.LoadGoal(npcId, goalItem);
                goalItem.SetProgressSlider(killCount);
            }
        }

        public void FullyComplete()
        {
            var tmp = new List<NpcId>(kills.Keys);
            foreach (var npcId in tmp)
                kills[npcId] = goal.NpcsKillsRequired[npcId];
        }
    }
}