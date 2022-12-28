using AO.Core.Utils;
using AO.Players;
using PacketSender = AO.Network.PacketSender;

namespace AO.Items
{
    public class Ring : Item
    {
        public Ring(ItemInfo itemInfo) 
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
            if (player.Skills[Skill.Magic] < SkillToUse)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughSkillToUse,  stackalloc[] {(int)Skill.Magic});
                return false;
            }

            return true;
        }
    }
}
