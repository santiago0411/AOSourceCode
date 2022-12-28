using AO.Players;

namespace AO.Systems.Questing.Requirements
{
    public interface IQuestRequirement
    {
        public bool DoesPlayerMeetRequirement(Player player);
    }
}
