namespace AO.Core.Database
{
    public enum DatabaseResultStatus
    {
        OperationFailed,
        CancellationRequested,
        Ok
    }
    
    public readonly struct DatabaseResult<T>
    {
        public readonly DatabaseResultStatus Status;
        public readonly T Item;

        public DatabaseResult(DatabaseResultStatus status, T item)
        {
            Status = status;
            Item = item;
        }

        public void Deconstruct(out DatabaseResultStatus status, out T item)
        {
            status = Status;
            item = Item;
        }
    }

    public readonly struct DatabaseResult<T1, T2>
    {
        public readonly DatabaseResultStatus Status;
        public readonly T1 Item1;
        public readonly T2 Item2;
        
        public DatabaseResult(DatabaseResultStatus status, T1 item1, T2 item2)
        {
            Status = status;
            Item1 = item1;
            Item2 = item2;
        }
        
        public void Deconstruct(out DatabaseResultStatus status, out T1 item1, out T2 item2)
        {
            status = Status;
            item1 = Item1;
            item2 = Item2;
        }
    }
}