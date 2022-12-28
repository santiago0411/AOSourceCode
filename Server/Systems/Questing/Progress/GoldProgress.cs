using System;
using AO.Players;
using AO.Systems.Questing.Goals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AO.Systems.Questing.Progress
{
    public sealed class GoldProgress : IQuestProgress
    {
        [JsonIgnore] 
        public byte Id => goal.Id;
        
        [JsonIgnore]
        public bool IsCompleted => subscribedToPlayer.Gold >= goal.Gold;
        public Action<Player> TryAdvanceToNextStep { private get; set; }

        private Player subscribedToPlayer;
        private readonly GoldGoal goal;
        
        public GoldProgress(GoldGoal goal)
        {
            this.goal = goal;
        }
        
        public void SubscribeToEvent(Player player)
        {
            subscribedToPlayer = player;
        }

        public void TurnInProgress()
        {
            PlayerMethods.RemoveGold(subscribedToPlayer, goal.Gold);
        }

        public void LoadExistingProgress(JObject progress) { }
        public void SendAllProgressUpdate() { }
        public void Dispose() {}
    }
}