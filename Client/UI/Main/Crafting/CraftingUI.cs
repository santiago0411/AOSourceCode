using System;
using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Player;
using AOClient.Player.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Crafting
{
    public class CraftingUI : MonoBehaviour
    {
        public readonly Dictionary<ItemId, CraftableItem> BlacksmithingItems = new();
        public readonly Dictionary<ItemId, CraftableItem> WoodworkingItems = new();
        public readonly Dictionary<ItemId, CraftableItem> TailoringItems = new();

        [Header("General")]
        [SerializeField] private Button exitButton;
        [SerializeField] private TextMeshProUGUI professionNameText;
        [SerializeField] private Button createButton, createAllButton;
        [SerializeField] private TMP_InputField quantityInput;

        [Header("Skill")]
        [SerializeField] private TextMeshProUGUI currentSkillField;
        [SerializeField] private Slider skillSlider;

        [Header("Craftable Items Panel")]
        [SerializeField] private CraftableItemUI craftableItemListItemPrefab;
        [SerializeField] private GameObject itemsContainer;

        [Header("Required Items Panel")]
        [SerializeField] private RequiredItemUI requiredItemPrefab;
        [SerializeField] private Image itemToCraftImage;
        [SerializeField] private TextMeshProUGUI itemToCraftNameText;
        [SerializeField] private GameObject requiredItemsContainer;

        private CraftableItemUI selectedItem;
        private readonly List<CraftableItemUI> craftableItemsInContainer = new();
        private readonly List<RequiredItemUI> requiredItemsList = new();

        private void Start()
        {
            exitButton.onClick.AddListener(Close);
            createButton.onClick.AddListener(CraftItem);
            createAllButton.onClick.AddListener(CraftAll);
            quantityInput.onValidateInput = ValidateInput;
            gameObject.SetActive(false);
        }

        public void OpenAndLoad(CraftingProfession profession, byte skillInProf)
        {
            DestroyCraftableItemsPanel();
            DestroyRequiredItemsPanel();

            currentSkillField.text = skillInProf.ToString();
            skillSlider.value = skillInProf;

            switch (profession)
            {
                case CraftingProfession.Blacksmithing:
                    professionNameText.text = "Herrería";
                    LoadCraftableItems(BlacksmithingItems.Values);
                    break;
                case CraftingProfession.Woodworking:
                    professionNameText.text = "Carpintería";
                    LoadCraftableItems(WoodworkingItems.Values);
                    break;
                case CraftingProfession.Tailoring:
                    professionNameText.text = "Sastrería";
                    LoadCraftableItems(TailoringItems.Values);
                    break;
                default:
                    gameObject.SetActive(false);
                    return;
            }

            gameObject.SetActive(true);
        }

        public void UpdateQuantities(ItemId updateSlotItemId)
        {
            if (!gameObject.activeSelf) return;

            foreach (var requiredItem in requiredItemsList)
            {
                if (requiredItem.RequiredItemId == updateSlotItemId)
                {
                    int totalAmount = GameManager.Instance.LocalPlayer.GetSlotsWithItem(requiredItem.RequiredItemId).Sum(x => x.Quantity);
                    requiredItem.AmountInInventoryText.text = totalAmount.ToString();
                }
            }

            foreach (var craftableItem in craftableItemsInContainer)
                craftableItem.UpdateMaxCraftable();
        }

        public void UpdateSkill(Skill skill, byte value)
        {
            if (!gameObject.activeSelf) return;

            if (skill is Skill.Blacksmithing or Skill.Woodworking or Skill.Tailoring)
            {
                currentSkillField.text = value.ToString();
                skillSlider.value = value;
            }
        }

        private void CraftItem()
        {
            if (selectedItem is null) return;
            ushort.TryParse(quantityInput.text, out ushort amount);
            if (amount < 1) return;
            PacketSender.CraftItem(selectedItem.CraftableItem.Profession, selectedItem.CraftableItem.Item.Id, amount);
        }

        private void CraftAll()
        {
            if (selectedItem is null) return;
            quantityInput.text = selectedItem.MaxCraftable.ToString();
            PacketSender.CraftItem(selectedItem.CraftableItem.Profession, selectedItem.CraftableItem.Item.Id, selectedItem.MaxCraftable);
        }

        private void LoadCraftableItems(IEnumerable<CraftableItem> items)
        {
            DestroyCraftableItemsPanel();

            foreach (var craftableItem in items)
            {
                CraftableItemUI craftableItemUI = Instantiate(craftableItemListItemPrefab, itemsContainer.transform);
                craftableItemUI.LoadCraftableItem(craftableItem);
                craftableItemUI.Button.onClick.AddListener(() => OnCraftableItemButtonClick(craftableItemUI, craftableItem));
                craftableItemsInContainer.Add(craftableItemUI);
            }

            if (craftableItemsInContainer.Count > 0)
            {
                selectedItem = craftableItemsInContainer[0];
                selectedItem.Button.onClick.Invoke();
            }
        }

        private void OnCraftableItemButtonClick(CraftableItemUI craftableItemUI, CraftableItem craftableItem)
        {
            selectedItem.HighlightImage.enabled = false;
            DestroyRequiredItemsPanel();
            itemToCraftImage.sprite = GameManager.Instance.GetSprite(craftableItem.Item.GraphicId);
            itemToCraftNameText.text = craftableItem.Item.Name;
            LoadItemRequirements(craftableItem);
            selectedItem = craftableItemUI;
            selectedItem.HighlightImage.enabled = true;
        }

        private void LoadItemRequirements(CraftableItem item)
        {
            requiredItemsList.Clear();

            foreach (var (requiredItem, requiredAmount) in item.RequiredItemsAndAmounts)
            {
                int totalAmount = GameManager.Instance.LocalPlayer.GetSlotsWithItem(requiredItem.Id).Sum(x => x.Quantity);
                RequiredItemUI requiredItemUI = Instantiate(requiredItemPrefab, requiredItemsContainer.transform);

                requiredItemUI.RequiredItemId = requiredItem.Id;
                requiredItemUI.ItemImage.sprite = GameManager.Instance.GetSprite(requiredItem.GraphicId);
                requiredItemUI.ItemNameText.text = requiredItem.Name;
                requiredItemUI.AmountInInventoryText.text = totalAmount.ToString();
                requiredItemUI.RequiredAmountText.text = requiredAmount.ToString();

                requiredItemsList.Add(requiredItemUI);
            }
        }

        private void DestroyCraftableItemsPanel()
        {
            craftableItemsInContainer.Clear();
            selectedItem = null;

            foreach (Transform child in itemsContainer.transform)
                Destroy(child.gameObject);
        }

        private void DestroyRequiredItemsPanel()
        {
            itemToCraftImage.sprite = null;
            itemToCraftNameText.text = string.Empty;

            foreach (Transform child in requiredItemsContainer.transform)
                Destroy(child.gameObject);
        }

        private static char ValidateInput(string text, int charIndex, char addedChar)
        {
            return char.IsDigit(addedChar) ? addedChar : (char)0x00;
        }

        private void Close()
        {
            gameObject.SetActive(false);
            PacketSender.CloseCraftingWindow();
        }
    }
}
