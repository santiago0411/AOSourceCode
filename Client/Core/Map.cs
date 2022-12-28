using UnityEngine;

namespace AOClient.Core
{
    public class Map : MonoBehaviour
    {
        [SerializeField] private string mapName = "Ullathorpe";
        [SerializeField] private short mapNumber = 1;

        public string Name => mapName;
        public short Number => mapNumber;
        public Bounds Boundaries { get; private set; }
        public Transform Trees { get; private set; }
        public Transform Obstacles { get; private set; }
        public Transform Roofs { get; private set; }

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
