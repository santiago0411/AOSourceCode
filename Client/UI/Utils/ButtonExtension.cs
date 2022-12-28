using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AOClient.UI.Utils
{
    public class ButtonExtension : Button
    {
        public UnityEvent onLeftClick;
        public UnityEvent onRightClick;
        public UnityEvent onMiddleClick;
        
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;
            
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    onLeftClick?.Invoke();
                    break;
                case PointerEventData.InputButton.Right:
                    onRightClick?.Invoke();
                    break;
                case PointerEventData.InputButton.Middle:
                    onMiddleClick?.Invoke();
                    break;
            }
        }
    }
}