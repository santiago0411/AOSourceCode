using System;
using AO.Players;
using UnityEngine;

namespace AO.Npcs.AI.Behaviours
{
    public abstract class TargetingBehaviourBase : MonoBehaviour
    {
        public abstract INpcAITarget CurrentTarget { get; protected set; }
        public abstract Action OnTargetFound { get; set; }
        public abstract Action<INpcAITarget> OnTargetMoved { get; set; }
        public abstract Action OnCurrentTargetInvalidated { get; set; }
        
        protected Faction FactionsThatCanBeAttacked { get; private set; }
        
        public virtual void Init() {}
        public virtual void Destroyed() {}
        public abstract void TryFindNewTarget();

        private void Start()
        {
            var thisNpc = GetComponent<Npc>();
            thisNpc.NpcAttacked += OnThisNpcAttacked;
            switch (thisNpc.Info.NpcFaction)
            {
                case NpcFaction.Neutral:
                    FactionsThatCanBeAttacked = Faction.Citizen | Faction.Criminal | Faction.Imperial | Faction.Chaos;
                    break;
                case NpcFaction.Imperial:
                    FactionsThatCanBeAttacked = Faction.Criminal | Faction.Chaos;
                    break;
                case NpcFaction.Chaos:
                    FactionsThatCanBeAttacked = Faction.Citizen | Faction.Imperial;
                    break;
                default:
                    Core.AoDebug.Assert(false, $"NpcFaction has an invalid value '{thisNpc.Info.NpcFaction}'");
                    break;
            }
            Init();
        }

        private void OnDestroy()
        {
            GetComponent<Npc>().NpcAttacked -= OnThisNpcAttacked;
            CleanUpCurrentTarget();
        }

        public void InvalidateTarget()
        {
            if (IsCurrentTargetValid())
                CleanUpCurrentTarget();
        }
        
        public bool IsCurrentTargetValid()
        {
            return IsTargetValid(CurrentTarget);
        }
        
        protected static bool IsTargetValid(INpcAITarget target)
        {
            return target is not null && target.gameObject && !target.IsDead;
        }
        
        protected virtual void SetNewTarget(INpcAITarget target)
        {
            CurrentTarget = target;
            CurrentTarget.TargetMoved += OnTargetMoved;
            CurrentTarget.TargetDied += OnTargetDied;
            OnTargetFound();
        }

        protected virtual void CleanUpCurrentTarget()
        {
            if (CurrentTarget is not null)
            {
                CurrentTarget.TargetMoved -= OnTargetMoved;
                CurrentTarget.TargetDied -= OnTargetDied;
            }

            CurrentTarget = null;
        }
        
        private void OnTargetDied(INpcAITarget target)
        {
            OnCurrentTargetInvalidated();
            CleanUpCurrentTarget();
        }
        
        private void OnThisNpcAttacked(INpcAITarget attacker)
        {
            if (!IsCurrentTargetValid())
                SetNewTarget(attacker);
        }
    }
}