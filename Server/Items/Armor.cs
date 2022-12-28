using AO.Core.Utils;
using AO.Players;
using PacketSender = AO.Network.PacketSender;

namespace AO.Items
{
    public class Armor : Item
    {
        public Armor(ItemInfo itemInfo) 
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
            if (NotAllowedRaces.Contains(player.Race.RaceType))
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantUseRace);
                return false;
            }
            if (Gender != Gender.Both && player.Gender != Gender)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantUseGender);
                return false;
            }
            if (player.Skills[Skill.CombatTactics] < SkillToUse)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughSkillToUse,  stackalloc[] {(int)Skill.CombatTactics});
                return false;
            }
            if ((ImperialOnly && player.Faction != Faction.Imperial) || (ChaosOnly && player.Faction != Faction.Chaos))
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantUseFaction);
                return false;
            }

            return true;
        }
    }
}
