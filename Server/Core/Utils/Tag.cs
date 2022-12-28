using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AO.Core.Utils
{
    public readonly struct Tag
    {
        public readonly byte Id;
        public readonly string Name;

        private Tag(string name, byte id)
        {
            Name = name; Id = id;
        }
        
        public static readonly Tag Tree = new("Tree", 0);
        public static readonly Tag IronDeposit = new("IronDeposit", 1);
        public static readonly Tag SilverDeposit = new("SilverDeposit", 2);
        public static readonly Tag GoldDeposit = new("GoldDeposit", 3);
        public static readonly Tag ElficWood = new("ElficTree", 4);
        public static readonly Tag Forge = new("Forge", 5);
        public static readonly Tag Anvil = new("Anvil", 6);
        public static readonly Tag Door = new("Door", 7);
        public static readonly Tag Roof = new("Roof", 8);
        public static readonly Tag Untagged = new("Untagged", byte.MaxValue);
        
        public static readonly ReadOnlyDictionary<string, byte> TagsByName = new(new Dictionary<string, byte>
            {
                { "Tree", 			0 },
                { "IronDeposit", 	1 },
                { "SilverDeposit", 	2 },
                { "GoldDeposit", 	3 },
                { "ElficTree", 		4 },
                { "Forge", 			5 },
                { "Anvil", 			6 },
                { "Door", 			7 },
                { "Roof", 			8 },
                { "Untagged", byte.MaxValue }
            }
        );
    } 
}
