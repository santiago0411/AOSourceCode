using AO.Core.Ids;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Crafting
{
    public class RequiredItemUI : MonoBehaviour
    {
        public ItemId RequiredItemId { get; set; }
        public Image ItemImage => itemImage;
        public TextMeshProUGUI ItemNameText => itemNameText;
        public TextMeshProUGUI AmountInInventoryText => amountInInventoryText;
        public TextMeshProUGUI RequiredAmountText => requiredAmountText;

        [SerializeField]
        private Image itemImage;
        [SerializeField]
        private TextMeshProUGUI itemNameText, amountInInventoryText, requiredAmountText;
    }
}
