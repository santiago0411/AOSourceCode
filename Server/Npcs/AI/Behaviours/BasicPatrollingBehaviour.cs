using System;
using AO.Core.Utils;
using AO.World;
using UnityEngine;

namespace AO.Npcs.AI.Behaviours
{
    public sealed class BasicPatrollingBehaviour : PatrollingBehaviourBase
    {
        public override Action<Tile> OnPatrolSuccessful { get; set; }
        
        private Npc thisNpc;
        private float lastPatrolTime;
        
        private void Start()
        {
            thisNpc = GetComponent<Npc>();
        }

        public override void Patrol()
        {
            if (thisNpc.Flags.IsImmobilized) 
                return;

            if ((Time.realtimeSinceStartup - lastPatrolTime) > 4f)
                DoPatrol();
        }

        private void DoPatrol()
        {
            if (ExtensionMethods.RandomNumber(1, 10) == 7)
            {
                int randomDirection = ExtensionMethods.RandomNumber(0, 3);
                Vector2 tentativePos = thisNpc.CurrentTile.Position;

                switch (randomDirection)
                {
                    case 0:
                        tentativePos += Vector2.up;
                        break;
                    case 1:
                        tentativePos += Vector2.down;
                        break;
                    case 2:
                        tentativePos += Vector2.left;
                        break;
                    case 3:
                        tentativePos += Vector2.right;
                        break;
                }
                
                if (thisNpc.CurrentTile.CanNpcMoveToNeighbourTile(thisNpc, tentativePos, out var newTile))
                    OnPatrolSuccessful(newTile);
            }

            lastPatrolTime = Time.realtimeSinceStartup;
        }
    }
}