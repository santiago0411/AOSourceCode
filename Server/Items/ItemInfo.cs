using AO.Core.Ids;
using JetBrains.Annotations;

namespace AO.Items
{
    public readonly struct ItemInfo
    {
        [UsedImplicitly]
        public readonly ItemId Id;
        [UsedImplicitly]
        public readonly string Name;
        [UsedImplicitly]
        public readonly ItemType ItemType;

        public void Deconstruct(out ItemId id, out string name, out ItemType type)
        {
            id = Id;
            name = Name;
            type = ItemType;
        }
    }
}