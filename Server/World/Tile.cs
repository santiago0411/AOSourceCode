using UnityEngine;
using AO.Npcs;
using AO.Npcs.Utils;
using AO.Players;

namespace AO.World
{
    public class Tile : PathFinding.IHeapItem<Tile>
    {
        public bool IsBlocked { get; set; }
        public bool IsWater { get; }
        public bool IsLava { get; }
        public bool InvalidNpcPosition { get; set; }
        public Vector2 Position { get; } 
        public Map ParentMap { get; }
        public Player Player { get; set; }
        public Npc Npc { get; set; }
        public Items.WorldItem WorldItem { get; set; }
        public readonly Tile[] Neighbours = new Tile[4];

        #region PathFinding
        public int GCost { get; set; }
        public int HCost { get; set; }
        private int FCost => GCost + HCost;
        public Tile Parent { get; set; }
        public int HeapIndex { get; set; }
        public bool Equals(Tile other)
        {
            return this == other;
        }

        #endregion

        public Tile(bool blocked, bool water, bool lava, Vector2 position, Map map)
        {
            IsBlocked = blocked;
            IsWater = water;
            IsLava = lava;
            Position = position;
            ParentMap = map;
        }

        public int CompareTo(Tile tileToCompare)
        {
            int compare = FCost.CompareTo(tileToCompare.FCost);

            if (compare == 0)
                compare = HCost.CompareTo(tileToCompare.HCost);

            return -compare;
        }

        public void FindNeighbours()
        {
            // NORTH
            Vector2 northPosition = Position + Vector2.up;
            WorldMap.Tiles.TryGetValue(northPosition, out Tile northTile);
            Neighbours[0] = northTile;

            // EAST
            Vector2 eastPosition = Position + Vector2.right;
            WorldMap.Tiles.TryGetValue(eastPosition, out Tile eastTile);
            Neighbours[1] = eastTile;

            // SOUTH
            Vector2 southPosition = Position + Vector2.down;
            WorldMap.Tiles.TryGetValue(southPosition, out Tile southTile);
            Neighbours[2] = southTile;

            // WEST
            Vector2 westPosition = Position + Vector2.left;
            WorldMap.Tiles.TryGetValue(westPosition, out Tile westTile);
            Neighbours[3] = westTile;
        }

        private bool TryGetNeighbour(Vector2 position, out Tile neighbour)
        {
            foreach (var nb in Neighbours)
                if (nb is not null && nb.Position == position)
                {
                    neighbour = nb;
                    return true;
                }

            neighbour = null;
            return false;
        }

        public bool CanItemBeInTile()
        {
            return !IsBlocked && !IsWater && !WorldItem;
        }

        public bool CanPlayerBeInTile(Player player)
        {
            if (IsBlocked) 
                return false;

            if (Player || Npc)
                return false;

            return !IsWater || player.HasBoatEquipped;
        }
        
        public bool CanNpcBeInTile(NpcInfo npcInfo, bool ignorePlayer = false)
        {
            if (IsBlocked || InvalidNpcPosition)
                return false;
            
            if (Npc || (Player && !ignorePlayer))
                return false;

            return IsWater ? npcInfo.WalksOnWater : npcInfo.WalksOnGround;
        }

        public bool IsTileEmpty()
        {
            return Npc is null && Player is null;
        }

        public bool CanPlayerMoveToNeighbourTile(Player player, Vector2 position, out Tile tile)
        {
            return TryGetNeighbour(position, out tile) && tile.CanPlayerBeInTile(player);
        }

        public bool CanNpcMoveToNeighbourTile(Npc npc, Vector2 position, out Tile tile)
        {
            return TryGetNeighbour(position, out tile) && tile.CanNpcBeInTile(npc.Info);
        }
    }
}
