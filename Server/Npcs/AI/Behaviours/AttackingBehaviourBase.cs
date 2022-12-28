using System;
using AO.Core;
using AO.Core.Utils;
using AO.Players;
using AO.World;
using UnityEngine;

namespace AO.Npcs.AI.Behaviours
{
    public abstract class AttackingBehaviourBase : MonoBehaviour
    {
        protected abstract bool IsTargetAttackable(Player target);
        protected abstract bool IsTargetAttackable(Npc npc);
        
        protected Npc ThisNpc { get; private set; }
        protected NpcAIBase ThisAI { get; private set; }

        private void Start()
        {
            ThisNpc = GetComponent<Npc>();
            ThisAI = GetComponent<NpcAIBase>();
        }

        public virtual void TryCastingSpell(INpcAITarget target)
        {
            if (ThisNpc.Info.Spells.Count < 1)
                return;

            if (!Timers.NpcCanCastSpellInterval(ThisNpc))
                return;

            if (ExtensionMethods.RandomNumber(1, 2) == 1)
                return;
            
            int randomSpell = UnityEngine.Random.Range(0, ThisNpc.Info.Spells.Count);
            var spellToCast = ThisNpc.Info.Spells[randomSpell].Spell;
            target.CastSpellOnTarget(spellToCast, ThisNpc);
        }
        
        public virtual bool TryAttacking(INpcAITarget _)
        {
            if (ThisNpc.Info.CasterOnly)
                return false;
            
            if (!TryFindAttackableTarget(ThisNpc, ThisAI.FacingDirection, out INpcAITarget target)) 
                return false;
            
            if (Timers.NpcCanAttackInterval(ThisNpc))
            {
                target.AttackTarget(ThisNpc);
                ThisAI.FacingDirection = target.CurrentTile.Position - ThisNpc.CurrentTile.Position;
            }

            return true;
        }

        private bool TryFindAttackableTarget(Npc thisNpc, Vector2 facingDirection, out INpcAITarget target)
        {
            Span<Facing> facingDirections = stackalloc Facing[] { Facing.Up, Facing.Right, Facing.Down, Facing.Left };

            foreach (Facing facing in facingDirections)
            {
                Tile tile = thisNpc.CurrentTile.Neighbours[facing.TileNeighbourIndex];
                
                if (thisNpc.Flags.IsImmobilized && facing.Direction != facingDirection)
                    continue;

                if (TryFindAttackableTargetInTile(tile, out target))
                    return true;
            }

            target = null;
            return false;
        }

        private bool TryFindAttackableTargetInTile(Tile tile, out INpcAITarget target)
        {
            target = null;
            
            if (tile is null)
                return false;
            
            if (tile.Player && IsTargetAttackable(tile.Player))
            {
                target = tile.Player;
                return true;
            }

            if (tile.Npc && IsTargetAttackable(tile.Npc))
            {
                target = tile.Npc;
                return true;
            }
            
            return false;
        }
    }
}