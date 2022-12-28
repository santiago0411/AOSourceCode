using AO.Players;

namespace AO.Npcs.AI.Behaviours
{
    public sealed class HostileAttackingBehaviour : AttackingBehaviourBase
    {
        protected override bool IsTargetAttackable(Player target)
        {
            return !target.IsDead;
        }

        protected override bool IsTargetAttackable(Npc npc)
        {
            return npc.IsPet && !npc.IsDead;
        }
    }
}