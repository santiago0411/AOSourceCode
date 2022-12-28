using System;
using AO.Spells;
using AO.World;
using UnityEngine;

namespace AO.Npcs.AI
{
    public interface INpcAITarget
    {
        GameObject gameObject { get; }
        bool IsDead { get; }
        Tile CurrentTile { get; }
        void AttackTarget(Npc attacker);
        void CastSpellOnTarget(Spell spell, Npc attacker);
        event Action<INpcAITarget> TargetMoved;
        event Action<INpcAITarget> TargetDied;
    }
}
