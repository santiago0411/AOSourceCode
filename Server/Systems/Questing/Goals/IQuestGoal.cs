using AO.Core.Ids;
using AO.Systems.Questing.Progress;

namespace AO.Systems.Questing.Goals
{
    public interface IQuestGoal
    {
        byte Id { get; }
        byte StepOrder { get; }
        IQuestProgress GetNewProgress(QuestId questId);
    }
}