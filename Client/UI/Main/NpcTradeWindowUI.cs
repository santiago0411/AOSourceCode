using System.Collections.Generic;
using AO.Core.Ids;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AOClient.Core;
using AOClient.Network;
using AOClient.Npcs;
using AOClient.UI.Main.Inventory;

namespace AOClient.UI.Main
{
    public sealed class NpcTradeWindowUI : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private Button exitButton;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private TextMeshProUGUI itemName, itemDescription, itemPrice;

        [Header("Other Window")] 
        [SerializeField] private Button otherWindowButton;
        [SerializeField] private TextMeshProUGUI otherWindowButtonText;
        [SerializeField] private GameObject otherWindowButtonPanel;
        
        [Header("Player")]
        [SerializeField] private Transform playerInventoryPanel;

        [Header("Npc")]
        [SerializeField] private Transform npcInventoryPanel;

        private InventorySlotUI selectedPlayerSlot;
        private InventorySlotUI selectedNpcSlot;

        private NpcInventory[] npcInventory;

        private readonly List<InventorySlotUI> playerInventoryUISlots = new(Constants.PLAYER_INV_SPACE);
        private readonly List<InventorySlotUI> npcInventoryUISlots = new(Constants.NPC_INV_SPACE);

        public void Awake()
        {
            exitButton.onClick.AddListener(() =>
            {
                Close();
                if (otherWindowButtonPanel.activeSelf)
                    UIManager.GameUI.QuestWindow.Close();
            });
                
            buyButton.onClick.AddListener(Buy);
            sellButton.onClick.AddListener(Sell);
            quantityInput.onValidateInput = ValidateInput;
    
            otherWindowButton.onClick.AddListener(() =>
            {
                gameObject.SetActive(false);
                UIManager.GameUI.QuestWindow.ShowWindow(true);
            });
            
            foreach (Transform child in playerInventoryPanel)
            {
                var inventorySlot = child.gameObject.GetComponent<InventorySlotUI>();
                inventorySlot.Button.onLeftClick.AddListener(() => PlayerInventoryClick(inventorySlot.SlotId));
                playerInventoryUISlots.Add(inventorySlot);
            }

            selectedPlayerSlot = playerInventoryUISlots[0];
            selectedPlayerSlot.HighlightImage.enabled = true;

            foreach (Transform child in npcInventoryPanel)
            {
                var inventorySlot = child.gameObject.GetComponent<InventorySlotUI>();
                inventorySlot.Button.onLeftClick.AddListener(() => NpcInventoryClick(inventorySlot.SlotId));
                npcInventoryUISlots.Add(inventorySlot);
            }
            
            selectedNpcSlot = npcInventoryUISlots[0];
            selectedNpcSlot.HighlightImage.enabled = true;
            
            gameObject.SetActive(false);
        }

        public void Open(bool hasQuests)
        {
            gameObject.SetActive(true);
            otherWindowButtonPanel.SetActive(hasQuests);
            
            foreach (var slot in UIManager.GameUI.InventoryUI.InventorySlots)
                LoadPlayerInventory(slot);
        }

        private void PlayerInventoryClick(byte slotId)
        {
            InventorySlotUI newSelectedSlot = playerInventoryUISlots[slotId];

            selectedPlayerSlot.HighlightImage.enabled = false;
            newSelectedSlot.HighlightImage.enabled = true;
            selectedPlayerSlot = newSelectedSlot;

            var inventorySlot = GameManager.Instance.LocalPlayer.Inventory[slotId];

            if (inventorySlot is not null)
            {
                itemName.text = inventorySlot.Item.Name;
                itemDescription.text = inventorySlot.Item.Description;
                itemPrice.text = $"Precio: {inventorySlot.SellingPrice}";
            }
        }

        private void NpcInventoryClick(byte slotId)
        {
            InventorySlotUI newSelectedSlot = npcInventoryUISlots[slotId];

            selectedNpcSlot.HighlightImage.enabled = false;
            newSelectedSlot.HighlightImage.enabled = true;
            selectedNpcSlot = newSelectedSlot;

            NpcInventory inventorySlot = npcInventory[slotId];
            
            if (inventorySlot is not null)
            {
                var item = GameManager.Instance.GetItem(inventorySlot.ItemId);
                itemName.text = item.Name;
                itemDescription.text = item.Description;
                itemPrice.text = $"Precio: {inventorySlot.Price}";
            }
        }

        public void LoadPlayerInventory(InventorySlotUI updatedSlot)
        {
            var slotToUpdate = playerInventoryUISlots[updatedSlot.SlotId];
            slotToUpdate.ItemImage.enabled = updatedSlot.ItemImage.enabled;
            slotToUpdate.ItemImage.sprite = updatedSlot.ItemImage.sprite;
            slotToUpdate.QuantityText.text = updatedSlot.QuantityText.text;
            slotToUpdate.EquippedText.text = updatedSlot.EquippedText.text;
        }

        public void LoadNpcInventory(NpcInventory[] inventory)
        {
            npcInventory = inventory;

            foreach (var slot in npcInventory)
            {
                if (slot is not null)
                {
                    var npcSlotUI = npcInventoryUISlots[slot.Slot];

                    npcSlotUI.ItemImage.sprite = GameManager.Instance.GetSpriteByItemId(slot.ItemId);
                    npcSlotUI.ItemImage.enabled = true;
                    npcSlotUI.QuantityText.text = slot.Quantity.ToString();
                }
            }
        }

        public void UpdateNpcInventory(byte slot, ushort quantity, ItemId itemId, int price)
        {
            var npcSlotUI = npcInventoryUISlots[slot];

            // If item id is not 0 its a new slot
            if (itemId != 0)
            {
                npcInventory[slot] = new NpcInventory(slot, itemId, quantity, price);

                npcSlotUI.ItemImage.sprite = GameManager.Instance.GetSpriteByItemId(itemId);
                npcSlotUI.ItemImage.enabled = true;
            }

            if (quantity <= 0)
            {
                npcSlotUI.ItemImage.enabled = false;
                npcSlotUI.QuantityText.text = string.Empty;
                return;
            }

            npcInventory[slot].Quantity = quantity;
            npcSlotUI.QuantityText.text = quantity.ToString();
        }

        public bool NpcInventorySlotIsNull(byte slot)
        {
            return npcInventory[slot] is null;
        }

        private void Buy()
        {
            ushort.TryParse(quantityInput.text, out ushort quantity);
            if (quantity > 0)
                PacketSender.NpcTrade(true, selectedNpcSlot.SlotId, quantity);
        }

        private void Sell()
        {
            ushort.TryParse(quantityInput.text, out ushort quantity);
            if (quantity > 0)
                PacketSender.NpcTrade(false, selectedPlayerSlot.SlotId, quantity);
        }

        public void Close()
        {
            PacketSender.EndNpcTrade();

            foreach (var playerSlotUI in playerInventoryUISlots)
            {
                playerSlotUI.HighlightImage.enabled = false;
                playerSlotUI.ItemImage.enabled = false;
                playerSlotUI.QuantityText.text = string.Empty;
                playerSlotUI.EquippedText.text = string.Empty;
            }

            selectedPlayerSlot = playerInventoryUISlots[0];
            selectedPlayerSlot.HighlightImage.enabled = true;

            foreach (var npcSlotUI in npcInventoryUISlots)
            {
                npcSlotUI.HighlightImage.enabled = false;
                npcSlotUI.ItemImage.enabled = false;
                npcSlotUI.QuantityText.text = string.Empty;
                npcSlotUI.EquippedText.text = string.Empty;
            }

            selectedNpcSlot = npcInventoryUISlots[0];
            selectedNpcSlot.HighlightImage.enabled = true;

            gameObject.SetActive(false);
        }

        private static char ValidateInput(string text, int charIndex, char addedChar)
        {
            return char.IsDigit(addedChar) ? addedChar : (char)0;
        }
    }
}
