using System;
using System.Collections.Generic;
using AO.Core.Ids;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Mailing
{
    public class ReceivedMailPanelUI : MonoBehaviour, IMailPanel
    {
        [SerializeField] private Button openMailButton;
        [SerializeField] private Transform receivedMailsContainer;
        [SerializeField] private MailListEntryUI mailListEntryPrefab;

        private MailListEntryUI currentlySelectedEntry;

        private readonly Dictionary<uint, Mail> cachedEntries = new();
        
        private void Start()
        {
            openMailButton.onClick.AddListener(OnOpenMailButtonClicked);
        }

        private void OnEnable()
        {
            foreach (Transform child in receivedMailsContainer)
            {
                var entry = child.GetComponent<MailListEntryUI>();
                if (entry.Mail.ExpirationDate <= DateTime.Now)
                    DeleteMail(entry);       
            }
        }

        public void Show()
        {
            PacketSender.FetchMails();
            gameObject.SetActive(true);
        }
        
        public void Close()
        {
            gameObject.SetActive(false);
        }
        
        public void AddEntries(List<Mail> entries)
        {
            foreach (var mail in entries)
            {
                MailListEntryUI entry = Instantiate(mailListEntryPrefab, receivedMailsContainer);
                entry.HighlightImage.enabled = false;
                entry.Mail = mail;
                entry.EntryText.text = Constants.ReceivedMailEntry(mail.SenderName, mail.Subject, mail.ExpiresIn);
                entry.Button.onClick.AddListener(() => OnEntrySelected(entry));
                cachedEntries.Add(mail.Id, mail);
            }
        }

        public void RemoveItemFromMail(uint mailId, ItemId itemId)
        {
            if (cachedEntries.TryGetValue(mailId, out Mail mail))
                mail.Items.Remove(itemId);
        }

        public void DeleteCurrentlySelectedEntry()
        {
            DeleteMail(currentlySelectedEntry);
            currentlySelectedEntry = null;
        }
        
        private void DeleteMail(MailListEntryUI entry)
        {
            cachedEntries.Remove(entry.Mail.Id);
            entry.transform.SetParent(null);
            Destroy(entry);
            if (currentlySelectedEntry == entry)
                currentlySelectedEntry = null;
        }

        private void OnEntrySelected(MailListEntryUI selectedEntry)
        {
            if (currentlySelectedEntry != null)
                currentlySelectedEntry.HighlightImage.enabled = false;

            selectedEntry.HighlightImage.enabled = true;
            currentlySelectedEntry = selectedEntry;
        }

        private void OnOpenMailButtonClicked()
        {
            if (currentlySelectedEntry is null)
                return;

            if (currentlySelectedEntry.Mail.ExpirationDate <= DateTime.Now)
            {
                DeleteCurrentlySelectedEntry();
                return;
            }
            
            UIManager.GameUI.MailWindow.ShowOpenedMailPanel(currentlySelectedEntry.Mail);
        }
    }
}