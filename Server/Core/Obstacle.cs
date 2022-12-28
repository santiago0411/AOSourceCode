using System.Collections.ObjectModel;
using UnityEngine;
using AO.Core.Utils;
using AO.World;
using PacketSender = AO.Network.PacketSender;

namespace AO.Core
{ 
    public class Obstacle : MonoBehaviour
    {
        public static uint SpawnedObstacles { get; private set; }

        private static readonly ReadOnlyCollection<byte> namelessObstacles = new(new[]{ Tag.Door.Id, Tag.Roof.Id, Tag.Untagged.Id });
        
        [SerializeField] private GameObject layerSorter;
        [SerializeField] private GameObject root;

        private Door thisDoor;

        private void Start()
        {
            if (transform.parent.name == "ObstaclesTreesDoors")
                SpawnedObstacles++;
            
            thisDoor = GetComponentInChildren<Door>();
            
            Destroy(layerSorter);
            BlockPositions();
            
            var collision = Physics2D.OverlapBox(transform.position, Vector2.one, 0f, CollisionManager.MapLayerMask);
            if (!collision)
                return;
            
            var parentMap = collision.GetComponent<Map>();

            switch (tag)
            {
                case "Tree":
                case "ElficTree":
                    transform.SetParent(parentMap.Trees);
                    break;
                case "IronDeposit":
                case "SilverDeposit":
                case "GoldDeposit":
                case "Forge":
                case "Anvil":
                    transform.SetParent(parentMap.Obstacles);
                    break;
                case "Roof":
                    transform.SetParent(parentMap.Roofs);
                    Destroy(root);
                    break;
                default:
                    transform.SetParent(parentMap.Obstacles);
                    Destroy(root);
                    break;
            }
        }

        public void Click(Players.Player player, bool doubleClick)
        {
            if (doubleClick && thisDoor)
            {
                thisDoor.DoubleClick(player.CurrentTile.Position);
                return;
            }

            var tagId = Tag.TagsByName[tag];
            if (!namelessObstacles.Contains(tagId))
                PacketSender.SendMultiMessage(player.Id, MultiMessage.ObstacleClick,  stackalloc int[] {tagId});
        }

        private void BlockPositions()
        {
            var rootColliders = root.GetComponentsInChildren<BoxCollider2D>();

            foreach (var rootCollider in rootColliders)
            {
                //Recalculate bounds in runtime based on the real position the object has and not the prefab
                var bounds = new Bounds((Vector2)rootCollider.transform.position + rootCollider.offset, rootCollider.size);

                var min = new Vector2Int(Mathf.CeilToInt(bounds.min.x), Mathf.CeilToInt(bounds.min.y));
                var max = new Vector2Int(Mathf.FloorToInt(bounds.max.x), Mathf.FloorToInt(bounds.max.y));

                for (int x = min.x; x <= max.x; x++)
                {
                    for (int y = min.y; y <= max.y; y++)
                    {
                        var position = new Vector2(x, y);

                        if (WorldMap.Tiles.TryGetValue(position, out Tile tile))
                            tile.IsBlocked = true;
                    }
                }
            }
        }
    }
}