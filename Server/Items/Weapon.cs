using AO.Core.Utils;
using AO.Players;
using AO.Players.Talents.Worker;
using PacketSender = AO.Network.PacketSender;

namespace AO.Items
{
    public class Weapon : Item
    {
        public Weapon(ItemInfo itemInfo) 
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

            if (WeaponType == WeaponType.FishingNet)
            {
                if (player.Class.ClassType != ClassType.Worker)
                {
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.CantUseClass);
                    return false;
                }
                
                var fishingTalentTree = player.WorkerTalentTrees.FishingTree;
                if (!fishingTalentTree.GetNode(FishingTalent.UseFishingNet).Acquired)
                {
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.CantUseTalent);
                    return false;
                }

                return true;
            }
            
            if (WeaponType == WeaponType.Staff)
            {
                if (player.Skills[Skill.Magic] < SkillToUse)
                {
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughSkillToUse,  stackalloc[] {(int)Skill.Magic});
                    return false;
                }
            }
            else
            {
                if (IsRangedWeapon)
                {
                    if (player.Skills[Skill.RangedWeapons] < SkillToUse)
                    {
                        PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughSkillToUse,  stackalloc[] {(int)Skill.RangedWeapons});
                        return false;
                    }
                }
                else
                {
                    if (WeaponType == WeaponType.Dagger)
                    {
                        if (player.Skills[Skill.Stabbing] < SkillToUse)
                        {
                            PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughSkillToUse,  stackalloc[] {(int)Skill.Stabbing});
                            return false;
                        }
                    }
                    else
                    {
                        if (WeaponType == WeaponType.Axe && player.Inventory.HasItemEquipped(ItemType.Shield))
                        {
                            PacketSender.SendMultiMessage(player.Id, MultiMessage.CantUseAxeAndShield);
                            return false;
                        }

                        if (player.Skills[Skill.ArmedCombat] < SkillToUse)
                        {
                            PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughSkillToUse,  stackalloc[] {(int)Skill.ArmedCombat});
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public override bool Use(Player player)
        {
            if (WeaponIsTool || IsRangedWeapon)
            {
                if (player.Inventory.TryGetEquippedItem(ItemType.Weapon, out var weapon))
                {
                    if (weapon != this)
                    {
                        PacketSender.SendMultiMessage(player.Id, MultiMessage.MustEquipItemFirst);
                        return false;
                    }
                }
            }

            switch (WeaponType)
            {
                case WeaponType.Bow:
                case WeaponType.Crossbow:
                    PacketSender.ClickRequest(player.Id, ClickRequest.ProjectileAttack);
                    break;
                case WeaponType.FishingRod:
                case WeaponType.FishingNet:
                    PacketSender.ClickRequest(player.Id, ClickRequest.Fish);
                    break;
                case WeaponType.LumberjackAxe:
                    PacketSender.ClickRequest(player.Id, ClickRequest.CutWood);
                    break;
                case WeaponType.MiningPick:
                    PacketSender.ClickRequest(player.Id, ClickRequest.Mine);
                    break;
            }

            return false; //Do NOT return true because that will remove the item and this is a weapon
        }
    }
}
