namespace AO.Players.Utils
{
    public readonly struct LevelUpStatsIncrease
    {
        public readonly int HpIncrease;
        public readonly int ManaIncrease;
        public readonly int StaminaIncrease;
        public readonly int HitIncrease;

        public LevelUpStatsIncrease(int hpIncrease, int manaIncrease, int stamIncrease, int hitIncrease)
        {
            HpIncrease = hpIncrease;
            ManaIncrease = manaIncrease;
            StaminaIncrease = stamIncrease;
            HitIncrease = hitIncrease;
        }
    }
}
