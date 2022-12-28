using System.Collections.Generic;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.UI.Main.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Mailing
{
    public class SendMailPanelUI : MonoBehaviour, IMailPanel
    {
        public bool IsOpen => gameObject.activeSelf;
        
        [SerializeField] private TMP_InputField toPlayerInput, subjectInput, bodyInput, goldInput;
        [SerializeField] private Transform sendItemsPanel;
        [SerializeField] private Button sendButton;

        private readonly List<byte> slotsToMail = new();
        
        private void Start()
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
            Reset();
        }

        public void Show()
        {
            slotsToMail.Clear();
            gameObject.SetActive(true);
        }

        public void Reset()
        {
            toPlayerInput.text = string.Empty;
            subjectInput.text = string.Empty;
            bodyInput.text = string.Empty;
            goldInput.text = string.Empty;
            slotsToMail.Clear();
            UIManager.GameUI.MailWindow.MailItemsPool.ResetObjects();
        }
        
        public void Close()
        {
            Reset();
            gameObject.SetActive(false);
        }

        public void OnSlotRightClicked(InventorySlotUI slot)
        {
            if (slotsToMail.Count >= Constants.MAX_MAIL_ITEMS || slotsToMail.Contains(slot.SlotId))
                return;
            
            slotsToMail.Add(slot.SlotId);
            MailItemUI mailItem = UIManager.GameUI.MailWindow.MailItemsPool.GetObject();
            mailItem.transform.SetParent(sendItemsPanel);
            mailItem.SetInventorySlot(slot);
            mailItem.OnRightClick.AddListener(() => OnMailItemRightClicked(mailItem));
        }
        
        private void OnSendButtonClicked()
        {
            var console = UIManager.GameUI.Console; 
            if (string.IsNullOrEmpty(toPlayerInput.text))
            {
                console.WriteLine(Constants.RECIPIENT_FIELD_EMPTY, ConsoleMessage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(subjectInput.text))
            {
                console.WriteLine(Constants.SUBJECT_FIELD_EMPTY, ConsoleMessage.Warning);
                return;
            }

            uint gold = 0;
            
            if (!string.IsNullOrEmpty(goldInput.text) && !uint.TryParse(goldInput.text, out gold))
                return;
            
            if (gold > GameManager.Instance.LocalPlayer.Gold)
            {
                console.WriteLine(Constants.NOT_ENOUGH_GOLD, ConsoleMessage.Warning);
                return;
            }

            PacketSender.SendMail(toPlayerInput.text, subjectInput.text, bodyInput.text, gold, slotsToMail.ToArray());
        }

        private void OnMailItemRightClicked(MailItemUI mailItem)
        {
            slotsToMail.Remove(mailItem.SlotId);
            mailItem.ResetPoolObject();
        }
    }
}