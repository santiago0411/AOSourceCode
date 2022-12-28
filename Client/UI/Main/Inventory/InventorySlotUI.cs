using AOClient.UI.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AOClient.UI.Main.Inventory
{
    public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public byte SlotId { get; private set; }
        public ButtonExtension Button => button;
        public Image HighlightImage => highlightImage;
        public Image ItemImage => itemImage;
        public TextMeshProUGUI QuantityText => quantityText;
        public TextMeshProUGUI EquippedText => equippedText;

        [SerializeField] private ButtonExtension button;
        [SerializeField] private Image highlightImage;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI equippedText;

        [Header("Greyscale")] 
        [SerializeField] private Material defaultMaterial; 
        [SerializeField] private Material greyscaleMaterial;

        private bool mouseOver;
        private float mouseOverTime;

        private void Start()
        {
            highlightImage.enabled = false;
            itemImage.enabled = false;
            quantityText.text = string.Empty;
            equippedText.text = string.Empty;

            SlotId = byte.Parse(name.Split('(')[1].Split(')')[0]);
        }

        private void Update()
        {
            if (!mouseOver)
                return;
            
            mouseOverTime += Time.deltaTime;

            if (mouseOverTime >= 1f && InventoryUI.MouseOverSlot is null)
                InventoryUI.MouseOverSlot = SlotId;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseOver = false;
            mouseOverTime = 0f;
            InventoryUI.MouseOverSlot = null;
        }

        public void GreyOut()
        {
            itemImage.material = greyscaleMaterial;
        }

        public void ResetColor()
        {
            itemImage.material = defaultMaterial;
        }
    }
}
