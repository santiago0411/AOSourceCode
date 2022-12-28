using UnityEngine;

namespace AOClient.Questing.Rewards
{
    public interface IQuestReward
    {
        void AddRewardToPanel(Transform panelTransform);
    }
}