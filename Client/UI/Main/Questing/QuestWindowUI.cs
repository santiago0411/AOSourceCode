using AO.Core.Ids;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AOClient.Core;
using AOClient.Network;
using AOClient.Questing;

namespace AOClient.UI.Main.Questing
{
    public class QuestWindowUI : MonoBehaviour
    {
        public QuestRewardsUI QuestRewardsUI { get; private set; }
        public QuestGoalsUI QuestGoalsUI { get; private set; }

        [SerializeField] private TextMeshProUGUI questNameText, questDescriptionText, rewardsText;
        [SerializeField] private Button acceptButton, completeButton, otherWindowButton, closeButton;
        [SerializeField] private TextMeshProUGUI otherWindowButtonText;
        [SerializeField] private GameObject otherWindowButtonPanel;
        
        private QuestsListPanelUI questsListPanel;
        private QuestId selectedQuest;
        private bool openedByNpc;
        
        private void Awake()
        {
            QuestRewardsUI = GetComponentInChildren<QuestRewardsUI>();
            QuestGoalsUI = GetComponentInChildren<QuestGoalsUI>();
            questsListPanel = GetComponentInChildren<QuestsListPanelUI>();
            gameObject.SetActive(false);
            
            acceptButton.onClick.AddListener(() => PacketSender.AcceptQuest(selectedQuest));
            completeButton.onClick.AddListener(() => PacketSender.CompleteQuest(selectedQuest));
            
            otherWindowButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                UIManager.GameUI.NpcTradeWindow.Open(true);
            });
            
            closeButton.onClick.AddListener(() =>
            {
                Close();
                if (openedByNpc)
                    UIManager.GameUI.NpcTradeWindow.Close();
            });
        }

        public void RemoveQuest(QuestId questId)
        {
            questsListPanel.RemoveQuest(questId);
            QuestRewardsUI.ResetRewards();
            QuestGoalsUI.ResetGoals();
            questsListPanel.LoadFirst();
        }
        
        public void LoadNpcQuests(QuestId[] questIds)
        {
            foreach (var quest in questIds)
                questsListPanel.AddQuestToPanel(quest);
            
            questsListPanel.LoadFirst();
            ShowWindow(true);
        }

        public void LoadQuest(QuestId questId)
        {
            Quest quest = GameManager.Instance.GetQuest(questId);
            selectedQuest = quest.Id;
            
            QuestRewardsUI.ResetRewards();
            QuestGoalsUI.ResetGoals();
            
            questNameText.text = quest.Name;
            questDescriptionText.text = quest.Description;
            rewardsText.enabled = true;
            acceptButton.gameObject.SetActive(true);
    
            QuestRewardsUI.LoadRewards(quest.Rewards);

            int lastStepOrder = 1;
            var localPlayer = GameManager.Instance.LocalPlayer;
            
            // If the player is currently on this quest load progresses and goals from inside progresses
            if (localPlayer.QuestManager.IsPlayerOnQuest(quest.Id))
            {
                ShowCompleteButton();
                var progresses = localPlayer.QuestManager.GetQuestProgresses(quest.Id);
                
                foreach (var progress in progresses)
                {
                    if (progress.StepOrder > lastStepOrder)
                        QuestGoalsUI.AddStepDivider();
                    
                    progress.LoadGoalAndProgress();
                    lastStepOrder = progress.StepOrder;
                }
                
                return;
            }

            ShowAcceptButton();
            // Otherwise just load the goals with no progress attached to them
            foreach (var goal in quest.Goals)
            {
                if (goal.StepOrder > lastStepOrder)
                    QuestGoalsUI.AddStepDivider();
                
                goal.LoadGoal();
                lastStepOrder = goal.StepOrder;
            }
        }

        public void ShowCompleteButton()
        {
            acceptButton.gameObject.SetActive(false);
            completeButton.gameObject.SetActive(true);
        }
        
        public void ShowAcceptButton()
        {
            completeButton.gameObject.SetActive(false);
            acceptButton.gameObject.SetActive(true);
        }

        public void ShowWindow(bool isNpcWindow)
        {
            if (gameObject.activeSelf)
                return;

            openedByNpc = isNpcWindow;
            gameObject.SetActive(true);
            OnWindowShow(isNpcWindow);
        }

        private void OnWindowShow(bool isNpcWindow)
        {
            if (isNpcWindow)
            {
                otherWindowButtonPanel.SetActive(true);
                // TODO list quests of the npc that the player is interacting with
            }
            else
            {
                otherWindowButtonPanel.SetActive(false);
                var localPlayer = GameManager.Instance.LocalPlayer;
                
                foreach(QuestId questId in localPlayer.QuestManager.GetAllQuests())
                    questsListPanel.AddQuestToPanel(questId);
                
                questsListPanel.LoadFirst();
            }
        }

        public void Close()
        {
            questNameText.text = string.Empty;
            questDescriptionText.text = string.Empty;
            rewardsText.enabled = false;
            acceptButton.gameObject.SetActive(false);
            completeButton.gameObject.SetActive(false);
            questsListPanel.Reset();
            QuestRewardsUI.ResetRewards();
            QuestGoalsUI.ResetGoals();
            
            gameObject.SetActive(false);
        }
    }
}