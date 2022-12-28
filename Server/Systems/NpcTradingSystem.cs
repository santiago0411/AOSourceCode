using System;
using System.Linq;
using UnityEngine;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Npcs;
using AO.Npcs.Utils;
using AO.Players;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems
{
    public static class NpcTradingSystem
    {
        public static float Discount(byte tradingSkill)
        {
            return 1 + tradingSkill / 100f;
        }

        public static void SellToPlayer(Player player, Npc npc, byte slot, ushort quantity)
        {
            //Ban player if they sent invalid data (edited packet)
            if (slot > Constants.NPC_INVENTORY_SPACE || quantity <= 0) return; //TODO Ban

            if (slot > npc.Inventory.Length) return;

            if (quantity > npc.Inventory[slot].Quantity)
                quantity = npc.Inventory[slot].Quantity;

            int finalPrice = Mathf.CeilToInt(npc.Inventory[slot].Price / Discount(player.Skills[Skill.Trading]) * quantity);
            if (finalPrice > player.Gold)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughMoney);
                return;
            }

            var item = npc.Inventory[slot].Item;

            if (player.Inventory.AddItemToInventory(item, quantity))
            {
                PlayerMethods.RemoveGold(player, (uint)finalPrice);
                RemoveItems(npc, slot, quantity);
                PlayerMethods.TryLevelSkill(player, Skill.Trading);
            }
            else
            {
                //Player inventory is full
                PacketSender.SendMultiMessage(player.Id, MultiMessage.InventoryFull);
            }
        }

        public static void BuyFromPlayer(Player player, Npc npc, byte slot, ushort quantity)
        {
            //Ban player if they sent invalid data (edited packet)
            if (slot > Constants.PLAYER_INV_SPACE || quantity <= 0) return;

            if (quantity > player.Inventory[slot].Quantity)
                quantity = player.Inventory[slot].Quantity;

            //Get a pointer to the item
            var item = player.Inventory[slot].Item;

            //Remove it from the inventory and add the gold divided by the selling reduction
            player.Inventory.RemoveQuantityFromSlot(slot, quantity);
            PlayerMethods.AddGold(player, (uint)item.Price / Constants.SELLING_PRICE_REDUCTION);
            PlayerMethods.TryLevelSkill(player, Skill.Trading);

            AddItems(npc, item.Id, quantity);
        }

        private static void AddItems(Npc npc, ItemId itemId, ushort quantity)
        {
            //Get a pointer to the item and an array with all the slots containing that item
            var item = GameManager.Instance.GetItem(itemId);
            NpcInventorySlot[] slotsWithItem = npc.Inventory.Where(x => x is not null && x.Item.Id == itemId).ToArray();

            foreach (var slot in slotsWithItem)
            {
                //If a slot with that item exists and the quantity doesn't exceed max add it and notify the players currently trading
                if ((quantity + slot.Quantity) <= item.MaxStacks)
                {
                    slot.Quantity += quantity;
                    foreach (var player in npc.InteractingWith)
                        PacketSender.NpcUpdateInventory(player, slot.Slot, slot.Quantity);
                    return;
                }
            }

            //If the npc previously had the same item or the trader keeps all items it buys add it to a new slot
            if (slotsWithItem.Length > 0 || npc.Info.KeepsItems)
            {
                int index = Array.FindIndex(npc.Inventory, x => x is null);
                //If FindIndex returns -1 the array is full
                if (index != -1)
                {
                    //Create a new slot and notify all the players currently trading
                    npc.Inventory[index] = new NpcInventorySlot((byte) index, item, quantity, false);
                    foreach (var player in npc.InteractingWith)
                        PacketSender.NpcUpdateInventory(player, (byte) index, quantity, item.Id);
                }
            }
        }

        private static void RemoveItems(Npc npc, byte slotNumber, ushort quantity)
        {
            //Get a pointer to the slot and remove the quantity
            var slot = npc.Inventory[slotNumber];
            slot.Quantity -= quantity;

            //If the slot has run out and the slot respawns reset it to the original quantity
            if (slot.Quantity <= 0 && slot.Respawns)
                slot.Quantity = slot.OriginalQuantity;

            //Notify all the players currently trading
            foreach (var player in npc.InteractingWith)
                PacketSender.NpcUpdateInventory(player, slot.Slot, slot.Quantity);
        }
    }
}