using AOClient.Questing.Progress;

namespace AOClient.Questing.Goals
{
    public interface IQuestGoal
    {
        byte StepOrder { get; }
        void LoadGoal();
        IQuestProgress GetNewProgress();
    }
}