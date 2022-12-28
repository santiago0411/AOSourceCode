using System.Collections.Generic;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Player;
using AOClient.Questing.Goals;
using AOClient.UI;

namespace AOClient.Questing.Progress
{
    public sealed class KillPlayersProgress : IQuestProgress
    {
        public byte GoalId => goal.Id;
        public byte StepOrder => goal.StepOrder;
        
        private readonly KillPlayersGoal goal;
        private readonly Dictionary<Faction, ushort> kills = new();

        public KillPlayersProgress(KillPlayersGoal goal)
        {
            this.goal = goal;
            kills.InitializeKeys(goal.PlayerKillsRequired.Keys);
        }
        
        public void Dispose()
        {
            kills.Clear();
        }

        public void UpdateProgress(Packet packet)
        {
            var faction = (Faction)packet.ReadByte();
            kills[faction] = packet.ReadUShort();
            UIManager.GameUI.Console.WriteLine($"{faction}: {kills[faction]}/{goal.PlayerKillsRequired[faction]} asesinados.");
        }

        public void LoadGoalAndProgress()
        {
            foreach (var (faction, killCount) in kills)
            {
                var goalItem = UIManager.GameUI.QuestWindow.QuestGoalsUI.GetGoalListItem();
                goal.LoadGoal(faction, goalItem);
                goalItem.SetProgressSlider(killCount);
            }
        }

        public void FullyComplete()
        {
            var tmp = new List<Faction>(kills.Keys);
            foreach (var faction in tmp)
                kills[faction] = goal.PlayerKillsRequired[faction];
        }
    }
}