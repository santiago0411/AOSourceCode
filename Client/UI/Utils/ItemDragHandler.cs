using AOClient.UI.Main.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AOClient.UI.Utils
{
    public sealed class ItemDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            UIManager.GameUI.InventoryUI.OnSlotLeftClicked(GetComponentInParent<InventorySlotUI>());
            Vector2 worldPos = Input.mousePosition;
            transform.position = worldPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            transform.localPosition = Vector3.zero;
        }
    }
}
