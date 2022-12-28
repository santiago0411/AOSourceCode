using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AO.Core;
using AO.Core.Database;
using AO.Core.Ids;
using AO.Core.Logging;
using AO.Core.Utils;
using AO.Network;
using AO.Players;
using Newtonsoft.Json;
using UnityEngine;

namespace AO.Systems.Mailing
{
    public sealed class MailManager : MonoBehaviour
    {
        private static readonly LoggerAdapter log = new(typeof(MailManager));

        private void Start()
        {
            InvokeRepeating(nameof(DeleteOrReturnExpiredMails), 10f, 60f);
        }

        public static async ValueTask<bool> UpdateAndDeletePlayerMails(Player player)
        {
            if (!player.Flags.CachedMails.Values.Any(m => m.ShouldBeDeleted || m.ShouldUpdateDatabase))
                return true;

            var mailsToUpdate = new List<Mail>();
            var mailsToDelete = new List<uint>();
            foreach (var mail in player.Flags.CachedMails.Values)
            {
                if (mail.ShouldBeDeleted)
                    mailsToDelete.Add(mail.Id);
                else if (mail.ShouldUpdateDatabase)
                    mailsToUpdate.Add(mail);
            }

            if (mailsToUpdate.Count == 0 && mailsToDelete.Count == 0)
                return true;

            Transaction transaction = await DatabaseManager.BeginTransactionAsync();
            if (transaction is null)
            {
                player.Flags.MailCacheExpirationTime = Time.realtimeSinceStartup + Constants.MAILS_CACHE_EXPIRATION_TIME;
                return false;
            }

            bool success = await UpdateAndDeletePlayerMails(mailsToUpdate, mailsToDelete, transaction);
            if (!success)
            {
                player.Flags.MailCacheExpirationTime = Time.realtimeSinceStartup + Constants.MAILS_CACHE_EXPIRATION_TIME;
                return false;
            }

            success = await transaction.CommitTransactionAsync();
            if (!success)
            {
                player.Flags.MailCacheExpirationTime = Time.realtimeSinceStartup + Constants.MAILS_CACHE_EXPIRATION_TIME;
                return false;
            }
            
            return true;
        }

        public static async ValueTask<bool> UpdateAndDeletePlayerMails(List<Mail> mailsToUpdate, List<uint> mailsToDelete, Transaction transaction)
        {
            bool success = mailsToDelete.Count == 0 || await DatabaseOperations.DeleteMails(mailsToDelete, transaction) == DatabaseResultStatus.Ok;
            if (!success)
                return false;

            foreach (var mailToUpdate in mailsToUpdate)
            {
                success = await DatabaseOperations.UpdateMail(mailToUpdate, transaction) == DatabaseResultStatus.Ok;
                if (!success)
                    return false;
            }

            return true;
        }
        
        public static async ValueTask CollectMailItem(Player player, uint mailId, ItemId itemId)
        {
            // Refresh the mail cache if it has expired
            if (player.Flags.MailCacheExpirationTime < Time.realtimeSinceStartup)
                if (!await FetchMails(player, false))
                    return;

            if (player.Flags.CachedMails.Count == 0)
                return; // TODO ban, trying to collect an item from a non existent mail
            
            if (!player.Flags.CachedMails.TryGetValue(mailId, out Mail mail))
                return; // TODO ban, trying to collect an item from a non existent mail or a mail that isn't theirs

            if (mail.ShouldBeDeleted)
                return; // This mail has been deleted from the db already
            
            if (!mail.DeserializedItems.TryGetValue(itemId, out uint quantity))
                return; // TODO ban, trying to collect an item that isn't in the mail

            mail.HasBeenOpened = true;
            
            // Handle special case gold first
            if (itemId == Constants.GOLD_ID)
            {
                PlayerMethods.AddGold(player, quantity);
                RemoveItemAndReserialize(player, mail, itemId);
                return;
            }

            if (player.Inventory.AddItemToInventory(GameManager.Instance.GetItem(itemId), (ushort)quantity))
                RemoveItemAndReserialize(player, mail, itemId);
        }

        private static void RemoveItemAndReserialize(Player player, Mail mail, ItemId itemId)
        {
            mail.DeserializedItems.Remove(itemId);
            mail.ShouldUpdateDatabase = true;
            mail.ItemsJson = SerializeItems(mail.DeserializedItems);
            PacketSender.RemoveMailItem(player.Id, mail.Id, itemId);
        }

        public static async Task<bool> FetchMails(Player player, bool notifyPlayer = true)
        {
            (var status, List<Mail> mails) = await DatabaseOperations.FetchCharactersMail(player.CharacterInfo.CharacterId);

            if (status != DatabaseResultStatus.Ok)
                return false;
            
            if (mails.Count <= 0)
                return true;

            foreach (var mail in mails)
            {
                DeserializeItems(mail);
                player.Flags.CachedMails.Add(mail.Id, mail);
            }

            player.Flags.MailCacheExpirationTime = Time.realtimeSinceStartup + Constants.MAILS_CACHE_EXPIRATION_TIME;
            if (notifyPlayer)
                PacketSender.FetchMailsReturn(player.Id, mails);

            return true;
        }
        
        public static async Task TryCreateAndSendMail(Player sender, string recipientCharName, string subject, string body, uint gold, HashSet<byte> slotsToSend)
        {
            // TODO check sender player is near mailbox
            
            // Validate slots to send
            if (slotsToSend.Count > Constants.MAIL_MAX_ITEMS)
            {
                log.Warn("Player {0} sent too many items to be mailed.", sender.Username);
                return; // TODO ban?
            }

            if (slotsToSend.Any(s => s > Constants.PLAYER_INV_SPACE - 1))
            {
                log.Warn("Player {0} sent invalid slot ids to be mailed.", sender.Username);
                return; // TODO ban?
            }

            var dbConnection = DatabaseManager.GetConnection();
            CharacterId recipientId;
            
            // First check to see if the player the mail is being sent to is online to avoid an extra db operation
            // Either way find the player id and reset the name to match the actual name of the player (case differences could happen)
            if (CharacterManager.Instance.TryGetOnlinePlayer(recipientCharName, out Player recipientPlayer))
            {
                recipientId = recipientPlayer.CharacterInfo.CharacterId;
                recipientCharName = recipientPlayer.Username;
            }
            else
            {
                var dbResult = await DatabaseOperations.FetchCharacterIdAndName(recipientCharName, dbConnection);

                if (dbResult.Status != DatabaseResultStatus.Ok)
                {
                    // Db operation failed
                    PacketSender.SendMultiMessage(sender.Id, MultiMessage.CantSendMailRightNow);
                    await dbConnection.CloseAsync();
                    return;
                }

                if (dbResult.Item1 == CharacterId.Empty)
                {
                    // If the id is 0 it means there is no player with that name
                    PacketSender.SendMultiMessage(sender.Id, MultiMessage.CharacterDoesntExist);
                    await dbConnection.CloseAsync();
                    return;
                }

                recipientId = dbResult.Item1;
                recipientCharName = dbResult.Item2;
            }
            
            // Validate recipient has enough inbox space
            var (status, inboxMailCount) = await DatabaseOperations.FetchCharacterMailCount(recipientId, dbConnection);
            if (status != DatabaseResultStatus.Ok)
            {
                PacketSender.SendMultiMessage(sender.Id, MultiMessage.CantSendMailRightNow);
                await dbConnection.CloseAsync();
                return;
            }

            if (inboxMailCount >= Constants.MAIL_MAX_MAILS)
            {
                PacketSender.SendMultiMessage(sender.Id, MultiMessage.RecipientInboxFull);
                await dbConnection.CloseAsync();
                return;
            }
            
            // Validate player has enough gold to send
            // This is also validated client side
            if (gold > sender.Gold)
                gold = sender.Gold;

            // Serialize inventory slots to json
            var deserializedItems = new Dictionary<ItemId, uint>();
            if (gold > 0)
                deserializedItems.Add(Constants.GOLD_ID, gold);
            
            foreach (var slotId in slotsToSend)
            {
                InventorySlot slot = sender.Inventory[slotId];
                if (slot is null || !slot.Item.Falls)
                    continue;
                deserializedItems.Add(slot.Item.Id, slot.Quantity);
            }

            // Create new mail and write it to database
            var mail = new Mail
            {
                SenderCharacterName = sender.CharacterInfo.CharacterName,
                SenderCharacterId = sender.CharacterInfo.CharacterId.AsPrimitiveType(),
                RecipientCharacterName = recipientCharName,
                RecipientCharacterId = recipientId.AsPrimitiveType(),
                Subject = subject.Length <= 25 ? subject : subject.Substring(0, Constants.MAIL_MAX_SUBJECT),
                Body =  body.Length <= 250 ? body : body.Substring(0, Constants.MAIL_MAX_BODY),
                ItemsJson = SerializeItems(deserializedItems),
                ExpirationDate = DateTime.Now.AddMonths(1),
                DeserializedItems = deserializedItems
            };

            var (status2, newMailId) = await DatabaseOperations.WriteMail(mail, dbConnection);
            await dbConnection.CloseAsync();
            if (status2 != DatabaseResultStatus.Ok)
            {
                PacketSender.SendMultiMessage(sender.Id, MultiMessage.CantSendMailRightNow);
                return;
            }
            
            // Remove gold and items sent from inventory
            if (gold > 0)
                PlayerMethods.RemoveGold(sender, gold);

            foreach (var slotId in slotsToSend)
                sender.Inventory.RemoveAllFromSlot(slotId);
            
            // Notify sender and recipient if online
            PacketSender.SendMultiMessage(sender.Id, MultiMessage.MailSentSuccessfully);
            if (recipientPlayer)
            {
                PacketSender.SendMultiMessage(recipientPlayer.Id, MultiMessage.NewMailReceived, stackalloc int[] { sender.Id.AsPrimitiveType() });
                mail.Id = newMailId;
                recipientPlayer.Flags.CachedMails.Add(mail.Id, mail);
                recipientPlayer.Flags.MailCacheExpirationTime = Time.realtimeSinceStartup + Constants.MAILS_CACHE_EXPIRATION_TIME;
                PacketSender.FetchMailsReturn(recipientPlayer.Id, mail);
            }
        }

        private static string SerializeItems(Dictionary<ItemId, uint> deserializedItems)
        {
            return deserializedItems.Count > 0 ? JsonConvert.SerializeObject(deserializedItems) : null;
        }

        private static readonly JsonSerializerSettings jss = new()
        {
            TypeNameHandling = TypeNameHandling.All, 
            Converters = new List<JsonConverter>() { new CustomDictionaryConverter<ItemId, uint>() }
        };
        
        private static void DeserializeItems(Mail mail)
        {
            if (!string.IsNullOrEmpty(mail.ItemsJson))
                mail.DeserializedItems = JsonConvert.DeserializeObject<Dictionary<ItemId, uint>>(mail.ItemsJson, jss);
        }
        
        private async void DeleteOrReturnExpiredMails()
        {
            Transaction transaction = await DatabaseManager.BeginTransactionAsync();

            if (transaction is null)
                return;
            
            // First delete all mails from the db that have either already been returned and expired again OR do not have any items to be returned
            var (status, deletedMailsCount) = await DatabaseOperations.DeleteExpiredMails(transaction);
            if (status != DatabaseResultStatus.Ok)
                return;

            if (deletedMailsCount > 0)
                log.Info("{0} expired mail(s) have been deleted.", deletedMailsCount);
            
            // Then fetch all the mails that need to be returned
            (var status2, List<Mail> mailsToReturn) = await DatabaseOperations.FetchMailsToReturn(transaction);
            if (status2 != DatabaseResultStatus.Ok)
                return;

            // If there aren't any mails to be returned just exit the function
            if (mailsToReturn.Count == 0)
            {
                await transaction.CommitTransactionAsync();
                return;
            }
            
            var newReturnedMails = new List<Mail>(mailsToReturn.Count);
            foreach (var mail in mailsToReturn)
            {
                // Create a new mail swapping the sender for the recipient and set the HasBeenReturned flag to true
                var newMail = new Mail
                {
                    SenderCharacterName = mail.RecipientCharacterName,
                    SenderCharacterId = 0,
                    RecipientCharacterName = mail.SenderCharacterName,
                    RecipientCharacterId = mail.SenderCharacterId,
                    Subject = $"Expired: {mail.Subject}",
                    Body =  mail.Body,
                    ItemsJson = mail.ItemsJson,
                    ExpirationDate = DateTime.Now.AddMonths(1),
                    HasBeenReturned = true
                };

                // Mark the old mail as deleted in case the player tries to retrieve it from the cache before it's actually deleted from the database
                mail.ShouldBeDeleted = true; 
                newReturnedMails.Add(newMail);
                
                // Write the new mail to the db if there were any.
                var (status3, newMailId) = await DatabaseOperations.WriteMail(newMail, transaction);
                if (status3 != DatabaseResultStatus.Ok)
                    return;

                newMail.Id = newMailId;
            }

            // Finally delete the original mails that have been returned
            var status4 = await DatabaseOperations.DeleteMails(mailsToReturn.Select(m => m.Id), transaction);
            if (status4 != DatabaseResultStatus.Ok)
                return;

            if (!await transaction.CommitTransactionAsync())
                return;
            
            log.Info("{0} expired mail(s) have been returned.", mailsToReturn.Count);
            
            // If new mails have successfully been written to db, check for online recipient players and notify their client
            foreach (var mail in newReturnedMails)
            {
                if (CharacterManager.Instance.TryGetOnlinePlayer(mail.RecipientCharacterName, out var recipientPlayer))
                {
                    DeserializeItems(mail);
                    recipientPlayer.Flags.CachedMails.Add(mail.Id, mail);
                    recipientPlayer.Flags.MailCacheExpirationTime = Time.realtimeSinceStartup + Constants.MAILS_CACHE_EXPIRATION_TIME;
                    PacketSender.FetchMailsReturn(recipientPlayer.Id, mail);
                }
            }
        }
    }
}