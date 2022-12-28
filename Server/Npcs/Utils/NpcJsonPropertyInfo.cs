using AO.Core.Ids;
using JetBrains.Annotations;

namespace AO.Npcs.Utils
{
    public readonly struct NpcJsonPropertyInfo
    {
        [UsedImplicitly]
        public readonly NpcId NpcId;
        [UsedImplicitly]
        public readonly NpcProperty Property;
        [UsedImplicitly]
        public readonly string Value;
    }
}