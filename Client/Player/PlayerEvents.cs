using System;
using AOClient.Player.Utils;

namespace AOClient.Player
{
    public class PlayerEvents
    {
        public event Action<Inventory> InventorySlotChanged;
        public event Action<uint> TotalGoldChanged;

        public void RaiseInventorySlotChanged(Inventory slot) => InventorySlotChanged?.Invoke(slot);
        public void RaiseTotalGoldChanged(uint gold) => TotalGoldChanged?.Invoke(gold);
    }
}