using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Items;
using AO.Players;
using AO.Players.Talents.Worker;
using JetBrains.Annotations;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Professions
{
    public sealed class CraftableItem
    {
        [UsedImplicitly]
        public readonly ushort Id;
        
        [UsedImplicitly]
        public readonly CraftingProfession Profession;
        
        [UsedImplicitly]
        public readonly ushort StaminaRequired;
        
        [UsedImplicitly]
        private readonly ItemId itemId;
        
        [UsedImplicitly]
        private readonly byte levelToCraft;

        [UsedImplicitly]
        private readonly string requiredTalent;
        
        [UsedImplicitly]
        private readonly byte pointsInTalent;

        public Item Item => Core.GameManager.Instance.GetItem(itemId);
        public ReadOnlyCollection<(ItemId itemId, ushort amountNeeded)> RequiredItemsAndAmounts;
        
        private BlacksmithingTalent? requiredBlacksmithingTalent;
        private WoodWorkingTalent? requiredWoodWorkingTalent;
        private TailoringTalent? requiredTailoringTalent;
        private Func<Player, bool> checkRequiredTalent;
        
        private CraftableItem() {}
        
        public void LoadRequirements(List<(ItemId, ushort)> requiredItemsAndAmounts)
        {
            RequiredItemsAndAmounts = new ReadOnlyCollection<(ItemId, ushort)>(requiredItemsAndAmounts);
            
            switch (Profession)
            {
                case CraftingProfession.Blacksmithing:
                    if (Enum.TryParse(requiredTalent, true, out BlacksmithingTalent bsTalent))
                        requiredBlacksmithingTalent = bsTalent;

                    checkRequiredTalent = CheckRequiredTalentBs;
                    break;
                case CraftingProfession.Woodworking:
                    if (Enum.TryParse(requiredTalent, true, out WoodWorkingTalent wwTalent))
                        requiredWoodWorkingTalent = wwTalent;

                    checkRequiredTalent = CheckRequiredTalentWw;
                    break;
                case CraftingProfession.Tailoring:
                    if (Enum.TryParse(requiredTalent, true, out TailoringTalent tlTalent))
                        requiredTailoringTalent = tlTalent;

                    checkRequiredTalent = CheckRequiredTalentTl;
                    break;
            }
        }
        
        public bool HasEnoughMaterials(Player player, ushort amountToCraft)
        {
            foreach (var (requiredItemId, requiredAmount) in RequiredItemsAndAmounts)
            {
                if (player.Inventory.TotalItemQuantity(requiredItemId) < (requiredAmount * amountToCraft))
                {
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughMaterials);
                    return false;
                }
            }

            return true;
        }
        
        public bool CanCraftItem(Player player)
        {
            return levelToCraft <= player.Level && checkRequiredTalent(player);
        }

        private bool CheckRequiredTalentBs(Player player)
        {
            if (!requiredBlacksmithingTalent.HasValue)
                return true;

            if (player.Class.ClassType != ClassType.Worker)
                return false;

            var bsTalentTree = player.WorkerTalentTrees.BlacksmithingTree;
            return bsTalentTree.GetNode(requiredBlacksmithingTalent.Value).Points >= pointsInTalent;
        }
        
        private bool CheckRequiredTalentWw(Player player)
        {
            if (!requiredWoodWorkingTalent.HasValue)
                return true;

            if (player.Class.ClassType != ClassType.Worker)
                return false;

            var wwTalentTree = player.WorkerTalentTrees.WoodWorkingTree;
            return wwTalentTree.GetNode(requiredWoodWorkingTalent.Value).Points >= pointsInTalent;
        }
        
        private bool CheckRequiredTalentTl(Player player)
        {
            if (!requiredTailoringTalent.HasValue)
                return true;

            if (player.Class.ClassType != ClassType.Worker)
                return false;

            var tlTalentTree = player.WorkerTalentTrees.TailoringTree;
            return tlTalentTree.GetNode(requiredTailoringTalent.Value).Points >= pointsInTalent;
        }
    }
}
