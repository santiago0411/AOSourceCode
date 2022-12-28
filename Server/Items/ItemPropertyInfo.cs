using AO.Core.Ids;
using JetBrains.Annotations;

namespace AO.Items
{
    public readonly struct ItemPropertyInfo
    {
        [UsedImplicitly]
        public readonly ItemId ItemId;
        [UsedImplicitly]
        public readonly ItemProperty Property;
        [UsedImplicitly]
        public readonly int Value;

        public void Deconstruct(out ItemProperty property, out int value)
        {
            property = Property;
            value = Value;
        }
    }
}