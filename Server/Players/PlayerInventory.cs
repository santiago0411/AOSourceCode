using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Items;
using AO.Players.Talents.Worker;
using AO.World;
using PacketSender = AO.Network.PacketSender;

namespace AO.Players
{
    public sealed class  PlayerInventory : IEnumerable<InventorySlot>
    {
        public InventorySlot this[int index] => inventory[index];

        private byte freeSlotsCount = Constants.PLAYER_INV_SPACE;
        
        private readonly Item gold;
        private readonly Player owner;
        private readonly InventorySlot[] inventory;
        private readonly Dictionary<ItemType, InventorySlot> equippedItems = new();

        public PlayerInventory(Player owner, string jsonString)
        {
            this.owner = owner;
            gold = GameManager.Instance.GetItem(Constants.GOLD_ID);
            inventory = CharacterManager.ConvertJsonToInventory(jsonString);

            foreach (var slot in inventory)
            {
                if (slot is null)
                    continue;
                
                freeSlotsCount--;
                // Add all the items the player has equipped to the dictionary
                if (slot is {Equipped: true})
                {
                    equippedItems.Add(slot.Item.Type, slot);
                    if (slot.Item.Type == ItemType.Boat)
                        owner.HasBoatEquipped = true;
                }
            }
        }

        public bool TryGetEquippedItem(ItemType itemType, out Item item)
        {
            if (!equippedItems.TryGetValue(itemType, out var slot))
            {
                item = default;
                return false;
            }

            item = slot.Item;
            return true;
        }

        public bool TryGetEquippedItemSlot(ItemType itemType, out InventorySlot slot)
        {
            return equippedItems.TryGetValue(itemType, out slot);
        }

        public bool HasItemEquipped(ItemType itemType)
        {
            return equippedItems.ContainsKey(itemType);
        }
        
        // The reason for having two functions that report equipped items (all of them at once, and individually)
        // is because this function is used to report to the players that come in range with each other
        // and to initially report to the owner of the equipped items when they first log in.
        public void SendEquippedItemsToPlayer(ClientId playerClientId)
        {
            Span<(byte, ItemId)> equippedItemsId = stackalloc (byte, ItemId)[equippedItems.Count];
            int i = 0;
            foreach (var slot in equippedItems.Values)
            {
                equippedItemsId[i] = (slot.Slot, slot.Item.Id);
                i++;
            }
            PacketSender.PlayerEquippedItems(owner.Id, equippedItemsId, playerClientId);
        }
        
        public void SendAllInventoryToPlayer()
        {
            foreach (var slot in inventory)
                if (slot is not null)
                    PacketSender.PlayerInventory(owner.Id, slot);
            SendEquippedItemsToPlayer(owner.Id);
        }
        
        /// <summary>Adds an item to the player inventory.</summary>
        public bool AddItemToInventory(Item item, ushort quantity)
        {
            if (!CanAddItemToInventory(item, quantity, out var slotsWithItem))
                return false;
            
            // Loop through all the slots that have the same item
            foreach (var slot in slotsWithItem ?? SlotsWithItem(item.Id))
            {
                // If the total quantity doesn't exceed MaxStacks add it to that slot
                var totalQuantity = (ushort)(slot.Quantity + quantity);
                if (totalQuantity <= slot.Item.MaxStacks)
                {
                    slot.Quantity += quantity;
                    PacketSender.PlayerUpdateInventory(owner.Id, slot.Slot, slot.Quantity);
                    return true;
                }

                // If if does exceed it add the maximum possible and set quantity to the remaining extras for the next loop
                var extras = (ushort)(totalQuantity - item.MaxStacks);
                slot.Quantity = item.MaxStacks;
                quantity = extras;
                PacketSender.PlayerInventory(owner.Id, slot);
            }
            
            // If the slots that already had the same item didn't have enough space
            // Loop until the quantity to add reaches 0
            while (quantity > 0)
            {
                // Find the index of the first empty (null) slot, no need to check for -1 because it's checked in CanAddItemToInventory()
                int emptyIndex = Array.FindIndex(inventory, x => x is null);
                var quantityToAdd = quantity <= item.MaxStacks ? quantity : item.MaxStacks;
                // Subtract from the total quantity the quantity that will be added this iteration
                quantity -= quantityToAdd;
                
                // Finally create a new slot, reduce the free count and notify the player
                var newSlot = new InventorySlot((byte)emptyIndex, item, quantityToAdd);
                inventory[emptyIndex] = newSlot;
                freeSlotsCount--;
                PacketSender.PlayerInventory(owner.Id, newSlot);
            }

            return true;
        }
        
        /// <summary>Returns whether the player has an empty slot or a slot where the item is the same and the quantity doesn't exceed the maximum.</summary>
        private bool CanAddItemToInventory(Item item, ushort quantity, out IEnumerable<InventorySlot> slotsWithItem)
        {
            // Calculate how many slots will be needed and if there are more or the same amount of free slots return true 
            var slotsNeeded = Mathf.CeilToInt((float)quantity / item.MaxStacks);
            if (freeSlotsCount >= slotsNeeded)
            {
                slotsWithItem = null;
                return true;
            }
            
            // Get all the slots with the same item and count how many free spaces are left in total
            slotsWithItem = SlotsWithItem(item.Id);
            var totalFreeSpaces = slotsWithItem.Sum(slot => slot.Item.MaxStacks - slot.Quantity);
            // If there are enough free spaces left return true
            if (totalFreeSpaces >= quantity)
                return true;
            
            // Recalculate how many extra slots are needed and return whether there are enough free slots
            var extraSpacesNeeded = quantity - totalFreeSpaces;
            slotsNeeded = Mathf.CeilToInt((float)extraSpacesNeeded / item.MaxStacks);
            return freeSlotsCount >= slotsNeeded;
        }
        
        private IEnumerable<InventorySlot> SlotsWithItem(ItemId itemId)
        {
            // Don't use linq to avoid closure since this method is called very often
            foreach (var slot in inventory)
                if (slot is not null && slot.Item.Id == itemId)
                    yield return slot;
        }

        /// <summary>Returns the sum of the quantity of all slots that contain the given item.</summary>
        public uint TotalItemQuantity(ItemId itemId)
        {
            // Don't use linq to avoid closure, and use a uint cause ushort could overflow
            uint result = 0;
            foreach (var slot in inventory)
                if (slot is not null && slot.Item.Id == itemId)
                    result += slot.Quantity;

            return result;
        }

        /// <summary>Tries to use the item at the specified slot.</summary>
        public void UseItem(byte itemSlot)
        {
            if (owner.Flags.IsDead || inventory[itemSlot] is null) 
                return;
            
            owner.Flags.IsWorking = false;

            if (owner.Flags.IsMeditating)
            {
                PacketSender.SendMultiMessage(owner.Id, MultiMessage.CantUseWhileMeditating);
                return;
            }

            var slot = inventory[itemSlot];

            //Check use interval - don't update interval when weapon is a tool or ranged
            if (!Timers.PlayerCanUseInterval(owner, !(slot.Item.WeaponIsTool || slot.Item.IsRangedWeapon)))
                return;

            if (slot.Item.Use(owner))
            {
                if (slot.Item.Type == ItemType.Gold)
                {
                    PlayerMethods.AddGold(owner, slot.Quantity);
                    slot.Quantity = 1; //Set gold quantity to 1 because gold can only be used once
                }

                //If it was successfully used remove 1 and notify the player
                slot.Quantity--;
                PacketSender.PlayerUpdateInventory(owner.Id, slot.Slot, slot.Quantity);

                //If the quantity is 0 or lower clear the slot
                if (slot.Quantity <= 0)
                    inventory[itemSlot] = null;
            }
        }

        /// <summary>Tries to equip the item at the specified slot.</summary>
        public void EquipItem(byte itemSlot)
        {
            if (!owner.Flags.IsDead && inventory[itemSlot] is not null)
            {
                var slot = inventory[itemSlot];

                if (slot.Equipped)
                {
                    UnequipItem(slot);
                }
                else if (slot.Item.Equip(owner))
                {
                    //Check if the player already has an item of that type equipped
                    if (equippedItems.TryGetValue(slot.Item.Type, out var otherSlot))
                    {
                        if (slot.Item.Type == ItemType.Boat && otherSlot.Item.Type == ItemType.Boat)
                            return;
                        UnequipItem(otherSlot);
                    }

                    EquipItem(slot);
                }
            }
        }
        
        /// <summary>Unequips the item at the specified slot and notifies the player.</summary>
        private void UnequipItem(InventorySlot slot)
        {
            if (slot.Item.Type == ItemType.Boat)
            {
                if (owner.Flags.IsSailing) 
                    return;
                owner.HasBoatEquipped = false;
            }

            equippedItems.Remove(slot.Item.Type);
            slot.Equipped = false;
            PacketSender.OnPlayerItemEquippedChanged(owner, slot.Slot, slot.Item.Id, false);
        }

        /// <summary>Equips the item at the specified slot and notifies the player.</summary>
        private void EquipItem(InventorySlot slot)
        {
            equippedItems.Add(slot.Item.Type, slot);
            slot.Equipped = true;
            PacketSender.OnPlayerItemEquippedChanged(owner, slot.Slot, slot.Item.Id, true);
        }

        /// <summary>Grabs the item the player is standing over.</summary>
        public void GrabItem()
        {
            if (owner.Flags.IsDead) 
                return;
            
            WorldItem worldItem = WorldMap.GetWorldItemAtPosition(owner.CurrentTile.Position);
            if (!worldItem || !worldItem.Grabbable)
                return;
                
            if (worldItem.ItemId == Constants.GOLD_ID)
            {
                PlayerMethods.AddGold(owner, worldItem.Quantity);
                worldItem.ResetWorldItem();
                return;
            }
                
            var item = GameManager.Instance.GetItem(worldItem.ItemId);
            if (AddItemToInventory(item, worldItem.Quantity))
                worldItem.ResetWorldItem();
        }

        /// <summary>Removes an item from the player inventory and creates a world item.</summary>
        public void DropItem(byte itemSlot, ushort quantity, bool draggedAndDropped, Vector2 position)
        {
            InventorySlot slot = inventory[itemSlot];

            if (!owner.Flags.IsDead && slot is not null)
            {
                //If it's a boat and player is sailing it cannot be dropped
                if (slot.Item.Type == ItemType.Boat && owner.Flags.IsSailing)
                   return;

                if (quantity > slot.Quantity) 
                    quantity = slot.Quantity;

                //If the item was dragged and dropped onto the screen
                if (draggedAndDropped)
                {
                    //Round the position because it was taken from the mouse position
                    position.Round(0);
                    Player targetPlayer = WorldMap.GetPlayerAtPosition(position);
                    
                    if (targetPlayer && targetPlayer.Inventory.AddItemToInventory(slot.Item, quantity))
                    {
                        PacketSender.SendMultiMessage(owner.Id, MultiMessage.PlayerDroppedItemTo,  stackalloc[] {targetPlayer.Id.AsPrimitiveType(), slot.Item.Id.AsPrimitiveType(), quantity});
                        PacketSender.SendMultiMessage(targetPlayer.Id, MultiMessage.PlayerGotItemDropped,  stackalloc[] {owner.Id.AsPrimitiveType(), slot.Item.Id.AsPrimitiveType(), quantity});
                        RemoveQuantityFromSlot(itemSlot, quantity);
                        return;
                    }
                }

                //If the item wasn't dragged and dropped, there was no target player to drop it to, or the target player has a full inventory
                //Try to add quantity to a world item that might already be there
                if (!TryAddQuantityToWorldItem(position, slot, quantity))
                {
                    //If there is no world item at that position try to create it on the ground
                    if (GameManager.Instance.CreateWorldItem(slot.Item, quantity, position))
                    {
                        //Remove it if it was created
                        RemoveQuantityFromSlot(itemSlot, quantity);
                    }
                }
            }
        }

        /// <summary>Tries to drop the sent amount of gold.</summary>
        public void DropGold(uint amount)
        {
            if (amount > 100000) amount = 100000;
            if (amount > owner.Gold) amount = owner.Gold;

            while (amount > 0)
            {
                //If the total amount to drop is higher than 10k, set next drop to 10k otherwise set to the total amount
                uint nextDropAmount = amount > 10000 ? 10000 : amount;

                //If CreateWorldItem returns false, it couldn't find an available position and the item wasn't created
                if (!GameManager.Instance.CreateWorldItem(gold, (ushort)nextDropAmount, owner.CurrentTile.Position))
                    break;

                PlayerMethods.RemoveGold(owner, nextDropAmount);
                amount -= nextDropAmount;
            } 
        }

        public void DropAllItems()
        {
            foreach (InventorySlot slot in inventory)
            {
                if (slot is null) 
                    continue;
                
                if (!slot.Item.IsNewbie && slot.Item.Falls)
                {
                    var dropQuantity = slot.Quantity;
                        
                    // If the player class is worker reduce collection items drop quantity
                    if (owner.Class.ClassType == ClassType.Worker)
                    {
                        ReduceOresDropQuantity(slot.Item.Id, ref dropQuantity);
                        ReduceWoodDropQuantity(slot.Item.Id, ref dropQuantity);
                    }
                    
                    // Remove the item regardless of if it was created or not, always lose items on death
                    RemoveQuantityFromSlot(slot.Slot, dropQuantity);
                    GameManager.Instance.CreateWorldItem(slot.Item, dropQuantity, owner.CurrentTile.Position);
                }
                else
                {
                    // Unequip every item that is newbie, or doesn't get dropped
                    if (slot.Item.Type != ItemType.Boat)
                        UnequipItem(slot);
                }
            }
        }

        public void LoseNewbieItems()
        {
            foreach (var slot in inventory)
                if (slot is not null && slot.Item.IsNewbie)
                    RemoveQuantityFromSlot(slot.Slot, slot.Quantity);
        }

        private void ReduceOresDropQuantity(ItemId itemId, ref ushort dropQuantity)
        {
            // If the item to drop is an ore
            bool itemIsOre = itemId == Constants.IRON_ORE_ID || itemId == Constants.SILVER_ORE_ID || itemId == Constants.GOLD_ORE_ID;
            if (itemIsOre)
            {
                // Get the mining talent tree and the drop less ore node
                var miningTalentTree = owner.WorkerTalentTrees.MiningTree;
                var dropLessOreNode = miningTalentTree.GetNode(MiningTalent.DropLessOre);
                            
                // Drop quantity explanation
                // 1 point  -> drop 70% (0.7) -> ((8 - 1) / 10) -> 0.7 -> 100 ore * 0.7 = 70 ore to drop
                // 2 points -> drop 60% (0.6) -> ((8 - 2) / 10) -> 0.6 -> 100 ore * 0.6 = 60 ore to drop
                // 3 points -> drop 50% (0.5) -> ((8 - 3) / 10) -> 0.5 -> 100 ore * 0.5 = 50 ore to drop
                if (dropLessOreNode.Acquired)
                    dropQuantity *= (ushort)((MiningNodesConstants.ORE_DROP_CONSTANT - dropLessOreNode.Points) / 10);
            }
        }

        private void ReduceWoodDropQuantity(ItemId itemId, ref ushort dropQuantity)
        {
            // If the item to drop is wood or elfic wood
            if (itemId == Constants.WOOD_ID || itemId == Constants.ELFIC_WOOD_ID)
            {
                // Get the wood cutting talent tree and the drop less wood node
                var woodCuttingTalentTree = owner.WorkerTalentTrees.WoodCuttingTree;
                var dropLessWoodNode = woodCuttingTalentTree.GetNode(WoodCuttingTalent.DropLessWood);
                
                if (dropLessWoodNode.Acquired)
                    dropQuantity *= (ushort)((WoodCuttingNodesConstants.WOOD_DROP_CONSTANT - dropLessWoodNode.Points) / 10);
            }
        }

        /// <summary>Swaps the slot of the items.</summary>
        public void SwapItemSlot(byte originalSlot, byte newSlot)
        {
            //Check that the slot contains an item
            if (inventory[originalSlot] is null) 
                return;
            
            //Get a pointer to each slot
            var oldSlot = inventory[originalSlot];
            var slot = inventory[newSlot];
            //Swap slot items
            inventory[newSlot] = oldSlot;
            inventory[originalSlot] = slot;

            //Swap slot numbers
            oldSlot.Slot = newSlot;
            PacketSender.PlayerSwapInventorySlots(owner.Id, originalSlot, newSlot);

            //Update second slot number if its not null
            if (slot is not null)
                slot.Slot = originalSlot;
        }

        public void RemoveAllFromSlot(byte slot)
        {
            var itemSlot = inventory[slot];
            if (itemSlot is not null)
                RemoveQuantityFromSlot(slot, itemSlot.Quantity);
        }
        
        /// <summary>Removes the specified quantity from the specified slot.</summary>
        public void RemoveQuantityFromSlot(byte slot, ushort quantity)
        {
            //Get a pointer to the item
            var itemSlot = inventory[slot];
            itemSlot.Quantity -= quantity;
            
            //If the quantity is 0 or lower clear the slot
            if (itemSlot.Quantity <= 0)
            {
                inventory[slot] = null;
                freeSlotsCount++;
            }

            //If the player had the item equipped remove it and notify him
            if (itemSlot.Equipped && itemSlot.Quantity == 0)
                UnequipItem(itemSlot);
            
            PacketSender.PlayerUpdateInventory(owner.Id, slot, itemSlot.Quantity); //Send update after equipped cause it will remove the item from the client and will not be able to unequip
        }

        /// <summary>Removes the specified amount of an item from all the slots required. IMPORTANT: before calling this method use TotalItemQuantity() to check whether the player has enough quantity to be removed.</summary>
        public void RemoveQuantityFromInventory(ItemId itemId, ushort quantity)
        {
            foreach (var slot in SlotsWithItem(itemId))
            {
                if (slot.Quantity >= quantity)
                {
                    RemoveQuantityFromSlot(slot.Slot, quantity);
                    return;
                }

                quantity -= slot.Quantity;
                RemoveQuantityFromSlot(slot.Slot, slot.Quantity);
            }
        }

        /// <summary>Tries to add more items to an existing world item on the specified position.</summary>
        /// <returns>Returns true when the quantity is added or the item doesn't match the id. Returns false when a new world item should be created instead.</returns>
        private bool TryAddQuantityToWorldItem(Vector2 position, InventorySlot itemSlot, ushort quantityToDrop)
        {
            //If there is an item at the player's position
            var worldItem = WorldMap.GetWorldItemAtPosition(position);

            if (worldItem)
            {
                //If the item is the same as the one the player is trying to drop and the quantity won't exceed max
                if (worldItem.ItemId == itemSlot.Item.Id && (worldItem.Quantity + quantityToDrop) <= itemSlot.Item.MaxStacks)
                {
                    //Add the quantity to the world item and remove it from the player's inventory
                    worldItem.Quantity += quantityToDrop;
                    RemoveQuantityFromSlot(itemSlot.Slot, quantityToDrop);
                    return true;
                }

                //The item isn't the same or the quantity exceeds max
                PacketSender.SendMultiMessage(owner.Id, MultiMessage.NoSpaceToDropItem);
                return true;
            }

            return false;
        }

        public IEnumerator<InventorySlot> GetEnumerator()
        {
            foreach (var slot in inventory)
                yield return slot;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class InventorySlot
    {
        /// <summary>The index of this slot instance on the inventory array in PlayerInventory. Used as id.</summary>
        public byte Slot;

        public readonly Item Item;
        public ushort Quantity;
        public bool Equipped;
        public uint SellingPrice => (uint)Item.Price / Constants.SELLING_PRICE_REDUCTION;

        public InventorySlot(byte slot, Item item, ushort quantity, bool equipped = false)
        {
            Slot = slot;
            Item = item;
            Quantity = quantity;
            Equipped = equipped;
        }
    }
}