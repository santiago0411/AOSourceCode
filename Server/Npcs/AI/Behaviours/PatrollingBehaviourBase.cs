using System;
using AO.World;
using UnityEngine;

namespace AO.Npcs.AI.Behaviours
{
    public abstract class PatrollingBehaviourBase : MonoBehaviour
    {
        public abstract Action<Tile> OnPatrolSuccessful { get; set; }
        public abstract void Patrol();
    }
}