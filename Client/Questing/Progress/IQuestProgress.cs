using System;
using AOClient.Network;

namespace AOClient.Questing.Progress
{
    public interface IQuestProgress : IDisposable
    {
        byte GoalId { get; }
        byte StepOrder { get; }
        void UpdateProgress(Packet packet);
        void LoadGoalAndProgress();
        void FullyComplete();
    }
}