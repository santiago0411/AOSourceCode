using AOClient.Network;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AOClient.UI.Main.Inventory
{
    public class DoubleClickHandler : MonoBehaviour, IPointerDownHandler
    {
        private float clickTime;
        private readonly float doubleClickTime = 0.3f;

        public void OnPointerDown(PointerEventData eventData)
        {
            if ((Time.realtimeSinceStartup - clickTime) < doubleClickTime)
            {
                byte selectedSlotId = UIManager.GameUI.InventoryUI.SelectedInventorySlot.SlotId;
                PacketSender.PlayerItemAction(selectedSlotId, true);
            }

            clickTime = Time.realtimeSinceStartup;
        }
    }
}
