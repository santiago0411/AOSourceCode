using AO.Core.Ids;
using AOClient.Core;
using AOClient.Network;
using AOClient.Questing;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AOClient.UI.Main.Questing
{
    public class QuestListItem : MonoBehaviour, IPoolObject
    {
        public int InstanceId => GetInstanceID();
        public bool IsBeingUsed { get; private set; }

        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Button button;
        
        public void SetQuest(QuestId questId)
        {
            Quest quest = GameManager.Instance.GetQuest(questId);
            questNameText.text = quest.Name;
            button.onClick.AddListener(() =>
            {
                UIManager.GameUI.QuestWindow.LoadQuest(quest.Id);
                PacketSender.SelectQuest(quest.Id);
            });
            gameObject.SetActive(true);
            IsBeingUsed = true;
        }

        public void ResetPoolObject()
        {
            button.onClick.RemoveAllListeners();
            questNameText.text = string.Empty;
            highlightImage.enabled = false;
            gameObject.SetActive(false);
            IsBeingUsed = false;
        }
    }
}

