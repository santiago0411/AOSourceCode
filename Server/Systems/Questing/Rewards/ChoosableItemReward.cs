using System.Collections.Generic;
using System.Collections.ObjectModel;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Items;
using AO.Players;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Rewards
{
    public sealed class ChoosableItemReward : IQuestReward
    {
        [JsonProperty("Classes")] 
        private readonly HashSet<ClassType> classes;
        
        [JsonProperty("ChoosableItems")]
        [JsonConverter(typeof(CustomReadOnlyDictionaryConverter<ItemId, ushort>))]
        private readonly ReadOnlyDictionary<ItemId, ushort> choosableItems;
        
        public void AssignReward(Player toPlayer)
        {
            if (classes is not null && !classes.Contains(toPlayer.Class.ClassType))
                return;
            
            if (!choosableItems.TryGetValue(toPlayer.Flags.SelectedItemRewardId, out ushort quantity))
                return;
            
            Item chosenItem = GameManager.Instance.GetItem(toPlayer.Flags.SelectedItemRewardId);
            AddOrMailItem(toPlayer, chosenItem, quantity);
        }
        
        private static void AddOrMailItem(Player toPlayer, Item item, ushort quantity)
        {
            if (toPlayer.Inventory.AddItemToInventory(item, quantity))
                return;

            // TODO forcefully mail item to player instead
        }
    }
}