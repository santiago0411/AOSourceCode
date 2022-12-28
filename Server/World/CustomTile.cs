using UnityEngine;

namespace AO.World
{
    [CreateAssetMenu(fileName = "CustomTile", menuName = "2D/Tiles/Custom AO Tile")]
    public class CustomTile : UnityEngine.Tilemaps.Tile
    {
        public TileType type = TileType.Grass;
        public enum TileType { Dirt, Grass, Snow, Water, Stone, Wood, Sand, Lava, Dungeon }
    }
}
