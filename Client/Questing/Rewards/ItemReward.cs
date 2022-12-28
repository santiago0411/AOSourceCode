using System.Collections.ObjectModel;
using AO.Core.Ids;
using AO.Core.Utils;
using AOClient.Core;
using AOClient.Player;
using AOClient.UI;
using AOClient.UI.Main.Questing;
using Newtonsoft.Json;
using UnityEngine;

namespace AOClient.Questing.Rewards
{
    public class ItemReward : IQuestReward
    {
        [JsonProperty("Classes")] 
        private readonly ReadOnlyCollection<ClassType> classes;
        
        [JsonProperty("ItemsRewarded")] 
        [JsonConverter(typeof(CustomReadOnlyDictionaryConverter<ItemId, ushort>))]
        private readonly ReadOnlyDictionary<ItemId, ushort> items;

        public void AddRewardToPanel(Transform panelTransform)
        {
            if (classes != null && !classes.Contains(GameManager.Instance.LocalPlayer.Class))
                return;

            foreach (var (itemId, quantity) in items)
            {
                var itemRewardUI = UIManager.GameUI.QuestWindow.QuestRewardsUI.ItemRewardInstance;
                SetItemUIProperties(itemRewardUI, panelTransform, itemId, quantity);
            }
        }
        
        private static void SetItemUIProperties(ItemRewardUI itemRewardUI, Transform panel, ItemId itemId, ushort quantity)
        {
            itemRewardUI.IsBeingUsed = true;
            itemRewardUI.SelectButton.interactable = false;
            itemRewardUI.ItemId = itemId;
            itemRewardUI.ItemSprite.sprite = GameManager.Instance.GetSpriteByItemId(itemId);
            itemRewardUI.QuantityText.text = quantity.ToString();
            itemRewardUI.transform.SetParent(panel, false);
        }
    }
}