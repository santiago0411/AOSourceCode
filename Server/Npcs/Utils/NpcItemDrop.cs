using AO.Core.Ids;

namespace AO.Npcs.Utils
{
    public readonly struct NpcItemDrop
    {
        public readonly ItemId ItemId;
        public readonly ushort Quantity;
        public readonly float DropChance;

        public NpcItemDrop(ItemId itemId, ushort quantity, float dropChance)
        {
            ItemId = itemId; 
            Quantity = quantity;
            DropChance = dropChance;
        }
    }
}