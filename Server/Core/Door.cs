using System.Collections.Generic;
using UnityEngine;
using AO.World;
using AO.Core.Utils;
using AO.Network;
using AO.Players;

namespace AO.Core
{
    public class Door : MonoBehaviour
    {
        public Vector2 Position => transform.parent.position;
        public bool State => closed;
        
        [SerializeField] private string id = string.Empty;
        [SerializeField] private bool locked;
        [SerializeField] private BoxCollider2D positionCollider;

        private int idHash;
        private Map map;
        private bool closed = true;
        private List<Tile> coveringTiles;

        private void Start()
        {
            if (id == string.Empty)
                locked = false;
            else
                idHash = id.GetHashCode();

            var transformPosition = transform.position;
            map = Physics2D.OverlapBox(transformPosition, Vector2.one, 0f, LayerMask.GetMask(Layer.Map.Name)).GetComponent<Map>();
            map.Doors.Add(this);

            coveringTiles = new List<Tile>();

            //Recalculate bounds in runtime based on the real position the object has and not the prefab
            var bounds = new Bounds((Vector2)transformPosition + positionCollider.offset, positionCollider.size);

            var min = new Vector2Int(Mathf.CeilToInt(bounds.min.x), Mathf.CeilToInt(bounds.min.y));
            var max = new Vector2Int(Mathf.FloorToInt(bounds.max.x), Mathf.FloorToInt(bounds.max.y));

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    var position = new Vector2(x, y);

                    if (WorldMap.Tiles.TryGetValue(position, out var tile))
                    {
                        tile.IsBlocked = true;
                        coveringTiles.Add(tile);
                    }
                }
            }

            Transform child = transform.Find("Extra");
            if (child)
                Destroy(child.gameObject);
        }

        public void UseKey(int keyIndex)
        {
            if (idHash == keyIndex)
                locked = !locked;
        }

        public void DoubleClick(Vector2 playerPosition)
        {
            if ((playerPosition - (Vector2)transform.position).magnitude > 2.5f)
                return;

            if (!locked)
            {
                closed = !closed;
                BlockUnblock();
                PacketSender.DoorState(transform.parent.position, closed, map);
            }
        }

        private void BlockUnblock()
        {
            foreach (var tile in coveringTiles)
                tile.IsBlocked = closed;
        }
    }
}