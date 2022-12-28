using AO.Core.Ids;
using AO.Spells;

namespace AO.Npcs.Utils
{
    public readonly struct NpcSpell
    {
        public readonly Spell Spell;
        public readonly byte Cooldown;

        public NpcSpell(SpellId spellId, byte cooldown)
        {
            Spell = Core.GameManager.Instance.GetSpell(spellId);
            Cooldown = cooldown;
        }
    }
}
