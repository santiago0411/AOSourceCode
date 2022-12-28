namespace AO.Core.Utils
{
    public readonly struct Layer
    {
        public const int DEFAULT = 0;
        public const int WATER = 4;
        public const int NPC_VISION_RANGE = 6;
        public const int PLAYER_VISION_RANGE = 7;
        public const int PLAYER = 8;
        public const int FOREGROUND = 9;
        public const int WORLD_ITEM = 10;
        public const int BACKGROUND = 11;
        public const int NPC = 12;
        public const int OBSTACLES = 13;
        public const int TRIGGER = 14;
        public const int SPAWNER = 15;
        public const int MAP = 16;
        
        public readonly string Name;
        public readonly int Id;
        
        private Layer(string name, int id)
        {
            Name = name; 
            Id = id;
        }

        public static readonly Layer Default = new("Default", DEFAULT);
        public static readonly Layer Water = new("Water", WATER);
        public static readonly Layer NpcVisionRange = new("NpcVisionRange", NPC_VISION_RANGE);
        public static readonly Layer PlayerVisionRange  = new("PlayerVisionRange", PLAYER_VISION_RANGE);
        public static readonly Layer Player = new("Player", PLAYER);
        public static readonly Layer Foreground = new("Foreground", FOREGROUND);
        public static readonly Layer WorldItem = new("WorldItem", WORLD_ITEM);
        public static readonly Layer Background = new("Background", BACKGROUND);
        public static readonly Layer Npc = new("Npc", NPC);
        public static readonly Layer Obstacles = new("Obstacles", OBSTACLES);
        public static readonly Layer Trigger = new("Trigger", TRIGGER);
        public static readonly Layer Spawner = new("Spawner", SPAWNER);
        public static readonly Layer Map = new("Map", MAP);
    }
}