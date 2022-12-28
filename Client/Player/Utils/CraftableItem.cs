using System.Collections.Generic;
using AOClient.Core.Utils;

namespace AOClient.Player.Utils
{
    public class CraftableItem
    {
        public readonly CraftingProfession Profession;
        public readonly Item Item;
        public readonly List<(Item, ushort)> RequiredItemsAndAmounts;

        public CraftableItem(CraftingProfession profession, Item item, List<(Item, ushort)> requiredItems)
        {
            Profession = profession;
            Item = item;
            RequiredItemsAndAmounts = requiredItems;
        }
    }
}
