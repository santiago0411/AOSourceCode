using AO.Core.Utils;
using AO.Players;
using PacketSender = AO.Network.PacketSender;

namespace AO.Items
{
    public class Arrow : Item
    {    
        public Arrow(ItemInfo itemInfo) 
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

            return true;
        }
    }
}
