using System;
using System.Linq;
using AOClient.Core;
using AOClient.Player.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Crafting
{
    public class CraftableItemUI : MonoBehaviour
    {
        public CraftableItem CraftableItem { get; private set; }
        public ushort MaxCraftable { get; private set; }
        public Image HighlightImage => highlightImage;
        public Button Button => button;

        [SerializeField] private TextMeshProUGUI craftableItemNameText;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Button button;

        public void LoadCraftableItem(CraftableItem craftableItem)
        {
            CraftableItem = craftableItem;
            HighlightImage.enabled = false;
            UpdateMaxCraftable();
        }

        public void UpdateMaxCraftable()
        {
            ushort craftableQuantity = ushort.MaxValue;

            foreach (var (requiredItem, requiredAmount) in CraftableItem.RequiredItemsAndAmounts)
            {
                int totalAmountOfItem = GameManager.Instance.LocalPlayer.GetSlotsWithItem(requiredItem.Id).Sum(x => x.Quantity);
                int maxAmount = totalAmountOfItem / requiredAmount;

                if (maxAmount == 0)
                {
                    craftableQuantity = 0;
                    break;
                }

                if (maxAmount < craftableQuantity)
                    craftableQuantity = (ushort)maxAmount;
            }

            MaxCraftable = craftableQuantity;
            craftableItemNameText.text = $"{CraftableItem.Item.Name} [{MaxCraftable}]";
        }
    }
}
