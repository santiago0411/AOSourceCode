using System.Collections.Generic;
using AOClient.Core;
using AOClient.UI.Main.Mailing;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Inventory
{
    public sealed class InventoryUI : MonoBehaviour
    {
        public static byte? MouseOverSlot { get; set; }

        public InventorySlotUI SelectedInventorySlot { get; private set; }
        public readonly List<RectTransform> SlotsTransforms = new();
        public readonly List<InventorySlotUI> InventorySlots = new();

        [SerializeField] private Button showInventoryButton;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private Texture2D labelBackground;

        private GUIStyle style;

        private void Start()
        {
            showInventoryButton.onClick.AddListener(UIManager.GameUI.Spells.HideSpells);

            foreach (Transform child in slotsContainer)
            {
                var inventorySlot = child.gameObject.GetComponent<InventorySlotUI>();
                SlotsTransforms.Add(inventorySlot.transform as RectTransform);
                inventorySlot.Button.onLeftClick.AddListener(() => OnSlotLeftClicked(inventorySlot));
                inventorySlot.Button.onRightClick.AddListener(() => OnSlotRightClicked(inventorySlot));
                InventorySlots.Add(inventorySlot);
            }

            style = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    background = labelBackground
                }
            };

            SelectedInventorySlot = InventorySlots[0];
            OnSlotLeftClicked(SelectedInventorySlot);
        }

        private void OnGUI()
        {
            if (MouseOverSlot is null) 
                return;

            string itemName = GameManager.Instance.LocalPlayer.Inventory[MouseOverSlot.Value]?.Item.Name;

            if (!string.IsNullOrEmpty(itemName))
            {
                var rect = new Rect(Input.mousePosition.x - 40, Screen.height - Input.mousePosition.y + 15, itemName.Length * 7, 20);
                GUI.Label(rect, itemName, style);
            }
        }

        public void UpdateInventory(Player.Utils.Inventory slot, bool remove)
        {
            InventorySlotUI slotUI = InventorySlots[slot.Slot];

            //Check if item has to be removed
            if (remove)
            {
                slotUI.ItemImage.sprite = null;
                slotUI.ItemImage.enabled = false;
                slotUI.QuantityText.text = string.Empty;
                slotUI.EquippedText.text = string.Empty;
            }
            else
            {
                //Only load the graphic if it hasn't been loaded already
                if (slotUI.ItemImage.sprite == null)
                {
                    Sprite graphic = GameManager.Instance.GetSprite(slot.Item.GraphicId);
                    slotUI.ItemImage.sprite = graphic;
                    slotUI.ItemImage.enabled = true;
                }

                slotUI.QuantityText.text = slot.Quantity.ToString();
            }

            UIManager.GameUI.NpcTradeWindow.LoadPlayerInventory(slotUI);
            UIManager.GameUI.CraftingWindow.UpdateQuantities(slot.Item.Id);
        }

        public void SwapSlots(byte slotAId, byte slotBId)
        {
            InventorySlotUI slotA = InventorySlots[slotAId];
            InventorySlotUI slotB = InventorySlots[slotBId];

            var slotASprite = slotA.ItemImage.sprite;
            var slotAQuantity = slotA.QuantityText.text;
            var slotAEquipped = slotA.EquippedText.text;

            if (slotB.ItemImage.enabled)
            {
                slotA.ItemImage.sprite = slotB.ItemImage.sprite;
                slotA.QuantityText.text = slotB.QuantityText.text;
                slotA.EquippedText.text = slotB.EquippedText.text;
            }
            else
            {
                slotA.ItemImage.sprite = null;
                slotA.ItemImage.enabled = false;
                slotA.QuantityText.text  = string.Empty;
                slotA.EquippedText.text = string.Empty;
            }

            slotB.ItemImage.sprite = slotASprite;
            slotB.ItemImage.enabled = true;
            slotB.QuantityText.text = slotAQuantity;
            slotB.EquippedText.text = slotAEquipped;

            if (UIManager.GameUI.NpcTradeWindow.gameObject.activeSelf)
            {
                UIManager.GameUI.NpcTradeWindow.LoadPlayerInventory(slotA);
                UIManager.GameUI.NpcTradeWindow.LoadPlayerInventory(slotB);
            }
        }

        public void ItemEquip(byte slot, bool equipped)
        {
            InventorySlots[slot].EquippedText.text = equipped ? "+" : string.Empty;
        }

        public void OnSlotLeftClicked(InventorySlotUI slot)
        {
            SelectedInventorySlot.HighlightImage.enabled = false;
            slot.HighlightImage.enabled = true;
            SelectedInventorySlot = slot;
        }

        private void OnSlotRightClicked(InventorySlotUI slot)
        {
            SendMailPanelUI sendMailWindow = UIManager.GameUI.MailWindow.SendMailWindow;
            if (sendMailWindow.IsOpen)
                sendMailWindow.OnSlotRightClicked(slot);
        }
    }
}
