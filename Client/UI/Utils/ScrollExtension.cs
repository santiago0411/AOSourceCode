using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AOClient.UI.Utils
{
    public class ScrollExtension : ScrollRect
    {
        public override void OnBeginDrag(PointerEventData eventData) { }
        public override void OnDrag(PointerEventData eventData) { }
        public override void OnEndDrag(PointerEventData eventData) { }
    }
}