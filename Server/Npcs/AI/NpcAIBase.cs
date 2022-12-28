using System;
using System.Collections;
using AO.Core;
using AO.Core.Utils;
using AO.Network;
using AO.Npcs.AI.Behaviours;
using AO.PathFinding;
using AO.Players;
using AO.World;
using UnityEngine;

namespace AO.Npcs.AI
{
    public abstract class NpcAIBase : MonoBehaviour
    {
        public Tile CurrentTile { get; private set; }
        public Vector2 FacingDirection
        {
            get => facingDirection;
            set
            {
                facingDirection = value;
                PacketSender.NpcFacing(ThisNpc, facingDirection, NearbyPlayers);
            }
        }

        public readonly CustomUniqueList<Player> NearbyPlayers = new();

        protected enum AIState
        {
            Idle,
            Patrolling,
            Chasing,
            Attacking,
            Leashing
        }

        protected AIState State;
        protected bool IsFollowingPath;
        protected bool IsPathRequested;
        protected PathRequest PathRequest;
        
        private bool isTargetReachable;
        private Vector2 facingDirection;
        private Vector2 targetOriginalPosition;

        // Readonly //
        protected Npc ThisNpc { get; private set; }
        protected ZoneType SpawnedZoneType { get; private set; }
        protected PatrollingBehaviourBase PatrollingBehaviour;
        protected TargetingBehaviourBase TargetingBehaviour;
        protected AttackingBehaviourBase AttackingBehaviour;

        protected abstract void OnIdle();
        protected abstract void OnTriggerEntered(Trigger trigger);
        protected abstract void OnMapEntered(Map map);
        protected abstract bool OnPathAdvanced();
        protected abstract void OnPathNotFound();
        
        public virtual void Init(Npc npc)
        {
            ThisNpc = npc;
            CurrentTile = ThisNpc.StartingTile;
            SpawnedZoneType = ThisNpc.CurrentMap.ZoneType;
            PathRequest = new PathRequest(new PathRequestOptions(ThisNpc.Info.WalksOnWater, ThisNpc.Info.WalksOnGround));
        }

        private void OnDestroy()
        {
            if (PatrollingBehaviour is not null)
                Destroy(PatrollingBehaviour);

            if (TargetingBehaviour is not null)
                Destroy(TargetingBehaviour);

            if (AttackingBehaviour is not null)
                Destroy(AttackingBehaviour);

            CurrentTile.Npc = null;
        }

        private void FixedUpdate()
        {
            CheckOwnership();
            UpdateParalysisState();

            if (!ThisNpc.Flags.IsParalyzed)
            {
                switch (State)
                {
                    case AIState.Idle:
                        OnIdle();
                        break;
                    case AIState.Patrolling:
                        PatrollingBehaviour?.Patrol();
                        break;
                    case AIState.Chasing:
                        ChaseTarget();
                        AttackingBehaviour.TryCastingSpell(TargetingBehaviour.CurrentTarget);
                        break;
                    case AIState.Attacking:
                        AttackTarget();
                        break;
                }
            }

            transform.position = Vector2.MoveTowards(transform.position, CurrentTile.Position, Constants.DEFAULT_NPC_MOVE_SPEED * Time.fixedDeltaTime);
            PacketSender.NpcPosition(ThisNpc, NearbyPlayers);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            switch (collision.gameObject.layer)
            {
                case Layer.PLAYER:
                    OnPlayerEnteredVisionRange(collision.GetComponent<Player>());
                    break;
                case Layer.TRIGGER:
                    OnTriggerEntered(collision.GetComponent<Trigger>());
                    break;
                case Layer.MAP:
                    OnMapEntered(collision.GetComponent<Map>());
                    break;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer == Layer.PLAYER)
                OnPlayerExitVisionRange(collision.GetComponent<Player>());
        }
        
        protected void OnPatrolSuccessful(Tile newTile)
        {
            UpdateTile(newTile, CurrentTile);
        }
        
        protected void OnTargetFound()
        {
            isTargetReachable = true;
            State = AIState.Chasing;
        }
        
        protected void OnTargetInvalidated()
        {
            if (State != AIState.Leashing)
                ResetAI();
        }

        protected void OnTargetMoved(INpcAITarget currentTarget)
        {
            isTargetReachable = true;
            Vector2 currentTargetPosition = currentTarget.CurrentTile.Position;
            bool targetMovedTooFarAway = (targetOriginalPosition - currentTargetPosition).sqrMagnitude > 4f;
            bool canNpcMove = !ThisNpc.Flags.IsImmobilized && !ThisNpc.Flags.IsParalyzed;

            if (targetMovedTooFarAway && canNpcMove)
                State = AIState.Chasing;
        }
        
        protected void ResetAI()
        {
            StopAllCoroutines();
            TargetingBehaviour.InvalidateTarget();
            ThisNpc.Flags.AttackedFirstBy = null;
            IsFollowingPath = false;
            IsPathRequested = false;
            
            State = AIState.Idle;
            TargetingBehaviour.TryFindNewTarget();
        }
        
        protected void UpdateTile(Tile newTile, Tile lastTile)
        {
            // Tile must only be updated if the npc is not leashing
            if (State != AIState.Leashing)
            {
                lastTile.Npc = null;
                newTile.Npc = ThisNpc;
            }

            CurrentTile = newTile;
            FacingDirection = CurrentTile.Position - lastTile.Position;

            if (newTile.ParentMap != lastTile.ParentMap && newTile.ParentMap)
                transform.SetParent(newTile.ParentMap.transform);
            
            ThisNpc.RaiseNpcMoved();
        }

        private void ChaseTarget()
        {
            if (!TargetingBehaviour.IsCurrentTargetValid())
            {
                ResetAI();
                return;
            }
            
            // If there is no path to target available forcefully set the state to attack
            if (!isTargetReachable)
            {
                State = AIState.Attacking;
                return;
            }
            
            // Do NOT use CurrentTile.Position here because it could be in the future and the npc will attack being 1 pos behind
            Vector2 distanceToTarget = TargetingBehaviour.CurrentTarget.CurrentTile.Position - (Vector2)transform.position;
            
            // If npc is next to the target set state to attacking and break the following path coroutine
            if (distanceToTarget.sqrMagnitude <= 1f)
            {
                State = AIState.Attacking;
                IsFollowingPath = false;
                return;
            }
            
            // Otherwise request a path to the target
            RequestPathToTarget(TargetingBehaviour.CurrentTarget);
        }
        
        private void AttackTarget()
        {
            if (!TargetingBehaviour.IsCurrentTargetValid())
            {
                ResetAI();
                return;
            }
            
            AttackingBehaviour.TryCastingSpell(TargetingBehaviour.CurrentTarget);
            
            // If it failed to attack set the state to chasing
            if (!AttackingBehaviour.TryAttacking(TargetingBehaviour.CurrentTarget))
                State = AIState.Chasing;
        }

        protected void RequestPathToTarget(INpcAITarget target)
        {
            // Avoid executing path finding if one path is already requested or if we have a valid path
            if (IsFollowingPath || IsPathRequested)
                return;

            // If there is no suitable tile around the target it means we cannot get to it
            if (!TryFindClosestAdjacentPositionAvailable(target, out var closestTile))
            {
                OnPathNotFound();
                return;
            }

            targetOriginalPosition = target.CurrentTile.Position;
            StartCoroutine(BeginPathRequest(closestTile));
        }

        private IEnumerator BeginPathRequest(Tile targetTile)
        {
            IsPathRequested = true;
            
            // Await for the npc to finish moving in case it hasn't reached the new position yet
            while ((Vector2)transform.position != CurrentTile.Position)
                yield return new WaitForFixedUpdate();

            IsFollowingPath = false;
            
            PathRequest.PathStart = CurrentTile;
            PathRequest.PathEnd = targetTile;
            PathRequest.OnPathFindingDone = OnPathFindingDone;

            PathFinder.RequestPath(PathRequest);
        }

        private void OnPathFindingDone(bool success, Tile[] path)
        {
            IsPathRequested = false;
            
            if (!success)
            {
                OnPathNotFound();
                isTargetReachable = false;
                return;
            }

            PacketSender.DebugNpcPath(ThisNpc.InstanceId, path, NearbyPlayers);
            StartCoroutine(FollowPath(path));
        }

        private IEnumerator FollowPath(Tile[] path)
        {
            int pathIndex = 0;
            Tile currentPathTile = path[pathIndex];
            Tile lastTile = CurrentTile;

            IsFollowingPath = true;
            while (IsFollowingPath)
            {
                // If the tile is occupied or the npc can't move, break
                bool cantNpcMove = ThisNpc.Flags.IsImmobilized || ThisNpc.Flags.IsParalyzed;
                if (!currentPathTile.IsTileEmpty() || cantNpcMove)
                {
                    IsFollowingPath = false;
                    yield break;
                }
                
                UpdateTile(currentPathTile, lastTile);
                
                // When it reaches the waypoint position
                while ((Vector2)transform.position != CurrentTile.Position)
                    yield return new WaitForFixedUpdate();
                
                // If the npc shouldn't continue following the path break out
                if (!OnPathAdvanced())
                    yield break;
    
                pathIndex++;
    
                if (pathIndex >= path.Length)
                {
                    IsFollowingPath = false;
                    yield break;
                }
    
                // Save the last tile and update the waypoint to the next one in the path
                lastTile = currentPathTile;
                currentPathTile = path[pathIndex];
            }
        }

        /// <summary>
        /// Calculates which of the four neighboring tiles around the target is the closest to this NPC and checks whether that tile is a valid position for it.
        /// </summary>
        /// <param name="target">The target to chase.</param>
        /// <param name="closestTile">Outputs the closest tile around the target.</param>
        /// <returns>Returns whether or not the output tile is valid for this NPC.</returns>
        private bool TryFindClosestAdjacentPositionAvailable(INpcAITarget target, out Tile closestTile)
        {
            bool validTileExists = false;
            closestTile = default;

            float lastDistance = float.MaxValue;
            Span<Vector2> positions = stackalloc Vector2[] { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

            Vector2 positionOne = target.CurrentTile.Position;
            Vector2 positionTwo = CurrentTile.Position;

            foreach (var neighborPosition in positions)
            {
                Vector2 newPosition = positionOne + neighborPosition;
                float distance = (newPosition - positionTwo).sqrMagnitude;

                if (distance < lastDistance
                    && WorldMap.Tiles.TryGetValue(newPosition, out var tile)
                    && (tile.ParentMap is null || tile.ParentMap.ZoneType == SpawnedZoneType) 
                    && tile.CanNpcBeInTile(ThisNpc.Info))
                {
                    closestTile = tile;
                    validTileExists = true;
                }

                lastDistance = distance;
            }
        
            return validTileExists;
        }
        
        private void CheckOwnership()
        {
            var owner = ThisNpc.Flags.CombatOwner; 
            if (owner && Timers.PlayerLostNpcInterval(owner))
                PlayerMethods.LostNpc(owner);
        }
        
        private void UpdateParalysisState()
        {
            if ((Time.realtimeSinceStartup - ThisNpc.Flags.ParalyzedTime) >= Constants.NPC_PARALYZE_TIME)
            {
                ThisNpc.Flags.IsParalyzed = false;
                ThisNpc.Flags.IsImmobilized = false;
            }
            
            if (ThisNpc.Flags.IsImmobilized)
                State = AIState.Attacking;
        }

        private void OnPlayerEnteredVisionRange(Player player)
        {
            NearbyPlayers.Add(player);
            PacketSender.NpcRangeChanged(player.Id, ThisNpc.InstanceId, true);
        }

        private void OnPlayerExitVisionRange(Player player)
        {
            NearbyPlayers.Remove(player);
            PacketSender.NpcRangeChanged(player.Id, ThisNpc.InstanceId, false);
        }
    }
}