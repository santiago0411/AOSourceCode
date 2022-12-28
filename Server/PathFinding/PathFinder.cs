using System;
using System.Collections.Generic;
using AO.World;
using UnityEngine;

namespace AO.PathFinding
{
    public class PathFinder : MonoBehaviour
    {
        private static PathFinder instance;
        private static readonly Heap<Tile> openSet = new();
        private static readonly HashSet<Tile> closedSet = new();
        
        private readonly Queue<PathRequest> requestsToExecute = new();

        private void Awake()
        {
            if (instance is null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else if (instance != this)
            {
                gameObject.SetActive(false);
                Destroy(this);
            }
        }

        private void Update()
        {
            if (requestsToExecute.Count == 0)
                return;
            
            var request = requestsToExecute.Dequeue();
            var result = FindPath(request);
            request.OnPathFindingDone(result.Success, result.Path);
        }

        public static void RequestPath(PathRequest request)
        {
            instance.requestsToExecute.Enqueue(request);
        }
        
        private static PathResult FindPath(PathRequest request)
        {
            var waypoints = Array.Empty<Tile>();
            bool success = false;

            openSet.Clear();
            closedSet.Clear();

            openSet.Add(request.PathStart);

            const float timeout = 0.01f;
            float startTime = Time.realtimeSinceStartup;
            
            while (openSet.Count > 0)
            {
                Tile currentTile = openSet.RemoveFirst();
                closedSet.Add(currentTile);
                
                if (currentTile == request.PathEnd)
                {
                    success = true;
                    break;
                }
                
                foreach (Tile neighbour in currentTile.Neighbours)
                {
                    if (neighbour is null || closedSet.Contains(neighbour) || !request.Options.IsValidTile(neighbour))
                        continue;

                    int newMovementCostToNeighbour = currentTile.GCost + GetDistance(currentTile, neighbour);

                    if (newMovementCostToNeighbour >= neighbour.GCost && openSet.Contains(neighbour))
                        continue;
                    
                    neighbour.GCost = newMovementCostToNeighbour;
                    neighbour.HCost = GetDistance(neighbour, request.PathEnd);
                    neighbour.Parent = currentTile;

                    if (!openSet.Contains(neighbour))
                    {
                        // Out of memory
                        if (!openSet.Add(neighbour))
                            break;
                    }
                    else
                    {
                        openSet.UpdateItem(neighbour);
                    }
                }

                float timeElapsed = Time.realtimeSinceStartup - startTime;
                // Timeout 
                if (timeElapsed >= timeout)
                    break;
            }

            if (success)
                waypoints = RetracePath(request.PathStart, request.PathEnd);

            return new PathResult(waypoints, success);
        }
        

        private static Tile[] RetracePath(Tile startTile, Tile targetTile)
        {
            var path = new List<Tile>();
            Tile currentTile = targetTile;
            
            while (currentTile != startTile)
            {
                currentTile.GCost = 0;
                currentTile.HCost = 0;

                path.Add(currentTile);
                var lastTile = currentTile;
                currentTile = lastTile.Parent;
                lastTile.Parent = null;
            }

            path.Reverse();
            return path.ToArray();
        }

        private static int GetDistance(Tile tileA, Tile tileB)
        {
            // If the tile is blocked by a player or npc return a high cost so it won't be chosen
            if (tileB.Player || tileB.Npc) return 100;

            return (int)Math.Abs(tileA.Position.x - tileB.Position.x) + (int)Math.Abs(tileA.Position.y - tileB.Position.y);
        }
    }
}
