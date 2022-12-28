using AO.Core.Logging;
using AO.Items;

namespace AO.Npcs.Utils
{
    public class NpcInventorySlot
    {
        public readonly byte Slot;
        public readonly Item Item;
        public readonly ushort OriginalQuantity;
        public ushort Quantity { get; set; }
        public readonly bool Respawns;
        public int Price => Item.Price;
        
        private static readonly LoggerAdapter log = new(typeof(NpcInventorySlot));
        
        public NpcInventorySlot(byte slot, Item item, ushort quantity, bool respawns)
        {
            Slot = slot; 
            Item = item; 
            OriginalQuantity = quantity;
            Quantity = quantity;
            Respawns = respawns;
            
            if (item.Price == 0)
                log.Warn("{0} item {1} has a price of 0!", nameof(NpcInventorySlot), item.Name);
        }
    }
}