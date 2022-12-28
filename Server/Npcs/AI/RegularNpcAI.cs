using System.Collections;
using AO.Network;
using AO.Npcs.AI.Behaviours;
using AO.PathFinding;
using AO.World;
using UnityEngine;

namespace AO.Npcs.AI
{
    public sealed class RegularNpcAI : NpcAIBase
    {
        private const float MAX_DISTANCE_FROM_SPAWN = 225f;
        private const int MAX_REQUESTS_TO_START = 5;

        public override void Init(Npc npc)
        {
            base.Init(npc);

            switch (npc.Info.PatrollingBehaviour)
            {
                case NpcPatrollingBehaviour.Static:
                    break;
                case NpcPatrollingBehaviour.Basic:
                    PatrollingBehaviour = gameObject.AddComponent<BasicPatrollingBehaviour>();
                    PatrollingBehaviour.OnPatrolSuccessful = OnPatrolSuccessful;
                    break;
                default:
                    Core.AoDebug.Assert(false, $"Unknown patrolling behaviour {npc.Info.PatrollingBehaviour}");
                    break;
            }

            #if AO_DEBUG
            bool hasTargetingButNoAttacking = npc.Info.TargetingBehaviour != NpcTargetingBehaviour.NoTargeting &&
                                              npc.Info.AttackingBehaviour == NpcAttackingBehaviour.NoAttacking;
            Core.AoDebug.Assert(!hasTargetingButNoAttacking, $"Npc {npc.Info.Name} ({npc.Info.Id}) has a targeting behaviour but no attacking behaviour");
            
            bool hasAttackingButNoTargeting = npc.Info.AttackingBehaviour != NpcAttackingBehaviour.NoAttacking &&
                                              npc.Info.TargetingBehaviour == NpcTargetingBehaviour.NoTargeting;
            Core.AoDebug.Assert(!hasAttackingButNoTargeting, $"Npc {npc.Info.Name} ({npc.Info.Id}) has an attacking behaviour but no targeting behaviour");
            #endif
            
            switch (npc.Info.TargetingBehaviour)
            {
                case NpcTargetingBehaviour.NoTargeting:
                    break;
                case NpcTargetingBehaviour.BasicHostile:
                    TargetingBehaviour = gameObject.AddComponent<HostileTargetingBehaviour>();
                    TargetingBehaviour.OnTargetMoved = OnTargetMoved;
                    TargetingBehaviour.OnTargetFound = OnTargetFound;
                    TargetingBehaviour.OnCurrentTargetInvalidated = OnTargetInvalidated;
                    break;
                default:
                    Core.AoDebug.Assert(false, $"Unknown targeting behaviour {npc.Info.TargetingBehaviour}");
                    break;
            }

            switch (npc.Info.AttackingBehaviour)
            {
                case NpcAttackingBehaviour.NoAttacking:
                    break;
                case NpcAttackingBehaviour.BasicHostile:
                    AttackingBehaviour = gameObject.AddComponent<HostileAttackingBehaviour>();
                    break;
                default:
                    Core.AoDebug.Assert(false, $"Unknown attacking behaviour {npc.Info.AttackingBehaviour}");
                    break;
            }
        }

        protected override void OnIdle()
        {
            if (PatrollingBehaviour is not null)
                State = AIState.Patrolling;
        }

        protected override void OnTriggerEntered(Trigger trigger)
        {
            switch (trigger.Type)
            {
                case TriggerType.SafeZone:
                    if (SpawnedZoneType != ZoneType.SafeZone)
                        BeginLeashingState();
                    break;
                case TriggerType.UnsafeZone:
                    if (SpawnedZoneType != ZoneType.UnsafeZone)
                        BeginLeashingState();
                    break;
                case TriggerType.Arena:
                    if (SpawnedZoneType != ZoneType.Arena)
                        BeginLeashingState();
                    break;
            }
        }

        protected override void OnMapEntered(Map map)
        {
            if (map.ZoneType != SpawnedZoneType)
                BeginLeashingState();
        }
        
        protected override bool OnPathAdvanced()
        {
            // If the npc is outside their permitted area
            if ((CurrentTile.Position - ThisNpc.StartingTile.Position).sqrMagnitude >= MAX_DISTANCE_FROM_SPAWN)
            {
                BeginLeashingState();
                return false;
            }

            return true;
        }

        protected override void OnPathNotFound()
        {
            State = AIState.Attacking;
        }

        private void BeginLeashingState()
        {
            ThisNpc.Attackable = false;
            ThisNpc.Flags.IsParalyzed = false;
            ThisNpc.Flags.IsImmobilized = false;
            State = AIState.Leashing;
            StartCoroutine(BeginPathRequestToStart());
        }
        
        private void EndLeashingState(bool foundPathToStart)
        {
            ThisNpc.Attackable = true;
            CurrentTile.Npc = ThisNpc;
            
            // If a path wasn't found teleport the npc to the starting tile
            if (!foundPathToStart)
            {
                if (!WorldMap.FindEmptyTileForNpc(ThisNpc, ThisNpc.StartingTile.Position, out Tile startingTile))
                {
                    // If an empty position back to the start couldn't be found just despawn the npc
                    // The chances of this ever happening are very very low
                    ThisNpc.Despawn();
                    return;
                }
                
                transform.position = startingTile.Position;
                UpdateTile(startingTile, CurrentTile);
            }
            
            ResetAI();
        }
        
        private IEnumerator BeginPathRequestToStart()
        {
            IsPathRequested = true;
            
            // Await for the npc to finish moving in case it hasn't reached the new position yet
            while ((Vector2)transform.position != CurrentTile.Position)
                yield return new WaitForFixedUpdate();
            
            IsFollowingPath = false;
            
            int requestCount = 0;
            bool foundPath = false;
            while (requestCount < MAX_REQUESTS_TO_START)
            {
                requestCount++;
                if (!WorldMap.FindEmptyTileForNpc(ThisNpc, ThisNpc.StartingTile.Position, out Tile startingTile))
                {
                    // If an empty position back to the start couldn't be found just despawn the npc
                    // The chances of this ever happening are very very low
                    ThisNpc.Despawn();
                    yield break;
                }
                
                PathRequest.PathStart = CurrentTile;
                PathRequest.PathEnd = startingTile;
                PathRequest.OnPathFindingDone = OnPathFindingToStartDone;
                
                // Reset to true in case we are in another loop iteration
                // OnPathFindToStartDone will set it to false
                IsPathRequested = true;
                PathFinder.RequestPath(PathRequest);

                // Await until path finding is done
                while (IsPathRequested)
                    yield return new WaitForFixedUpdate();
                
                // If it's following a path it means FindPath was successful
                if (IsFollowingPath)
                {
                    foundPath = true;
                    break;
                }
                
                // Wait one second before requesting again
                yield return new WaitForSeconds(1);
            }

            if (!foundPath)
                EndLeashingState(false);   
            
            IsPathRequested = false;
        }
        
        private void OnPathFindingToStartDone(bool success, Tile[] path)
        {
            if (success)
            {
                PacketSender.DebugNpcPath(ThisNpc.InstanceId, path, NearbyPlayers);
                StartCoroutine(FollowPathLeashing(path));
            }
            
            IsPathRequested = false;
        }
        
        private IEnumerator FollowPathLeashing(Tile[] path)
        {
            int pathIndex = 0;
            Tile currentPathTile = path[pathIndex];
            Tile lastTile = CurrentTile;

            // Manually update the last tile because being in a leashing state UpdateTile won't update the tile's npc
            if (lastTile.Npc == ThisNpc)
                lastTile.Npc = null;

            IsFollowingPath = true;
            while (IsFollowingPath)
            {
                // If the tile is occupied can't follow path
                if (!currentPathTile.IsTileEmpty())
                {
                    // Only request a new path to start if the last tile is further away than 5 positions
                    if ((path[^1].Position - lastTile.Position).sqrMagnitude > 25f)
                        StartCoroutine(BeginPathRequestToStart());

                    IsFollowingPath = false;
                    yield break;
                }
            
                UpdateTile(currentPathTile, lastTile);
                
                // When it reaches the waypoint position
                while ((Vector2)transform.position != CurrentTile.Position)
                    yield return new WaitForFixedUpdate();
                
                pathIndex++;

                if (pathIndex >= path.Length)
                {
                    IsFollowingPath = false;
                    EndLeashingState(true);
                    yield break;
                }

                // Save the last tile and update the waypoint to the next one in the path
                lastTile = currentPathTile;
                currentPathTile = path[pathIndex];
            }
        }
    }
}