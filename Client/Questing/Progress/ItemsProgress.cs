using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Player;
using AOClient.Player.Utils;
using AOClient.Questing.Goals;
using AOClient.UI;

namespace AOClient.Questing.Progress
{
    public sealed class ItemProgress : IQuestProgress
    {
        private static  PlayerManager LocalPlayer => GameManager.Instance.LocalPlayer;
        
        public byte GoalId => goal.Id;
        public byte StepOrder => goal.StepOrder;

        private bool maxReported;
        
        private readonly ItemsGoal goal;
        private readonly Dictionary<ItemId, int> itemsRequired = new();
        
        public ItemProgress(ItemsGoal goal)
        {
            this.goal = goal;
            itemsRequired.InitializeKeys(goal.ItemsRequired.Keys);
            LocalPlayer.Events.InventorySlotChanged += OnInventorySlotChanged;
            
            foreach (var itemId in goal.ItemsRequired.Keys!)
            {
                Inventory[] slots = LocalPlayer.GetSlotsWithItem(itemId);
                int amount = slots.Sum(s => s.Quantity);
                itemsRequired[itemId] = amount;
                WriteProgress(itemId, true);
            }
        }
        
        public void LoadGoalAndProgress()
        {
            foreach (var (itemId, amount) in itemsRequired)
            {
                var goalItem = UIManager.GameUI.QuestWindow.QuestGoalsUI.GetGoalListItem();
                goal.LoadGoal(itemId, goalItem);
                goalItem.SetProgressSlider(amount);
            }
        }

        public void FullyComplete()
        {
            var tmp = new List<ItemId>(itemsRequired.Keys);
            foreach (var itemId in tmp)
                itemsRequired[itemId] = goal.ItemsRequired[itemId];
        }

        private void OnInventorySlotChanged(Inventory slot)
        {
            ItemId itemId = slot.Item.Id;
            
            if (!goal.ItemsRequired.ContainsKey(itemId))
                return;
            
            Inventory[] slots = LocalPlayer.GetSlotsWithItem(itemId);
            int amount = slots.Sum(s => s.Quantity);
            itemsRequired[itemId] = amount;
            WriteProgress(itemId, false);
        }

        private void WriteProgress(ItemId itemId, bool force)
        {
            var currentAmount = itemsRequired[itemId];
            var requiredAmount = goal.ItemsRequired[itemId];

            // If the maximum was already reported but now the player has less, set it back to false so we can start reporting again
            if (maxReported)
                maxReported = currentAmount >= requiredAmount;
            
            if (force || !maxReported)
            {
                UIManager.GameUI.Console.WriteLine($"{GameManager.Instance.GetItem(itemId).Name}: {(currentAmount > requiredAmount ? requiredAmount : currentAmount)}/{requiredAmount}.");
                maxReported = currentAmount >= requiredAmount;
            }
        }

        public void Dispose()
        {
            LocalPlayer.Events.InventorySlotChanged -= OnInventorySlotChanged;
            itemsRequired.Clear();
        }
        
        public void UpdateProgress(Packet packet) { }
    }
}