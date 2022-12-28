using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using UnityEngine;
using AOClient.Core;

namespace AOClient.UI.Main.Questing
{
    public class QuestsListPanelUI : MonoBehaviour
    {
        [SerializeField] private Transform questPanelContainer;
        [SerializeField] private QuestListItem listItemPrefab;

        private Pool<QuestListItem> listItemsPool;
        private readonly HashSet<QuestId> questsInPanel = new();

        private void Awake()
        {
            listItemsPool = new Pool<QuestListItem>(listItemPrefab, Constants.MAX_QUESTS);
        }

        public void AddQuestToPanel(QuestId questId)
        {
            listItemsPool ??= new Pool<QuestListItem>(listItemPrefab, Constants.MAX_QUESTS);
            questsInPanel.Add(questId);
            QuestListItem item = listItemsPool.GetObject();
            item.transform.SetParent(questPanelContainer, false);
            item.SetQuest(questId);
        }
        
        public void LoadFirst()
        {
            if (questsInPanel.Count > 0)
                UIManager.GameUI.QuestWindow.LoadQuest(questsInPanel.First());
        }

        public void RemoveQuest(QuestId questId)
        {
            questsInPanel.Remove(questId);
            listItemsPool.ResetObjects();
            foreach (var quest in questsInPanel)
                AddQuestToPanel(quest);
        }
        
        public void Reset()
        {
            questsInPanel.Clear();
            listItemsPool.ResetObjects();
        }
    }
}