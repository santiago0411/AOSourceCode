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
    public sealed class GatherResourcesProgress : IQuestProgress
    {
        [JsonIgnore]
        public bool IsCompleted { get; private set; }
        public Action<Player> TryAdvanceToNextStep { private get; set; }

        private Player subscribedToPlayer;
        private readonly QuestId questId;
        private readonly GatherResourcesGoal goal;
        
        [JsonProperty("Id")] 
        public byte Id => goal.Id;
        [JsonProperty("Progress")]
        private readonly Dictionary<ItemId, ushort> resourcesGathered = new();

        public GatherResourcesProgress(QuestId questId, GatherResourcesGoal goal)
        {
            this.questId = questId;
            this.goal = goal;
            resourcesGathered.InitializeKeys(goal.ResourcesRequired.Keys);
        }

        public void SubscribeToEvent(Player player)
        {
            subscribedToPlayer = player;
            player.Events.ResourceGathered += OnPlayerResourceGathered;
        }
        
        public void TurnInProgress() { }
        public void LoadExistingProgress(JObject progress)
        {
            foreach (var (key, value) in progress)
                resourcesGathered[ItemId.Parse(key)] = Convert.ToUInt16(value);
        }

        public void SendAllProgressUpdate()
        {
            foreach (var itemId in resourcesGathered.Keys)
                UpdateProgress(itemId);
        }

        private void UpdateProgress(ItemId itemId)
        {
            // Pass the data to write in the packet in a struct to avoid closure allocation
            var callbackData = new WritePacketCallbackData(goal.Id, itemId, resourcesGathered[itemId]);
            PacketSender.QuestProgressUpdate(subscribedToPlayer.Id, questId, callbackData,
                (packet, data) =>
                {
                    packet.Write(data.GoalId);
                    packet.Write(data.ItemId.AsPrimitiveType());
                    packet.Write(data.Quantity);
                });
        }

        private void OnPlayerResourceGathered(ItemId itemId, ushort quantity)
        {
            if (!goal.ResourcesRequired.ContainsKey(itemId) || MaxQuantityAcquired(itemId))
                return;
            
            resourcesGathered[itemId] += quantity;
            
            UpdateProgress(itemId);

            if (IsGoalCompleted())
            {
                IsCompleted = true;
                Dispose();
                TryAdvanceToNextStep(subscribedToPlayer);
            }
        }
        
        private bool IsGoalCompleted()
        {
            foreach (var (itemId, currentAmount) in resourcesGathered)
                if (goal.ResourcesRequired[itemId] > currentAmount)
                    return false;

            return true;
        }

        private bool MaxQuantityAcquired(ItemId itemId)
        {
            return resourcesGathered[itemId] >= goal.ResourcesRequired[itemId];
        }
        
        public void Dispose()
        {
            subscribedToPlayer.Events.ResourceGathered -= OnPlayerResourceGathered;
            resourcesGathered.Clear();
        }
        
        private readonly struct WritePacketCallbackData
        {
            public readonly byte GoalId;
            public readonly ItemId ItemId;
            public readonly ushort Quantity;

            public WritePacketCallbackData(byte goalId, ItemId itemId, ushort quantity)
            {
                GoalId = goalId;
                ItemId = itemId;
                Quantity = quantity;
            }
        }
    }
}