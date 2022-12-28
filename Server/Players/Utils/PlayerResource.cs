namespace AO.Players.Utils
{
    public class PlayerResource
    {
        public ushort MaxAmount { get; private set; }
        public ushort CurrentAmount { get; private set; }

        public PlayerResource(ushort maxAmount, ushort currentAmount)
        {
            MaxAmount = maxAmount;
            CurrentAmount = currentAmount;
        }

        /// <summary>Sets both the max and current values of the resource.</summary>
        public void SetResource(ushort maxAmount, ushort currentAmount)
        {
            MaxAmount = maxAmount;
            CurrentAmount = currentAmount;
        }

        /// <summary>Takes the specified amount from the resource.</summary>
        public void TakeResource(ushort amount)
        {
            CurrentAmount -= amount;
            if (CurrentAmount <= 0)
                CurrentAmount = 0;
        }

        /// <summary>Adds the specified amount to the resource.</summary>
        public void AddResource(ushort amount)
        {
            CurrentAmount += amount;
            if (CurrentAmount > MaxAmount)
                CurrentAmount = MaxAmount;
        }
    }
}
