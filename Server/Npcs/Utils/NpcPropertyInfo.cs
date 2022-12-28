using AO.Core.Ids;
using JetBrains.Annotations;

namespace AO.Npcs.Utils
{
    public readonly struct NpcPropertyInfo
    {
        [UsedImplicitly]
        public readonly NpcId NpcId;
        [UsedImplicitly]
        public readonly NpcProperty Property;
        [UsedImplicitly]
        public readonly float Value;

        public void Deconstruct(out NpcProperty property, out float value)
        {
            property = Property;
            value = Value;
        }
    }
}