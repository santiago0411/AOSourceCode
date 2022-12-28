using AO.Core.Utils;
using AO.Players;
using PacketSender = AO.Network.PacketSender;

namespace AO.Items
{
    public class Shield : Item
    {
        public Shield(ItemInfo itemInfo) 
            : base(itemInfo)
        {
        }
        
        public override bool Equip(Player player)
        {
            if (player.IsGameMaster)
                return true;

            if (IsNewbie && !PlayerMethods.IsNewbie(player))
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.ItemOnlyNewbies);
                return false;
            }

            if (NotAllowedClasses.Contains(player.Class.ClassType))
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantUseClass);
                return false;
            }

            if (player.Inventory.TryGetEquippedItem(ItemType.Weapon, out var weapon))
            {
                if (weapon.WeaponType == WeaponType.Axe)
                {
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.CantUseAxeAndShield);
                    return false;
                }
            }

            if (player.Skills[Skill.ShieldDefense] < SkillToUse)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughSkillToUse, stackalloc[] {(int)Skill.ShieldDefense});
                return false;
            }

            return true;
        }
    }
}