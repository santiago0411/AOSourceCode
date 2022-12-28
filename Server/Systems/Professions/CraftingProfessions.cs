using System.Collections.Generic;
using AO.Core.Ids;
using UnityEngine;
using AO.Core.Utils;
using AO.Items;
using AO.Players;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Professions
{
    public static class CraftingProfessions
    {
        private static void CraftItem(Player player, WorkParameters craftingParameters)
        {
            var craftableItem = craftingParameters.CraftableItem;
            
            if (!PlayerMethods.TakeStamina(player, craftableItem.StaminaRequired))
                return;
            
            if (!player.Inventory.AddItemToInventory(craftableItem.Item, 1))
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.InventoryFull);
                player.Flags.IsWorking = false;
                return;
            }

            foreach (var (requiredItemId, requiredAmount) in craftableItem.RequiredItemsAndAmounts)
                player.Inventory.RemoveQuantityFromInventory(requiredItemId, requiredAmount);

            if (--craftingParameters.AmountToCraft <= 0)
                player.Flags.IsWorking = false;
        }
        
        // ########## BLACKSMITHING START ########## //
        public static readonly Dictionary<ItemId, CraftableItem> BlacksmithingItems = new();
        private static readonly Collider2D[] collisionResults = new Collider2D[5];

        public static void TryBlacksmithing(Player player, ItemId itemToCraft, ushort amountToCraft)
        {
            player.Flags.IsWorking = false;

            if (!CanPlayerCraftBlacksmithItem(player, itemToCraft, out var craftableItem))
                return;

            if (!HasHammerEquipped(player))
                return;

            if (!IsNearAnvil(player))
                return;

            if (!craftableItem.HasEnoughMaterials(player, amountToCraft))
                return;

            var parameters = new WorkParameters
            {
                CraftableItem = craftableItem,
                AmountToCraft = amountToCraft
            };

            player.StartCoroutine(player.WorkCoroutine(CraftItem, parameters));
        }

        private static bool CanPlayerCraftBlacksmithItem(Player player, ItemId itemId, out CraftableItem craftableItem)
        {
            if (!BlacksmithingItems.TryGetValue(itemId, out craftableItem)) 
                return false; // TODO ban sent invalid item id?

            return craftableItem.CanCraftItem(player);
        }

        private static bool HasHammerEquipped(Player player)
        {
            if (player.Inventory.TryGetEquippedItem(ItemType.Miscellaneous, out var item))
                if (item.MiscellaneousType == MiscellaneousType.BlacksmithingHammer)
                    return true;

            PacketSender.SendMultiMessage(player.Id, MultiMessage.NoHammerEquipped);
            return false;
        }

        private static bool IsNearAnvil(Player player)
        {
            var size = Physics2D.OverlapBoxNonAlloc(player.CurrentTile.Position, Vector2.one * 2, 0f, collisionResults, CollisionManager.ObstaclesLayerMask);

            for (int i = 0; i < size; i++)
            {
                if (collisionResults[i].CompareTag(Tag.Anvil.Name))
                    return true;
            }

            PacketSender.SendMultiMessage(player.Id, MultiMessage.TooFarFromAnvil);
            return false;
        }
        // ########## BLACKSMITHING END ########## //
        
        
        // ########## WOODWORKING START ########## //
        public static readonly Dictionary<ItemId, CraftableItem> WoodworkingItems  = new();

        public static void TryWoodworking(Player player, ItemId itemToCraft, ushort amountToCraft)
        {
            player.Flags.IsWorking = false;

            if (!CanPlayerCraftWoodworking(player, itemToCraft, out var craftableItem))
                return;

            if (!HasHandsawEquipped(player))
                return;

            if (!craftableItem.HasEnoughMaterials(player, amountToCraft))
                return;

            var parameters = new WorkParameters
            {
                CraftableItem = craftableItem,
                AmountToCraft = amountToCraft
            };

            player.StartCoroutine(player.WorkCoroutine(CraftItem, parameters));
        }

        private static bool CanPlayerCraftWoodworking(Player player, ItemId itemId, out CraftableItem craftableItem)
        {
            if (!WoodworkingItems.TryGetValue(itemId, out craftableItem)) 
                return false; // TODO ban sent invalid item id?

            return craftableItem.CanCraftItem(player);
        }

        private static bool HasHandsawEquipped(Player player)
        {
            if (player.Inventory.TryGetEquippedItem(ItemType.Miscellaneous, out var item))
                if (item.MiscellaneousType == MiscellaneousType.Handsaw)
                    return true;

            PacketSender.SendMultiMessage(player.Id, MultiMessage.NoHandsawEquipped);
            return false;
        }
        // ########## WOODWORKING END ########## //
        
        
        // ########## TAILORING START ########## //
        public static readonly Dictionary<ItemId, CraftableItem> TailoringItems = new();

        public static void TryTailoring(Player player, ItemId itemToCraft, ushort amountToCraft)
        {
            player.Flags.IsWorking = false;

            if (!CanPlayerCraftTailoring(player, itemToCraft, out var craftableItem))
                return;

            if (!HasSewingKitEquipped(player))
                return;

            if (!craftableItem.HasEnoughMaterials(player, amountToCraft))
                return;

            var parameters = new WorkParameters
            {
                CraftableItem = craftableItem,
                AmountToCraft = amountToCraft
            };

            player.StartCoroutine(player.WorkCoroutine(CraftItem, parameters));
        }

        private static bool CanPlayerCraftTailoring(Player player, ItemId itemId, out CraftableItem craftableItem)
        {
            if (!TailoringItems.TryGetValue(itemId, out craftableItem)) 
                return false; // TODO ban sent invalid item id?

            return craftableItem.CanCraftItem(player);
        }

        private static bool HasSewingKitEquipped(Player player)
        {
            if (player.Inventory.TryGetEquippedItem(ItemType.Miscellaneous, out var item))
                if (item.MiscellaneousType == MiscellaneousType.SewingKit)
                    return true;

            PacketSender.SendMultiMessage(player.Id, MultiMessage.NoSewingKitEquipped);
            return false;
        }
        // ########## TAILORING END ########## //
    }
}