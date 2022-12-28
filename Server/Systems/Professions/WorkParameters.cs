using AO.Items;

namespace AO.Systems.Professions
{
    public class WorkParameters
    {
        public Item[] CollectingItems;
        public CraftableItem CraftableItem;
        public ushort AmountToCraft;
        public float IntervalModifier = 1f;
        public float FishingAmountModifier = 1f;
        public float SmeltingOreRequiredModifier = 1f;
    }
}
