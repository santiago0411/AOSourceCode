using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using AO.Players;
using AO.Systems.Questing.Goals;
using AO.Systems.Questing.Progress;

namespace AO.Systems.Questing
{
    public sealed class QuestSteps
    {
        public bool AllStepsCompleted => CurrentProgresses.All(p => p.IsCompleted);
        
        public byte CurrentStep { get; private set; }
        public IQuestProgress[] CurrentProgresses { get; private set; }
        
        private readonly Queue<IQuestProgress[]> steps = new();

        public QuestSteps(QuestId questId, IQuestGoal[][] stepsGoals, Player forPlayer, byte startingOnStep = 1)
        {
            for (int i = startingOnStep - 1; i < stepsGoals.Length; i++)
                steps.Enqueue(stepsGoals[i].Select(goal => goal.GetNewProgress(questId)).ToArray());
            
            CurrentStep = startingOnStep;
            CurrentProgresses = steps.Dequeue();

            foreach (var prog in CurrentProgresses)
            {
                prog.TryAdvanceToNextStep = TryAdvanceToNextStep;
                prog.SubscribeToEvent(forPlayer);
            }
        }
        public void TurnInCurrentProgresses()
        {
            foreach (var prog in CurrentProgresses)
                prog.TurnInProgress();
        }

        public void DisposeCurrentProgresses()
        {
            foreach (var prog in CurrentProgresses)
                prog.Dispose();
        }

        private void TryAdvanceToNextStep(Player player)
        {
            if (CurrentProgresses.Any(p => !p.IsCompleted))
                return;

            if (steps.Count == 0)
                return;

            CurrentStep++;
            CurrentProgresses = steps.Dequeue();
            
            foreach (var prog in CurrentProgresses)
            {
                prog.TryAdvanceToNextStep = TryAdvanceToNextStep;
                prog.SubscribeToEvent(player);
            }
        }
    }
}