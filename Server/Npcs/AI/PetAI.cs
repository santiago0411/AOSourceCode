using System.Collections;
using AO.Core.Utils;
using AO.Network;
using AO.Npcs.AI.Behaviours;
using AO.Players;
using AO.World;
using UnityEngine;

namespace AO.Npcs.AI
{
    public sealed class PetAI : NpcAIBase
    {
        private const float MAX_DISTANCE_FROM_PLAYER = 100f;
        private const int MAX_REQUESTS_TO_OWNER = 5;
        
        private bool isPathToOwnerRequested;
        private Player owner;

        public override void Init(Npc npc)
        {
            base.Init(npc);

            owner = npc.PetOwner;
            
            TargetingBehaviour = gameObject.AddComponent<PetTargetingBehaviour>();
            TargetingBehaviour.OnTargetMoved = OnTargetMoved;
            TargetingBehaviour.OnTargetFound = OnTargetFound;
            TargetingBehaviour.OnCurrentTargetInvalidated = OnTargetInvalidated;

            AttackingBehaviour = gameObject.AddComponent<PetAttackingBehaviour>();
        }

        public void TrySetNewTarget(INpcAITarget target, bool overrideCurrentTarget = false)
        {
            if (!ThisNpc.Info.CanBeManualPetTarget)
            {
                PacketSender.SendMultiMessage(owner.Id, MultiMessage.PetIgnoreCommand);
                return;
            }

            ((PetTargetingBehaviour)TargetingBehaviour).SetTarget(target, overrideCurrentTarget);
        }

        public void StopAttacking()
        {
            TargetingBehaviour.InvalidateTarget();
            ResetAI();
        }

        public void Dismiss()
        {
            ThisNpc.Despawn();
        }

        protected override void OnIdle()
        {
            Vector2 distanceToOwner = CurrentTile.Position - owner.CurrentTile.Position;

            if (owner.Flags.IsSailing && distanceToOwner.sqrMagnitude >= MAX_DISTANCE_FROM_PLAYER)
            {
                Dismiss();
                return;
            }

            if (!isPathToOwnerRequested && distanceToOwner.sqrMagnitude >= 9f)
                StartCoroutine(BeginPathToOwnerRequest());
        }

        protected override void OnTriggerEntered(Trigger trigger) { }

        protected override void OnMapEntered(Map map)
        {
            if (map.ZoneType == ZoneType.SafeZone)
                Dismiss();
        }

        protected override bool OnPathAdvanced()
        {
            // If the state is Idle it means the npc is following the player
            if (State == AIState.Idle)
                return true;
            
            // If the pet moved too far away from the owner invalidate the target and stop chasing
            if ((CurrentTile.Position - owner.CurrentTile.Position).sqrMagnitude >= MAX_DISTANCE_FROM_PLAYER)
            {
                ResetAI();
                return false;
            }

            return true;
        }

        protected override void OnPathNotFound()
        {
            // If the path that couldn't be found was a path to the owner ignore it
            if (State == AIState.Idle)
                return;

            State = AIState.Attacking;
        }

        private IEnumerator BeginPathToOwnerRequest()
        {
            isPathToOwnerRequested = true;

            int requestCount = 0;
            bool foundPath = false;
            while (requestCount < MAX_REQUESTS_TO_OWNER)
            {
                requestCount++;
                
                // Request a path and wait for 1 second
                RequestPathToTarget(owner);

                while (IsPathRequested)
                    yield return new WaitForFixedUpdate();
                
                // If it's following a path it was successful break
                if (IsFollowingPath)
                {
                    foundPath = true;
                    break;
                }
                
                // Wait one second before requesting again
                yield return new WaitForSeconds(1);
            }
            
            if (!foundPath)
                TeleportToOwner();
            
            isPathToOwnerRequested = false;
        }

        private void TeleportToOwner()
        {
            if (WorldMap.FindEmptyTileForNpc(ThisNpc, owner.CurrentTile.Position, out Tile newTile))
            {
                transform.position = newTile.Position;
                UpdateTile(newTile, CurrentTile);
                return;
            }
            
            Dismiss();
        }
    }
}