using System;
using AO.Core.Utils;
using AO.Players;
using UnityEngine;

namespace AO.Npcs.AI.Behaviours
{
    public sealed class HostileTargetingBehaviour : TargetingBehaviourBase
    {
        public override INpcAITarget CurrentTarget { get; protected set; }
        public override Action OnTargetFound { get; set; }
        public override Action<INpcAITarget> OnTargetMoved { get; set; }
        public override Action OnCurrentTargetInvalidated { get; set; }

        private NpcAIBase npcAI;
        private bool findingNewTarget;
        private readonly CustomUniqueList<Player> playersSubscribedTo = new();

        public override void Init()
        {
            npcAI = GetComponent<NpcAIBase>();
        }

        public override void Destroyed()
        {
            foreach (var player in playersSubscribedTo)
                player.Events.PlayerRevived -= OnPlayerRevived;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer != Layer.PLAYER)
                return;

            var player = collision.GetComponentInParent<Player>();
            player.Events.PlayerRevived += OnPlayerRevived;
            playersSubscribedTo.Add(player);
            
            // If there is a valid current target only hook up the revived event
            if (IsCurrentTargetValid())
                return;
            
            if (IsTargetValid(player))
                SetNewTarget(player);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer != Layer.PLAYER)
                return;

            var player = collision.GetComponentInParent<Player>();
            if (ReferenceEquals(CurrentTarget, player))
            {
                OnCurrentTargetInvalidated();
                CleanUpCurrentTarget();
            }
            
            // Always clean up the revived event
            player.Events.PlayerRevived -= OnPlayerRevived;
            playersSubscribedTo.Remove(player);
        }

        public override void TryFindNewTarget()
        {
            foreach (var player in npcAI.NearbyPlayers)
            {
                var canPlayerFactionBeAttacked = (player.Faction & FactionsThatCanBeAttacked) == player.Faction;
                // Idk if checking HasDisconnected flag is necessary but just in case to avoid a weird edge case where the GO is still alive but the player has technically disconnected
                if (IsTargetValid(player) && canPlayerFactionBeAttacked && !player.Flags.HasDisconnected)
                {
                    SetNewTarget(player);
                    return;
                }
            }
        }

        protected override void SetNewTarget(INpcAITarget target)
        {
            if (target is Player player)
                player.Events.PlayerDisconnected += OnPlayerDisconnected;
            
            base.SetNewTarget(target);
        }
        
        protected override void CleanUpCurrentTarget()
        {
            if (CurrentTarget is Player player)
                player.Events.PlayerDisconnected -= OnPlayerDisconnected;
            
            base.CleanUpCurrentTarget();
        }

        private void OnPlayerRevived(Player player)
        {
            // This event is used in the case there is only one player nearby and they get revived
            // This way there is no need to constantly poll for new targets
            if (!IsCurrentTargetValid())
                SetNewTarget(player);
        }

        private void OnPlayerDisconnected(Player player)
        {
            OnCurrentTargetInvalidated();
            CleanUpCurrentTarget();
        }
    }
}