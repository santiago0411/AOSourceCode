using AO.Core.Utils;
using AO.Players;
using AO.Players.Talents.Worker;
using PacketSender = AO.Network.PacketSender;

namespace AO.Items
{
    public class Boat : Item
    {
        public Boat(ItemInfo itemInfo) 
            : base(itemInfo)
        {
        }
        
        public override bool Equip(Player player)
        {
            if (player.IsGameMaster)
                return true;

            if (IsGalley && player.Class.ClassType != ClassType.Worker)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantUseClass);
                return false;
            }
            
            if (player.Skills[Skill.Sailing] < SkillToUse)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughSkillToUse,  stackalloc[] {(int)Skill.Sailing});
                return false;
            }

            if (IsGalley && !player.WorkerTalentTrees.FishingTree.GetNode(FishingTalent.GalleyFishing).Acquired)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantUseTalent);
                return false;
            }

            player.HasBoatEquipped = true;
            return true;
        }
    }
}
