using System.Linq;
using System.Threading.Tasks;
using AO.Core;
using AO.Npcs;
using AO.Players;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AO.World
{
    public class WorldMap : MonoBehaviour
    {
#if UNITY_EDITOR
        public static readonly System.Collections.Concurrent.ConcurrentDictionary<Vector2, Tile> Tiles = new();
#else
        public static readonly System.Collections.Generic.Dictionary<Vector2, Tile> Tiles = new();
#endif
        
        public static Tilemap ObstaclesRoofsTreesMap => instance.obstacles;
        public static Tilemap RoofsMap => instance.roofs;
        private static Tilemap BackgroundMap => instance.background; 
        private static Tilemap ForegroundMap => instance.foreground; 
        private static Tilemap WaterMap => instance.water;
        private static Tilemap BlockedPositions => instance.blockedPositions;
        
        private static WorldMap instance;
        
        [SerializeField] private Tilemap background, foreground, water, blockedPositions, obstacles, roofs;
        
        private delegate bool IsPositionValidFunc<in T>(Vector2 position, T state, out Tile tile);
        
        // Used lambdas to cache these delegates to avoid constant memory allocation from method groups + closures
        private static readonly IsPositionValidFunc<object> isValidItemTile =
            (Vector2 position, object _, out Tile tile) =>
                Tiles.TryGetValue(position, out tile) && tile.CanItemBeInTile();

        private static readonly IsPositionValidFunc<Player> isValidPlayerTile =
            (Vector2 position, Player player, out Tile tile) =>
                Tiles.TryGetValue(position, out tile) && tile.CanPlayerBeInTile(player);
        
        private static readonly IsPositionValidFunc<Npc> isValidNpcTile =
            (Vector2 position, Npc npc, out Tile tile) =>
                Tiles.TryGetValue(position, out tile) && tile.CanNpcBeInTile(npc.Info);
        
        private void Awake()
        {
            if (instance is null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                gameObject.SetActive(false);
                Destroy(this);
            }
        }

        public static void LoadTiles()
        {
            // If tiles is not empty it means they were loaded from cache
            if (Tiles.Count != 0)
                return;
            
            AoDebug.BeingTimer();
            var maps = FindObjectsOfType<Map>();

            foreach (var position in BackgroundMap.cellBounds.allPositionsWithin)
            {
                // Get the background and water tile if neither exist it's not a valid position
                var bgTile = BackgroundMap.GetTile(position);
                var waterTile = WaterMap.GetTile(position);
                if (!bgTile && !waterTile)
                    continue;
                
                // If it is a valid position check the Foreground map and the BlockedPositions map to see if this position should be blocked
                var fgTile = ForegroundMap.GetTile(position) ?? BlockedPositions.GetTile(position);
                bool blockedPosition = (bool)fgTile;
                
                // Check if it's water by checking if the water tile exists and if it's lava according to the TileType
                bool isWater = (bool)waterTile;
                bool isLava = bgTile is CustomTile { type: CustomTile.TileType.Lava };
                
                // Add one to the position to remove the Tilemap offset, this will be the final game position
                Vector2 gamePosition = position + Vector3.one;
                
                // Finally get the Map that this tile belongs to create the tile and add it to the dictionary
                var parentMap = maps.FirstOrDefault(map => map.Boundaries.Contains(gamePosition));
                var tile = new Tile(blockedPosition, isWater, isLava, gamePosition, parentMap);
                Tiles[gamePosition] = tile;
            }
            
            // Because there could be water tiles that aren't within the bounds of the background map
            // loop again through the bounds of the WaterMap
            foreach (var position in WaterMap.cellBounds.allPositionsWithin)
            {
                var waterTile = WaterMap.GetTile(position);
                if (!waterTile)
                    continue;
                
                // If this tile was already created above just continue
                Vector2 gamePosition = position + Vector3.one;
                if (Tiles.ContainsKey(gamePosition))
                    continue;
                
                // Otherwise do the same steps as above
                var fgTile = ForegroundMap.GetTile(position) ?? BlockedPositions.GetTile(position);
                bool blockedPosition = (bool)fgTile;
                
                var parentMap = maps.FirstOrDefault(map => map.Boundaries.Contains(gamePosition));
                var tile = new Tile(blockedPosition, true, false, gamePosition, parentMap);
                Tiles[gamePosition] = tile;
            }

            Parallel.ForEach(Tiles.Values, t => t.FindNeighbours());
            AoDebug.EndTimer<WorldMap>();
            new Core.Logging.LoggerAdapter(typeof(WorldMap)).Info("WorldMap start-up successful.");
        }
        
        /// <summary>Converts a map position to real world position.</summary>
        public static Vector2 MapPositionToWorldPosition(Map map, Vector2 gamePosition)
        {
            float x = map.Boundaries.min.x + (gamePosition.x - 0.5f);
            float y = map.Boundaries.max.y - (gamePosition.y + 0.5f);
            return new Vector2(x, y);
        }

        /// <summary>Converts a real world position to a map position.</summary>
        public Vector2 WorldPositionToMapPosition(Map map, Vector2 mapPosition)
        {
            float x = mapPosition.x - map.Boundaries.min.x;
            float y = map.Boundaries.max.y - mapPosition.y;
            return new Vector2(x + 0.5f, y - 0.5f);
        }

        public static Npc GetNpcAtPosition(Vector2 position)
        {
            Tiles.TryGetValue(position, out Tile tile);
            return tile?.Npc;
        }

        public static Player GetPlayerAtPosition(Vector2 position)
        {
            Tiles.TryGetValue(position, out Tile tile);
            return tile?.Player;
        }

        public static Items.WorldItem GetWorldItemAtPosition(Vector2 position)
        {
            Tiles.TryGetValue(position, out Tile tile);
            return tile?.WorldItem;
        }

        public static bool FindEmptyTileForItem(Vector2 position, out Tile tile)
        {
            return FindPosition(position, null, isValidItemTile, out tile);
        }

        public static bool FindEmptyTileForPlayer(Player player, Vector2 position, out Tile tile)
        {
            return FindPosition(position, player, isValidPlayerTile, out tile);
        }

        public static bool FindEmptyTileForNpc(Npc npc, Vector2 position, out Tile tile)
        {
            return FindPosition(position, npc, isValidNpcTile, out tile);
        }

        private static bool FindPosition<T>(Vector2 startingPosition, T state, IsPositionValidFunc<T> isPositionValidFunc, out Tile tile)
        {
            if (isPositionValidFunc(startingPosition, state, out tile))
                return true;

            int direction = 0;
            const int size = 16;
            int chainSize = 1;
            var position = startingPosition;

            for (int k = 0; k < size; k++)
            {
                for (int j = 0; j < (k < (size - 1) ? 2 : 3); j++)
                {
                    for (int i = 0; i < chainSize; i++)
                    {
                        switch (direction)
                        {
                            case 0:
                                position += Vector2.up;
                                break;
                            case 1:
                                position += Vector2.right;
                                break;
                            case 2:
                                position += Vector2.down;
                                break;
                            case 3:
                                position += Vector2.left;
                                break;
                        }

                        if (isPositionValidFunc(position, state, out tile))
                            return true;
                    }

                    direction = (direction + 1) % 4;
                }

                chainSize++;
            }

            return false;
        }
    }
}