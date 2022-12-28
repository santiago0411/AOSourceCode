using AO.World;
using JetBrains.Annotations;

namespace AO.PathFinding
{
    public sealed class PathRequestOptions
    {
        private readonly bool canWalkOnWater;
        private readonly bool canWalkOnGround;

        public PathRequestOptions(bool canWalkOnWater, bool canWalkOnGround)
        {
            this.canWalkOnWater = canWalkOnWater;
            this.canWalkOnGround = canWalkOnGround;
        }

        public bool IsValidTile([NotNull] Tile tile)
        {
            if (tile.IsBlocked || tile.InvalidNpcPosition)
                return false;

            return tile.IsWater ? canWalkOnWater : canWalkOnGround;
        }
    }
}
