using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Logging;
using AO.Core.Utils;
using AO.Players;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Questing
{
    public static class QuestManager
    {
        private static Dictionary<QuestId, Quest> quests;
        /// <summary> Contains which quests can each npc turn in.</summary>
        private static readonly Dictionary<NpcId, IEnumerable<QuestId>> npcTurnIns = new();
        
        private static readonly LoggerAdapter log = new(typeof(QuestManager));

        public static Quest GetQuest(QuestId id)
        {
            AoDebug.Assert(quests.ContainsKey(id), $"Quest {id} was not found.");
            return quests[id];
        }
        
        public static async Task LoadQuests()
        {
            quests =  await Core.Database.DatabaseOperations.FetchAllQuests();
            log.Info("Successfully loaded quests.");
        }

        public static void ReloadQuest(Quest quest)
        {
            quests.AddOrUpdate(quest.Id, quest);
        }

        public static void NpcAddQuestTurnIn(NpcId npcId, IEnumerable<QuestId> questIds)
        {
            npcTurnIns.AddOrUpdate(npcId, questIds);
        }

        public static void AcceptQuestFromNpc(QuestId questId, Player player)
        {
            if (!player.Flags.InteractingWithNpc)
                return;

            if (!quests.TryGetValue(questId, out var quest))
                return;

            var npc = player.Flags.InteractingWithNpc;
            if (!npc.Info.GiveQuests.Contains(quest))
                return;
            
            AssignQuestToPlayer(quest, player, GetQuestTurnInNpcs(questId));
        }

        public static bool AssignQuestToPlayer(QuestId questId, Player player)
        {
            if (!quests.TryGetValue(questId, out var quest))
                return false;

            AssignQuestToPlayer(quest, player, GetQuestTurnInNpcs(questId));
            return true;
        }
        
        private static void AssignQuestToPlayer(Quest quest, Player player, List<NpcId> turnInNpcs)
        {
            if (player.QuestManager.IsOnQuest(quest.Id))
                return;

            if (!CanPlayerAcceptQuest(quest, player))
                return;
            
            if (!quest.DoesPlayerMeetAllRequirements(player))
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.QuestRequirementsNotMet);
                return;
            }

            var steps = quest.GetNewSteps(player);
            player.QuestManager.AddQuest(quest.Id, steps);
            PacketSender.QuestAssigned(player.Id, quest.Id, turnInNpcs.Count > 0, turnInNpcs);
        }

        public static List<NpcId> GetQuestTurnInNpcs(QuestId questId)
        {
            var npcsIds = new List<NpcId>(5);
            foreach (var (npcId, turnInQuests) in npcTurnIns)
                if (turnInQuests.Contains(questId))
                    npcsIds.Add(npcId);
            return npcsIds;
        }   

        public static void TryCompleteQuest(QuestId questId, Player player)
        {
            if (!quests.TryGetValue(questId, out var quest))
                return;

            if (!player.QuestManager.IsOnQuest(questId))
                return;

            var npcsList = GetQuestTurnInNpcs(questId);
            if (npcsList.Count > 0)
            {
                var interactingWith = player.Flags.InteractingWithNpc;
                if (!interactingWith || !npcsList.Contains(interactingWith.Info.Id))
                    return;
            }

            var steps = player.QuestManager.GetQuestSteps(questId);

            if (!quest.TryCompleteQuest(player, steps))
                return;

            player.QuestManager.RemoveQuest(questId);
            player.QuestManager.AddQuestCompleted(questId);
            PacketSender.QuestCompleted(player.Id, questId);
        }

        private static bool CanPlayerAcceptQuest(Quest quest, Player player)
        {
            if (player.QuestManager.QuestsCount >= Constants.PLAYER_MAX_QUESTS)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.QuestLogFull);
                return false;
            }

            return quest.Repeatable switch
            {
                Repeatable.No => !player.QuestManager.QuestCompleted(quest.Id),
                Repeatable.Daily => true,
                Repeatable.Weekly => true,
                _ => false
            };
        }
    }
}