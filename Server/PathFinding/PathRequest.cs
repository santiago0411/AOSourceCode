using System;
using AO.World;

namespace AO.PathFinding
{
    public sealed class PathRequest
    {
        public Tile PathStart;
        public Tile PathEnd;
        public Action<bool, Tile[]> OnPathFindingDone;
        
        public readonly PathRequestOptions Options;
        
        public PathRequest(PathRequestOptions options)
        {
            PathStart = null;
            PathEnd = null;
            Options = options;
        }
    }
}
