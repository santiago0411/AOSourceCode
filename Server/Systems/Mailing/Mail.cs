using System;
using System.Collections.Generic;
using AO.Core.Ids;
using SqlKata;

namespace AO.Systems.Mailing
{
    public sealed class Mail
    {
        [Ignore]
        [Column("id")]
        public uint Id { get; set; }

        [Column("sender_character_name")]
        public string SenderCharacterName { get; set; }
        
        [Column("sender_character_id")]
        public uint SenderCharacterId { get; set; }
        
        [Column("recipient_character_name")]
        public string RecipientCharacterName { get; set; }
        
        [Column("recipient_character_id")]
        public uint RecipientCharacterId { get; set; }
        
        [Column("subject")]
        public string Subject { get; set; }
        
        [Column("body")]
        public string Body { get; set; }

        [Column("items_json")]
        public string ItemsJson { get; set; }
        
        [Column("expiration_date")]
        public DateTime ExpirationDate { get; set; }
        
        [Column("has_been_returned")]
        public bool HasBeenReturned { get; set; }
        
        [Column("has_been_opened")]
        public bool HasBeenOpened { get; set; }
        
        [Ignore]
        public Dictionary<ItemId, uint> DeserializedItems = new();

        [Ignore] 
        public bool ShouldUpdateDatabase;

        [Ignore] 
        public bool ShouldBeDeleted;
    }
}
