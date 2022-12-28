using UnityEngine;
using UnityEngine.Tilemaps;

namespace AOClient.Core.Utils
{
    [CreateAssetMenu(fileName = "CustomTile", menuName = "2D/Tiles/Custom AO Tile")]
    public class CustomTile : Tile
    {
        public TileType Type => type;
        [SerializeField] private TileType type = TileType.Grass;
        public enum TileType { Dirt, Grass, Snow, Water, Stone, Wood, Lava, Dungeon }

        public static Vector3Int WorldPositionToTile(Vector3 position)
        {
            return new Vector3Int(((int)position.x) - 1, ((int)position.y) - 1, 0);
        }
    }
}
