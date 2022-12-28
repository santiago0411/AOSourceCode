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
    public class ItemReward : IQuestReward
    {
        [JsonProperty("Classes")] 
        private readonly HashSet<ClassType> classes;
        
        [JsonProperty("ItemsRewarded")]
        [JsonConverter(typeof(CustomReadOnlyDictionaryConverter<ItemId, ushort>))]
        private readonly ReadOnlyDictionary<ItemId, ushort> itemsRewarded;

        public void AssignReward(Player toPlayer)
        {
            if (classes is not null && !classes.Contains(toPlayer.Class.ClassType))
                return;
            
            //Loop and give the player all the items that this quest always rewards
            foreach (var (itemId, amount) in itemsRewarded)
                AddOrMailItem(toPlayer, GameManager.Instance.GetItem(itemId), amount);
        }

        private static void AddOrMailItem(Player toPlayer, Item item, ushort quantity)
        {
            if (toPlayer.Inventory.AddItemToInventory(item, quantity))
                return;

            // TODO forcefully mail item to player instead
        }
    }
}