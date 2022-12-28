namespace AOClient.Player.Utils
{
    public class Inventory
    {
        public byte Slot { get; set; }
        public Item Item { get; }
        public ushort Quantity { get; set; }
        public bool Equipped { get; set; }
        public uint SellingPrice { get; }

        public Inventory(byte slot, Item item, ushort quantity, uint sellingPrice, bool equipped = false)
        {
            Slot = slot; Item = item; Quantity = quantity; SellingPrice = sellingPrice; Equipped = equipped;
        }
    }
}

