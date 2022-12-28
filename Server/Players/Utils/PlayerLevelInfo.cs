using JetBrains.Annotations;

namespace AO.Players.Utils
{
    public readonly struct PlayerLevelInfo
    {
        [UsedImplicitly]
        public readonly byte Level;
        [UsedImplicitly]
        public readonly uint MaxXp;
        [UsedImplicitly]
        public readonly byte MaxSkill;
    }
}