using System;
using System.Collections.Generic;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Players;
using AO.Systems.Questing.Goals;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Questing.Progress
{
    public sealed class ExploreAreaProgress : IQuestProgress
    {
        [JsonIgnore]
        public bool IsCompleted { get; private set; }
        public Action<Player> TryAdvanceToNextStep { private get; set; }

        private Player subscribedToPlayer;
        private readonly ExploreAreaGoal goal;
        private readonly QuestId questId;

        [JsonProperty("Id")] 
        public byte Id => goal.Id;
        [JsonProperty("Progress")]
        private readonly Dictionary<int, bool> exploredAreas = new();

        public ExploreAreaProgress(QuestId questId, ExploreAreaGoal goal)
        {
            this.questId = questId;
            this.goal = goal;
            exploredAreas.InitializeKeys(goal.ExploreAreaIds);
        }
        
        public void SubscribeToEvent(Player player)
        {
            subscribedToPlayer = player;
            player.Events.PlayerEnteredExploreArea += OnPlayerEnteredExploreArea;
        }

        public void TurnInProgress() { }
        public void LoadExistingProgress(JObject progress)
        {
            foreach (var (key, value) in progress)
                exploredAreas[Convert.ToInt32(key)] = Convert.ToBoolean(value);
        }

        public void SendAllProgressUpdate()
        {
            foreach (var (areaId, explored) in exploredAreas)
                UpdateProgress(areaId, explored);
        }

        private void UpdateProgress(int areaId, bool explored)
        {
            // Pass the data to write in the packet in a struct to avoid closure allocation
            var callbackData = new WritePacketCallbackData(goal.Id, areaId, explored);
            PacketSender.QuestProgressUpdate(subscribedToPlayer.Id, questId, callbackData, 
                (packet, data) =>
                {
                    packet.Write(data.GoalId);
                    packet.Write(data.AreaId);
                    packet.Write(data.Explored);
                });
        }

        private void OnPlayerEnteredExploreArea(int areaId)
        {
            if (!exploredAreas.ContainsKey(areaId) || exploredAreas[areaId])
                return;
            
            exploredAreas[areaId] = true;
            
            UpdateProgress(areaId, true);
            
            if (IsGoalCompleted())
            {
                IsCompleted = true;
                Dispose();
                TryAdvanceToNextStep(subscribedToPlayer);
            }
        }

        private bool IsGoalCompleted()
        {
            foreach (var entry in exploredAreas)
                if (!entry.Value)
                    return false;

            return true;
        }

        public void Dispose()
        {
            subscribedToPlayer.Events.PlayerEnteredExploreArea -= OnPlayerEnteredExploreArea;
            exploredAreas.Clear();
        }
        
        private readonly struct WritePacketCallbackData
        {
            public readonly byte GoalId;
            public readonly int AreaId;
            public readonly bool Explored;

            public WritePacketCallbackData(byte goalId, int areaId, bool explored)
            {
                GoalId = goalId;
                AreaId = areaId;
                Explored = explored;
            }
        }
    }
}