using AO.Players;
using AO.Systems;
using AO.Systems.Combat;

namespace AO.Npcs.AI.Behaviours
{
    public sealed class PetAttackingBehaviour : AttackingBehaviourBase
    {
        protected override bool IsTargetAttackable(Player target)
        {
            var owner = ThisNpc.PetOwner;
            
            if (target.Flags.IsDead)
                return false;

            if (owner.Id == target.Id)
                return false;

            if (CombatSystemUtils.BothInArena(owner, target))
                return true;

            if (Party.ArePlayersInSameParty(owner, target)) // TODO || same clan
                return false;

            if (owner.Faction is Faction.Citizen or Faction.Imperial)
                if (target.Faction is Faction.Citizen or Faction.Imperial)
                    return false;

            return true;
        }

        protected override bool IsTargetAttackable(Npc npc)
        {
            var owner = ThisNpc.PetOwner;
            
            if (npc.IsPet)
                return IsTargetAttackable(npc.PetOwner);

            var targetNpcOwner = npc.Flags.CombatOwner; 
            
            if (targetNpcOwner is null)
                return true;
            
            if (targetNpcOwner.Id == owner.Id)
                return true;
            
            if (Party.ArePlayersInSameParty(owner, targetNpcOwner)) // TODO || same clan
                return true;
            
            if (owner.Faction is Faction.Citizen or Faction.Imperial)
                if (targetNpcOwner.Faction is Faction.Citizen or Faction.Imperial)
                    return false;

            return true;
        }
    }
}