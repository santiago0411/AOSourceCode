using AO.Core.Ids;

namespace AOClient.Npcs
{
    public class NpcInventory
    {
        public readonly byte Slot;
        public readonly ItemId ItemId;
        public ushort Quantity;
        public readonly int Price;

        public NpcInventory(byte slot, ItemId itemId, ushort quantity, int price)
        {
            Slot = slot; ItemId = itemId; Quantity = quantity; Price = price;
        }
    }
}
