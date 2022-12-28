using AO.Core.Ids;
using AOClient.Core;
using AOClient.UI.Main.Inventory;
using AOClient.UI.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AOClient.UI.Main.Mailing
{
    public class MailItemUI : MonoBehaviour, IPoolObject
    {
        public byte SlotId => inventorySlot.SlotId;
        public ItemId ItemId { get; private set; }
        public UnityEvent OnLeftClick => button.onLeftClick;
        public UnityEvent OnRightClick => button.onRightClick;
        
        private InventorySlotUI inventorySlot;
        
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private ButtonExtension button;

        public void SetInventorySlot(InventorySlotUI slot)
        {
            inventorySlot = slot;
            itemImage.sprite = slot.ItemImage.sprite;
            quantityText.text = slot.QuantityText.text;
            slot.GreyOut();
            gameObject.SetActive(true);
        }

        public void SetItem(ItemId itemId, uint quantity)
        {
            ItemId = itemId;
            itemImage.sprite = GameManager.Instance.GetSpriteByItemId(itemId);
            quantityText.text = quantity.ToString();
            gameObject.SetActive(true);
        }

        public int InstanceId => GetInstanceID();
        public bool IsBeingUsed => gameObject.activeSelf;
        
        public void ResetPoolObject()
        {
            if (inventorySlot != null)
            {
                inventorySlot.ResetColor();
                inventorySlot = null;
            }
            
            gameObject.SetActive(false);
            OnLeftClick.RemoveAllListeners();
            OnRightClick.RemoveAllListeners();
        }
    }
}