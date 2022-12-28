using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Spells
{
    public class SpellSlotUI : MonoBehaviour
    {
        public byte SlotId { get; private set; }
        public Button Button => button;
        public Image HighlightImage => highlightImage;
        public TextMeshProUGUI SpellNameText => spellNameText;

        [SerializeField] private Button button;
        [SerializeField] private Image highlightImage;
        [SerializeField] private TextMeshProUGUI spellNameText;

        private void Start()
        {
            highlightImage.enabled = false;
            SlotId = byte.Parse(name.Split('(')[1].Split(')')[0]);
        }
    }
}
