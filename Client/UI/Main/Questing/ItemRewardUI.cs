using AO.Core.Ids;
using AOClient.Core;
using AOClient.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Questing
{
    public class ItemRewardUI : MonoBehaviour, IPoolObject
    {
        private static ItemRewardUI selectedItem;
        
        public ItemId ItemId { private get; set; }
        public Image ItemSprite => itemSprite;
        public TextMeshProUGUI QuantityText => quantityText;
        public Button SelectButton => selectButton;
        
        public int InstanceId => GetInstanceID();
        public bool IsBeingUsed { get; set; }
        
        [SerializeField] private Image itemSprite, selectedHighlight;
        [SerializeField] private Button selectButton;
        [SerializeField] private TextMeshProUGUI quantityText;

        private void Start()
        {
            selectedHighlight.gameObject.SetActive(false);
            
            selectButton.onClick.AddListener(() =>
            {
                if (selectedItem != null)
                    selectedItem.selectedHighlight.gameObject.SetActive(false);
                
                selectedHighlight.gameObject.SetActive(true);
                selectedItem = this;
                PacketSender.SelectQuestItemReward(ItemId);
            });
        }
        
        public void ResetPoolObject()
        {
            IsBeingUsed = false;
            gameObject.SetActive(false);
            selectedHighlight.gameObject.SetActive(false);
        }
    }
}