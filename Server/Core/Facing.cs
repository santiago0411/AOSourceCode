using UnityEngine;
using AO.Core.Utils;

namespace AO.Core
{ 
    public readonly struct Facing
    {
        public static readonly Facing Up = new(Vector2.up, Heading.North, 0);
        public static readonly Facing Right = new(Vector2.right, Heading.East, 1);
        public static readonly Facing Down = new(Vector2.down, Heading.South, 2);
        public static readonly Facing Left = new(Vector2.left, Heading.West, 3);

        public readonly Vector2 Direction;
        public readonly Heading Heading;
        public readonly int TileNeighbourIndex;
        
        private Facing(Vector2 direction, Heading heading, int index)
        {
            Direction = direction;
            Heading = heading;
            TileNeighbourIndex = index;
        }
    }
}