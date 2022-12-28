using UnityEngine.EventSystems;
using UnityEngine;

namespace AOClient.UI.Utils
{
    public class SpellMouseOverHandler : MonoBehaviour, IPointerEnterHandler
    {
        private byte slot;

        private void Start()
        {
            string slotStr = gameObject.name.Split('(')[1].Split(')')[0];
            slot = byte.Parse(slotStr);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Input.GetMouseButton(0))
                UIManager.GameUI.Spells.ClickSpell(slot);
        }
    }
}
