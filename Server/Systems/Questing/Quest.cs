using System.Linq;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Logging;
using AO.Core.Utils;
using AO.Players;
using AO.Systems.Questing.Goals;
using AO.Systems.Questing.Requirements;
using AO.Systems.Questing.Rewards;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Questing
{
    public sealed class Quest
    {
        private static readonly LoggerAdapter log = new(typeof(Quest));

        public readonly QuestId Id;
        public readonly Repeatable Repeatable;
        
        private readonly IQuestRequirement[] requirements;
        private readonly IQuestGoal[][] goals;
        private readonly IQuestReward[] rewards;
        private readonly bool hasChoosableRewards;

        public Quest(QuestInfo questInfo)
        {
            Id = questInfo.Id;
            Repeatable = questInfo.Repeatable;
            (requirements, goals, rewards) =
                QuestJsonUtils.DeserializeQuestJsons(Id, questInfo.Requirements, questInfo.Goals, questInfo.Rewards);
            hasChoosableRewards = rewards.Any(r => r is ChoosableItemReward);

            for (int i = 0; i < goals.Length - 1; i++)
                if (goals[i].Any(g => g is ItemsGoal || g is GoldGoal))
                {
                    AoDebug.Assert(true, $"Quest {Id} has turn in goals on steps that aren't the last one!!");
                    log.Error($"Quest {Id} has turn in goals on steps that aren't the last one!!");
                }
        }

        public QuestSteps GetNewSteps(Player forPlayer) => new(Id, goals, forPlayer);

        public QuestSteps GetNewSteps(Player forPlayer, byte startingOnStep) => new(Id, goals, forPlayer, startingOnStep);

        public bool DoesPlayerMeetAllRequirements(Player player)
        {
            // Do not use linq to avoid closure
            foreach (var req in requirements)
                if (!req.DoesPlayerMeetRequirement(player))
                    return false;

            return true;
        }
        
        public bool TryCompleteQuest(Player player, QuestSteps steps)
        {
            if (!steps.AllStepsCompleted)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NotAllStepsAreCompleted);
                return false;
            }

            if (hasChoosableRewards && (player.Flags.SelectedQuestId != Id || player.Flags.SelectedItemRewardId == ItemId.Empty))
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.MustChooseQuestReward);
                return false;
            }

            steps.TurnInCurrentProgresses();
            
            foreach (var reward in rewards)
                reward.AssignReward(player);

            return true;
        }
    }
}
