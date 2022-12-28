using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Questing.Goals;
using AOClient.UI;

namespace AOClient.Questing.Progress
{
    public sealed class GoldProgress : IQuestProgress
    {
        public byte GoalId => goal.Id;
        public byte StepOrder => goal.StepOrder;

        private bool maxReported;
        
        private readonly GoldGoal goal;
        
        public GoldProgress(GoldGoal goal)
        {
            this.goal = goal;
            var localPlayer = GameManager.Instance.LocalPlayer;
            localPlayer.Events.TotalGoldChanged += OnTotalGoldChanged;
            WriteProgress(localPlayer.Gold, true);
        }

        public void Dispose()
        {
            GameManager.Instance.LocalPlayer.Events.TotalGoldChanged -= OnTotalGoldChanged;
        }
        
        public void LoadGoalAndProgress()
        {
            var goalItem = UIManager.GameUI.QuestWindow.QuestGoalsUI.GetGoalListItem();
            goal.LoadGoal(goalItem);
            goalItem.SetProgressSlider(GameManager.Instance.LocalPlayer.Gold);
        }

        public void FullyComplete()
        {
            
        }

        private void OnTotalGoldChanged(uint gold)
        {
            WriteProgress(gold, false);
        }

        private void WriteProgress(uint currentGold, bool force)
        {
            var requiredGold = goal.Gold;
            
            // If the maximum was already reported but now the player has less, set it back to false so we can start reporting again
            if (maxReported)
                maxReported = currentGold >= requiredGold;
            
            if (force || !maxReported)
            {
                UIManager.GameUI.Console.WriteLine($"{(currentGold > requiredGold ? requiredGold : currentGold)}/{requiredGold} monedas de oro.");
                maxReported = currentGold >= requiredGold;
            }
        }

        public void UpdateProgress(Packet packet) { }
    }
}