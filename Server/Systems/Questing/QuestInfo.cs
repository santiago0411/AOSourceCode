using AO.Core.Ids;
using JetBrains.Annotations;
using SqlKata;

namespace AO.Systems.Questing
{
    public readonly struct QuestInfo
    {
        [UsedImplicitly]
        [Column("id")]
        public readonly QuestId Id;
        
        [UsedImplicitly] 
        [Column("repeatable")] 
        public readonly Repeatable Repeatable;

        [UsedImplicitly] 
        [Column("requirements")]
        public readonly string Requirements;

        [UsedImplicitly] 
        [Column("goals")]
        public readonly string Goals;
        [UsedImplicitly] 

        [Column("rewards")]
        public readonly string Rewards;
    }
}