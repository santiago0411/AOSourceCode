using System;
using AO.Core.Utils;
using AO.Players;
using AO.Systems.Questing;
using PacketSender = AO.Network.PacketSender;
using CraftingProfession = AO.Systems.Professions.CraftingProfession;

namespace AO.Items
{
    public class Miscellaneous : Item
    {
        private readonly bool onUseRequiresEquipped;
        private readonly Action<Player> onUse;

        public Miscellaneous(ItemInfo itemInfo, MiscellaneousType miscellaneousType)
            : base(itemInfo)
        {
            switch (miscellaneousType)
            {
                case MiscellaneousType.BlacksmithingHammer:
                    onUseRequiresEquipped = true;
                    onUse = player => PacketSender.ClickRequest(player.Id, ClickRequest.CraftBlacksmithing);
                    break;

                case MiscellaneousType.IronOre:
                case MiscellaneousType.SilverOre:
                case MiscellaneousType.GoldOre:
                    onUseRequiresEquipped = true;
                    onUse = player => PacketSender.ClickRequest(player.Id, ClickRequest.Smelt);
                        break;

                case MiscellaneousType.Handsaw:
                    onUseRequiresEquipped = true;
                    onUse = player => PacketSender.OpenCraftingWindow(player, CraftingProfession.Woodworking);
                    break;

                case MiscellaneousType.SewingKit:
                    onUseRequiresEquipped = true;
                    onUse = player => PacketSender.OpenCraftingWindow(player, CraftingProfession.Tailoring);
                    break;
                
                case MiscellaneousType.QuestGiver:
                    onUse = player => QuestManager.AssignQuestToPlayer(QuestId, player);
                    break;
            }
        }

        public override bool Equip(Player player) => onUseRequiresEquipped;

        public override bool Use(Player player)
        {
            if (!onUseRequiresEquipped || ItemEquipped(player))
                onUse?.Invoke(player);

            return false; // Do NOT return true because that will remove the item
        }

        private bool ItemEquipped(Player player)
        {
            if (player.Inventory.TryGetEquippedItem(ItemType.Miscellaneous, out var item))
            {
                if (item != this)
                {
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.MustEquipItemFirst);
                    return false;
                }
            }

            return true;
        }
    }
}

