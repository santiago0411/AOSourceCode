using System;
using AO.Players;
using Newtonsoft.Json.Linq;

namespace AO.Systems.Questing.Progress
{
    public interface IQuestProgress : IDisposable
    {
        public byte Id { get; }
        public bool IsCompleted { get; }
        public Action<Player> TryAdvanceToNextStep { set; }
        public void SubscribeToEvent(Player player);
        public void TurnInProgress();
        public void LoadExistingProgress(JObject progress);
        public void SendAllProgressUpdate();
    }
}