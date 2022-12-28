using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AOClient.UI.Utils
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class ButtonImageChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Sprite baseImage, onMouseOverImage, onClickImage;

        private Image image;

        private void Start()
        {
            image = GetComponent<Image>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            image.sprite = onMouseOverImage;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            image.sprite = baseImage;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            image.sprite = onClickImage;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            image.sprite = baseImage;
        }
    }
}