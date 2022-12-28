using AOClient.Core.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Mailing
{
    public class MailListEntryUI : MonoBehaviour
    {
        public Mail Mail { get; set; }
        public TextMeshProUGUI EntryText => entryText;
        public Image HighlightImage => highlightImage;
        public Button Button => button;
        
        [SerializeField] private TextMeshProUGUI entryText;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Button button;
    }
}