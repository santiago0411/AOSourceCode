using System;
using System.Collections.Generic;
using AO.Core.Ids;

namespace AOClient.Core.Utils
{
    public sealed class Mail
    {
        public uint Id;
        public string SenderName;
        public string Subject;
        public string Body;
        public TimeSpan ExpiresIn;
        public DateTime ExpirationDate;
        public readonly Dictionary<ItemId, uint> Items = new();
    }
}