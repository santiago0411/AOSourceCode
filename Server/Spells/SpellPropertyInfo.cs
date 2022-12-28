using AO.Core.Ids;
using JetBrains.Annotations;

namespace AO.Spells
{
    public readonly struct SpellPropertyInfo
    {
        [UsedImplicitly]
        public readonly SpellId SpellId;
        [UsedImplicitly]
        public readonly SpellProperty Property;
        [UsedImplicitly]
        public readonly string Value;
        
        public void Deconstruct(out SpellProperty property, out string value)
        {
            property = Property;
            value = Value;
        }
    }
}