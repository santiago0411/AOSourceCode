namespace AOClient.Core.Utils
{
    public struct Layer
    {
        public const int DEFAULT = 0;
        public const int WATER = 4;
        public const int PLAYER = 8;
        public const int FOREGROUND = 9;
        public const int WORLD_ITEM = 10;
        public const int BACKGROUND = 11;
        public const int NPC = 12;
        public const int OBSTACLES = 13;
        public const int MAP = 16;
        public const int ROOF = 17;
        
        public readonly string Name;
        public readonly int Id;

        private Layer(string name, int id)
        {
            Name = name;
            Id = id;
        }

        public static readonly Layer Default = new("Default", 0);
        public static readonly Layer Water = new("Water", 4);
        public static readonly Layer Player = new("Player", 8);
        public static readonly Layer Foreground = new("Foreground", 9);
        public static readonly Layer WorldItem = new("WorldItem", 10);
        public static readonly Layer Background = new("Background", 11);
        public static readonly Layer Npc = new("Npc", 12);
        public static readonly Layer Obstacles = new("Obstacles", 13);
        public static readonly Layer Map = new("Map", 16);
        public static readonly Layer Roof = new("Roof", 17);
    }
}