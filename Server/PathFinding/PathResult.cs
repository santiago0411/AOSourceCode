using AO.World;

namespace AO.PathFinding
{
    public readonly struct PathResult
    {
        public readonly Tile[] Path;
        public readonly bool Success;

        public PathResult(Tile[] path, bool success)
        {
            Path = path;
            Success = success;
        }
    }
}
