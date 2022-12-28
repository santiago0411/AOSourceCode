using AOClient.Core;
using UnityEngine;

namespace AOClient.UI.Main.Questing
{
    public class QuestGoalsUI : MonoBehaviour
    {
        [SerializeField] private Transform goalsContainer;
        
        [Header("Goals Prefabs")] 
        [SerializeField] private GoalListItem goalListItemPrefab;
        [SerializeField] private GoalStepDivider goalListStepDividerPrefab;

        private Pool<GoalListItem> goalItemsPool;
        private Pool<GoalStepDivider> goalStepDividersPool;
        
        private void Awake()
        {
            goalItemsPool = new Pool<GoalListItem>(goalListItemPrefab);
            goalStepDividersPool = new Pool<GoalStepDivider>(goalListStepDividerPrefab);
        }

        public GoalListItem GetGoalListItem()
        {
            var item = goalItemsPool.GetObject();
            item.gameObject.SetActive(true);
            item.IsBeingUsed = true;
            item.gameObject.transform.SetParent(goalsContainer, false);
            return item;
        }

        public void AddStepDivider()
        {
            var divider = goalStepDividersPool.GetObject();
            divider.IsBeingUsed = true;
            var go = divider.gameObject;
            go.SetActive(true);
            go.transform.SetParent(goalsContainer, false);
        }

        public void ResetGoals()
        {
            goalItemsPool.ResetObjects();
            goalStepDividersPool.ResetObjects();
        }
    }
}