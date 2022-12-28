using System;
using AO.Players;
using AO.Systems.Questing.Goals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AO.Systems.Questing.Progress
{
    public sealed class ItemProgress : IQuestProgress
    {
        [JsonIgnore] 
        public byte Id => goal.Id;
        [JsonIgnore]
        public bool IsCompleted => IsGoalCompleted();
        public Action<Player> TryAdvanceToNextStep { private get; set; }

        private Player subscribedToPlayer;
        private readonly ItemsGoal goal;

        public ItemProgress(ItemsGoal goal)
        {
            this.goal = goal;
        }
        
        public void SubscribeToEvent(Player player)
        {
            subscribedToPlayer = player;
        }

        public void TurnInProgress()
        {
            foreach (var (itemId, quantity) in goal.ItemsRequired)
                subscribedToPlayer.Inventory.RemoveQuantityFromInventory(itemId, quantity);
        }

        public void LoadExistingProgress(JObject progress) {}
        public void SendAllProgressUpdate() { }

        private bool IsGoalCompleted()
        {
            foreach (var (itemId, quantity) in goal.ItemsRequired)
                if (subscribedToPlayer.Inventory.TotalItemQuantity(itemId) < quantity)
                    return false;

            return true;
        }
        
        public void Dispose() {}
    }
}