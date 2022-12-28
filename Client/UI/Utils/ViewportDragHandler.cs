using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AOClient.UI.Utils
{
    public class ViewportDragHandler : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Scrollbar scrollbar;

        private bool canDrag;
        private bool fixedUpdateDone = true;

        private void FixedUpdate()
        {
            //Since OnDrag gets called on Update the more FPS the more it gets called so on build it would instantly scroll to the end
            fixedUpdateDone = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canDrag)
            {
                Vector2 mousePos = Input.mousePosition;
                if (mousePos.y < 240f)
                {
                    if (fixedUpdateDone)
                    {
                        scrollbar.value -= 0.1f;
                        fixedUpdateDone = false;
                    }
                }
                else
                {
                    if (fixedUpdateDone)
                    {
                        scrollbar.value += 0.1f;
                        fixedUpdateDone = false;
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            canDrag = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            canDrag = true;
        }
    }
}
