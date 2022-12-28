using AO.Core.Ids;
using AOClient.Core.Utils;
using AOClient.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Mailing
{
    public class OpenedMailPanelUI : MonoBehaviour, IMailPanel
    {
        public uint OpenedMailId { get; private set; }
        
        [SerializeField] private TextMeshProUGUI fromUserText, subjectText, bodyText;
        [SerializeField] private Transform itemsPanel;
        [SerializeField] private Button getAllItemsButton, deleteMailButton;

        private void Start()
        {
            getAllItemsButton.onClick.AddListener(OnGetAllItemsButtonClicked);
            deleteMailButton.onClick.AddListener(OnDeleteMailButtonClicked);
        }

        public void Show(Mail mail)
        {
            gameObject.SetActive(true);
            OpenedMailId = mail.Id;
            fromUserText.text = mail.SenderName;
            subjectText.text = mail.Subject;
            bodyText.text = mail.Body;

            var pool = UIManager.GameUI.MailWindow.MailItemsPool;
            foreach (var (itemId, quantity) in mail.Items)
            {
                MailItemUI mailItemUI = pool.GetObject();
                mailItemUI.transform.SetParent(itemsPanel);
                mailItemUI.SetItem(itemId, quantity);
                mailItemUI.OnLeftClick.AddListener(() => OnMailItemLeftClicked(mailItemUI));
            }
        }

        public void Close()
        {
            fromUserText.text = string.Empty;
            subjectText.text = string.Empty;
            bodyText.text = string.Empty;
            UIManager.GameUI.MailWindow.MailItemsPool.ResetObjects();
            gameObject.SetActive(false);
        }

        public void RemoveItem(ItemId itemId)
        {
            foreach (Transform child in itemsPanel)
            {
                var mailItemUI = child.GetComponent<MailItemUI>();
                if (mailItemUI.ItemId == itemId)
                {
                    mailItemUI.ResetPoolObject();
                    return;
                }
            }
        }

        private void OnMailItemLeftClicked(MailItemUI mailItem)
        {
            PacketSender.CollectMailItem(OpenedMailId, mailItem.ItemId);
        }
        
        private void OnGetAllItemsButtonClicked()
        {
            foreach (Transform child in itemsPanel)
                OnMailItemLeftClicked(child.GetComponent<MailItemUI>());
        }

        private void OnDeleteMailButtonClicked()
        {
            PacketSender.DeleteMail(OpenedMailId);
            UIManager.GameUI.MailWindow.ReceivedMailPanel.DeleteCurrentlySelectedEntry();
            UIManager.GameUI.MailWindow.ShowReceivedMailPanel();
        }
    }
}