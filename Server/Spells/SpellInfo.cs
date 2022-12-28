using AO.Core.Ids;
using JetBrains.Annotations;

namespace AO.Spells
{
    public readonly struct SpellInfo
    {
        [UsedImplicitly]
        public readonly SpellId Id;

        [UsedImplicitly] 
        public readonly SpellType Type;
        
        [UsedImplicitly]
        public readonly SpellTarget Target;
        
        [UsedImplicitly]
        public readonly ushort Particle;
        
        [UsedImplicitly]
        public readonly byte MinSkill;
        
        [UsedImplicitly]
        public readonly ushort ManaRequired;
        
        [UsedImplicitly]
        public readonly ushort StaminaRequired;
        
        [UsedImplicitly]
        public readonly bool UsesMagicItemBonus;
        
        [UsedImplicitly]
        public readonly byte MagicItemPowerNeeded;

        public void Deconstruct(out SpellId id, out SpellTarget spellTarget, out ushort particle, out byte minSkill,
            out ushort manaRequired, out ushort staminaRequired, out bool usesMagicItemBonus,
            out byte magicItemPowerNeeded)
        {
            id = Id;
            spellTarget = Target;
            particle = Particle;
            minSkill = MinSkill;
            manaRequired = ManaRequired;
            staminaRequired = StaminaRequired;
            usesMagicItemBonus = UsesMagicItemBonus;
            magicItemPowerNeeded = MagicItemPowerNeeded;
        }
    }
}