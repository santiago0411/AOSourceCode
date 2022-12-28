using UnityEngine;
using AO.Core;
using AO.Core.Utils;
using AO.Players;

namespace AO.World
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Map : MonoBehaviour
    {
        public string Name => mapName;
        public short Number => mapNumber;
        public ZoneType ZoneType => zoneType;
        public Bounds Boundaries { get; private set; }
        public Transform Trees { get; private set; }
        public Transform Obstacles { get; private set; }
        public Transform Roofs { get; private set; }
        public readonly CustomUniqueList<Door> Doors = new();
        public readonly CustomUniqueList<Player> PlayersInMap = new();
        
        [Header("Map Info")]
        [SerializeField] private string mapName = "Ullathorpe";
        [SerializeField] private short mapNumber = 1;
        [SerializeField] private ZoneType zoneType = ZoneType.SafeZone;

        private void Awake()
        {
            gameObject.name = mapName;
            Boundaries = GetComponent<Collider2D>().bounds;
            Trees = transform.Find("Trees").transform;
            Obstacles = transform.Find("Obstacles").transform;
            Roofs = transform.Find("Roofs").transform;
        }
    }
}
