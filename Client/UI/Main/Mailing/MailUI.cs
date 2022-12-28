using AO.Core.Ids;
using AOClient.Core;
using AOClient.Core.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Mailing
{
    public class MailUI : MonoBehaviour
    {
        public bool IsOpen => gameObject.activeSelf;
        public ReceivedMailPanelUI ReceivedMailPanel => receivedMailPanel;
        public SendMailPanelUI SendMailWindow => sendMailPanel;
        public Pool<MailItemUI> MailItemsPool { get; private set; }
        
        [SerializeField] private ReceivedMailPanelUI receivedMailPanel;
        [SerializeField] private OpenedMailPanelUI openedMailPanel;
        [SerializeField] private SendMailPanelUI sendMailPanel;
        [SerializeField] private Button closeButton, inboxButton, sendMailButton;
        [SerializeField] private MailItemUI mailItemPrefab;

        private IMailPanel activePanel;
        
        private void Start()
        {
            MailItemsPool = new Pool<MailItemUI>(mailItemPrefab);
            
            activePanel = receivedMailPanel;
            closeButton.onClick.AddListener(Close);
            inboxButton.onClick.AddListener(ShowReceivedMailPanel);
            sendMailButton.onClick.AddListener(ShowSendMailPanel);

            Close();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            activePanel = receivedMailPanel;
            receivedMailPanel.Show();
        }

        public void ShowReceivedMailPanel()
        {
            activePanel.Close();
            activePanel = receivedMailPanel;
            receivedMailPanel.Show();
        }

        public void ShowOpenedMailPanel(Mail mail)
        {
            activePanel.Close();
            activePanel = openedMailPanel;
            openedMailPanel.Show(mail);
        }

        private void ShowSendMailPanel()
        {
            activePanel.Close();
            activePanel = sendMailPanel;
            sendMailPanel.Show();
        }

        public void RemoveItemFromMail(uint mailId, ItemId itemId)
        {
            // This removes the item from the cache
            receivedMailPanel.RemoveItemFromMail(mailId, itemId);
            
            // This removes the item from the UI if the mail is currently opened
            if (openedMailPanel.OpenedMailId == mailId)
                openedMailPanel.RemoveItem(itemId);
        }
        
        private void Close()
        {
            activePanel.Close();
            gameObject.SetActive(false);
        }
    }
}