using AOClient.Network;
using AOClient.UI.Main.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main
{
    public sealed class DropItemsUI : MonoBehaviour
    {
        public Vector2 DropPosition { get; set; }
        public bool DragAndDrop { get; set; }

        [SerializeField] private TMP_InputField quantityField;
        [SerializeField] private Button dropQuantityButton, dropAllButton, dropGoldButton;

        private InventoryUI inventory;
        private bool droppingGold;

        private void Start()
        {
            inventory = UIManager.GameUI.InventoryUI;
            dropQuantityButton.onClick.AddListener(() => Drop(false));
            dropAllButton.onClick.AddListener(() => Drop(true));

            dropGoldButton.onClick.AddListener(() => 
            {
                gameObject.SetActive(true);
                droppingGold = true;
                quantityField.text = "1";
            });

            gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            droppingGold = false;

            if (inventory.SelectedInventorySlot.QuantityText.text.Equals("1"))
            {
                Drop(true);
                gameObject.SetActive(false);
            }
            else
                quantityField.text = "1";
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Drop(bool all)
        {
            if (droppingGold)
            {
                DropGold(all);
                return;
            }

            ushort quantity;

            if (all)
            {
                ushort.TryParse(inventory.SelectedInventorySlot.QuantityText.text, out quantity);
                if (quantity == 0)
                    return;
            }
            else
            {
                if (!ushort.TryParse(quantityField.text, out quantity))
                    quantity = ushort.MaxValue;
                else if (quantity == 0)
                    return;
            }

            if (DragAndDrop)
                PacketSender.PlayerDropItem(inventory.SelectedInventorySlot.SlotId, quantity, true, DropPosition);
            else
                PacketSender.PlayerDropItem(inventory.SelectedInventorySlot.SlotId, quantity, false);

            gameObject.SetActive(false);
        }

        private void DropGold(bool all)
        {
            if (all)
            {
                PacketSender.DropGold(100000);
            }
            else
            {
                if (uint.TryParse(quantityField.text, out var quantity))
                    PacketSender.DropGold(quantity);
            }

            gameObject.SetActive(false);
        }
    }
}
