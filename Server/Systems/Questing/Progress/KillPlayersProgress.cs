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
    public sealed class KillPlayersProgress : IQuestProgress
    {
        [JsonIgnore]
        public bool IsCompleted { get; private set; }
        public Action<Player> TryAdvanceToNextStep { private get; set; }

        private Player subscribedToPlayer;
        private readonly QuestId questId;
        private readonly KillPlayersGoal goal;
        
        [JsonProperty("Id")] 
        public byte Id => goal.Id;
        [JsonProperty("Progress")]
        private readonly Dictionary<Faction, ushort> kills = new();

        public KillPlayersProgress(QuestId questId, KillPlayersGoal goal)
        {
            this.questId = questId;
            this.goal = goal;
            kills.InitializeKeys(goal.PlayerKillsRequired.Keys);
        }
        
        public void SubscribeToEvent(Player player)
        {
            subscribedToPlayer = player;
            player.Events.KilledPlayer += OnPlayerKilledPlayer;
        }

        public void TurnInProgress() { }
        public void LoadExistingProgress(JObject progress)
        {
            foreach (var (key, value) in progress)
                kills[(Faction)Convert.ToByte(key)] = Convert.ToUInt16(value);
        }

        public void SendAllProgressUpdate()
        {
            foreach (var faction in kills.Keys)
                UpdateProgress(faction);
        }

        private void UpdateProgress(Faction faction)
        {
            // Pass the data to write in the packet in a struct to avoid closure allocation
            var callbackData = new WritePacketCallbackData(goal.Id, (byte)faction, kills[faction]);
            PacketSender.QuestProgressUpdate(subscribedToPlayer.Id, questId, callbackData, 
                (packet, data) =>
                {
                    packet.Write(data.GoalId);
                    packet.Write(data.Faction);
                    packet.Write(data.KillsCount);
                });
        }
        
        private void OnPlayerKilledPlayer(Player playerKilled)
        {
            Faction faction = playerKilled.Faction;
            
            if (!goal.PlayerKillsRequired.ContainsKey(faction))
                return;
            
            kills[faction]++;
            
            UpdateProgress(faction);

            if (IsGoalCompleted())
            {
                IsCompleted = true;
                Dispose();
                TryAdvanceToNextStep(subscribedToPlayer);
            }
        }

        private bool IsGoalCompleted()
        {
            foreach (var entry in kills)
                if (goal.PlayerKillsRequired[entry.Key] != entry.Value)
                    return false;

            return true;
        }
        
        public void Dispose()
        {
            subscribedToPlayer.Events.KilledPlayer -= OnPlayerKilledPlayer;
            kills.Clear();
        }
        
        private readonly struct WritePacketCallbackData
        {
            public readonly byte GoalId;
            public readonly byte Faction;
            public readonly ushort KillsCount;

            public WritePacketCallbackData(byte goalId, byte faction, ushort killsCount)
            {
                GoalId = goalId;
                Faction = faction;
                KillsCount = killsCount;
            }
        }
    }
}