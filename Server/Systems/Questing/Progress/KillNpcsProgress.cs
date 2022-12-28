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
    public sealed class KillNpcsProgress : IQuestProgress
    {
        [JsonIgnore]
        public bool IsCompleted { get; private set; }
        public Action<Player> TryAdvanceToNextStep { private get; set; }

        private Player subscribedToPlayer;
        private readonly QuestId questId;
        private readonly KillNpcsGoal goal;
        
        [JsonProperty("Id")] 
        public byte Id => goal.Id;
        [JsonProperty("Progress")]
        private readonly Dictionary<NpcId, ushort> kills = new();

        public KillNpcsProgress(QuestId questId, KillNpcsGoal goal)
        {
            this.questId = questId;
            this.goal = goal;
            kills.InitializeKeys(goal.NpcsKillsRequired.Keys);
        }

        public void SubscribeToEvent(Player player)
        {
            subscribedToPlayer = player;
            player.Events.KilledNpc += OnPlayerKilledNpc;
        }

        public void TurnInProgress() { }
        public void LoadExistingProgress(JObject progress)
        {
            foreach (var (key, value) in progress)
                kills[NpcId.Parse(key)] = Convert.ToUInt16(value);
        }

        public void SendAllProgressUpdate()
        {
            foreach (var npcId in kills.Keys)
                UpdateProgress(npcId);
        }

        private void UpdateProgress(NpcId npcId)
        {
            // Pass the data to write in the packet in a struct to avoid closure allocation
            var callbackData = new WritePacketCallbackData(goal.Id, npcId, kills[npcId]);
            PacketSender.QuestProgressUpdate(subscribedToPlayer.Id, questId, callbackData,
                (packet, data) =>
                {
                    packet.Write(data.GoalId);
                    packet.Write(data.NpcId.AsPrimitiveType());
                    packet.Write(data.KillsCount);
                });
        }

        private void OnPlayerKilledNpc(NpcId npcId)
        {
            if (!goal.NpcsKillsRequired.ContainsKey(npcId))
                return;

            kills[npcId]++;
                
            UpdateProgress(npcId);
    
            if (IsGoalCompleted())
            {
                IsCompleted = true;
                Dispose();
                TryAdvanceToNextStep(subscribedToPlayer);
            }
        }
        
        private bool IsGoalCompleted()
        {
            foreach (var (npcId, amountKilled) in kills)
                if (goal.NpcsKillsRequired[npcId] != amountKilled)
                    return false;

            return true;
        }

        public void Dispose()
        {
            subscribedToPlayer.Events.KilledNpc -= OnPlayerKilledNpc;
            kills.Clear();
        }
        
        private readonly struct WritePacketCallbackData
        {
            public readonly byte GoalId;
            public readonly NpcId NpcId;
            public readonly ushort KillsCount;

            public WritePacketCallbackData(byte goalId, NpcId npcId, ushort killsCount)
            {
                GoalId = goalId;
                NpcId = npcId;
                KillsCount = killsCount;
            }
        }
    }
}