using AOClient.Network;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AOClient.UI.Utils
{
    public sealed class ItemDropHandler : MonoBehaviour, IDropHandler
    {
        [SerializeField] private RectTransform invPanelTransform;

        public void OnDrop(PointerEventData eventData)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (!RectTransformUtility.RectangleContainsScreenPoint(invPanelTransform, Input.mousePosition) && RectTransformUtility.RectangleContainsScreenPoint(UIManager.GameUI.CameraTransform, Input.mousePosition))
            {
                UIManager.GameUI.DropItems.DropPosition = worldPos;
                UIManager.GameUI.DropItems.DragAndDrop = true;

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    UIManager.GameUI.DropItems.Drop(true);
                else
                    UIManager.GameUI.DropItems.Show();
            }
            else
            {
                byte selectedSlot = UIManager.GameUI.InventoryUI.SelectedInventorySlot.SlotId;
                foreach (var rect in UIManager.GameUI.InventoryUI.SlotsTransforms)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition))
                    {
                        string slot = rect.gameObject.name.Split('(')[1].Split(')')[0];
                        byte.TryParse(slot, out byte newSlot);
                        PacketSender.PlayerSwappedItemSlot(selectedSlot, newSlot);
                        break;
                    }
                }
            }
        }
    }
}
