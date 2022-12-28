namespace AO.Players.Talents
{
    public sealed class TalentTreeNode
    {
        public readonly byte TalentId;
        public byte Points { get; private set; }
        public bool Acquired => Points > 0;

        private readonly byte maxPoints;

        public TalentTreeNode(byte talentId, byte currentPoints, byte maxPoints)
        {
            TalentId = talentId;
            Points = currentPoints;
            this.maxPoints = maxPoints;
        }

        public bool SkillUp()
        {
            if (Points < maxPoints)
            {
                Points++;
                return true;
            }

            return false;
        }
    }
}