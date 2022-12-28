using System;
using AO.Core.Utils;
using AO.Items;
using AO.Npcs;
using AO.Npcs.AI;
using AO.Players;
using UnityEngine;
using Attribute = AO.Players.Attribute;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Combat
{
    public static partial class CombatSystem
    {
        private static void PlayerDamagesPlayer(Player attacker, Player target)
        {
            WeaponType equippedWeaponType = 0;
            float headDefense = 0, bodyDefense = 0, boatDefense = 0;
            float damage = CalculateDamage(attacker, out Item weapon);

            TryPlayerEnvenomPlayer(attacker, target, weapon);

            if (attacker.Flags.IsSailing && attacker.Inventory.TryGetEquippedItem(ItemType.Boat, out var attackerBoat))
                damage += ExtensionMethods.RandomNumber(attackerBoat.MinHit, attackerBoat.MaxHit);

            if (target.Flags.IsSailing && target.Inventory.TryGetEquippedItem(ItemType.Boat, out var targetBoat))
                boatDefense = ExtensionMethods.RandomNumber(targetBoat.MinDef, targetBoat.MaxDef);

            if (weapon is not null)
            {
                equippedWeaponType = weapon.WeaponType;
                damage += weapon.Reinforcement;
            }

            int bodyPart = ExtensionMethods.RandomNumber(1, 6);

            if ((BodyPart)bodyPart == BodyPart.Head)
            {
                if (target.Inventory.TryGetEquippedItem(ItemType.Helmet, out var helmet))
                    headDefense = ExtensionMethods.RandomNumber(helmet.MinDef, helmet.MaxDef);
            }
            else
            {
                int minDef = 0, maxDef = 0;

                if (target.Inventory.TryGetEquippedItem(ItemType.Armor, out var armor))
                {
                    minDef = armor.MinDef;
                    maxDef = armor.MaxDef;
                }

                if (target.Inventory.TryGetEquippedItem(ItemType.Shield, out var shield))
                {
                    minDef += shield.MinDef;
                    maxDef += shield.MaxDef;
                }

                bodyDefense = ExtensionMethods.RandomNumber(minDef, maxDef);

                //If the attacker is warrior or paladin and is using a sword reduce armor defense
                if (equippedWeaponType == WeaponType.Sword && CombatSystemUtils.ClassWarriorOrPaladin(attacker.Class.ClassType))
                    bodyDefense *= Constants.SWORD_ARMOR_PEN_MOD;
            }

            damage -= headDefense + bodyDefense + boatDefense;

            if (damage < 0) damage = 1;

            //If the weapon is an axe and the attacker is warrior or paladin try to critically strike
            if (equippedWeaponType == WeaponType.Axe && CombatSystemUtils.ClassWarriorOrPaladin(attacker.Class.ClassType))
                if (ExtensionMethods.RandomNumber(1, 100) <= Constants.AXE_PLAYER_CRIT_CHANCE)
                    damage *= Constants.AXE_CRIT_DAMAGE_MOD;

            int damageInt = Mathf.RoundToInt(damage);

            if (!TryToStab(attacker, weapon, damageInt, targetPlayer: target))
            {
                PacketSender.SendMultiMessage(attacker.Id, MultiMessage.PlayerHitPlayer,  stackalloc[] {target.Id.AsPrimitiveType(), bodyPart, damageInt});
                PacketSender.SendMultiMessage(target.Id, MultiMessage.PlayerHitByPlayer,  stackalloc[] {attacker.Id.AsPrimitiveType(), bodyPart, damageInt});

                target.Health.TakeDamage(damageInt, target.Die);
                PacketSender.PlayerIndividualResource(target, Resource.Health);
            }

            Skill skillToLevel = weapon is null ? Skill.UnarmedCombat
                                : weapon.IsRangedWeapon
                                ? Skill.RangedWeapons : Skill.ArmedCombat;

            PlayerMethods.TryLevelSkill(attacker, skillToLevel);

            if (target.Health.CurrentHealth <= 0)
                PlayerMethods.AddDeath(attacker, target);
        }

        /// <summary>Makes the player do damage to the npc and receive experience.</summary>
        private static void PlayerDamagesNpc(Player player, Npc npc)
        {
            int damage;
            float baseDamage = CalculateDamage(player, out Item weapon, npc);

            if (player.Flags.IsSailing && player.Inventory.TryGetEquippedItem(ItemType.Boat, out var boat))
                baseDamage += ExtensionMethods.RandomNumber(boat.MinHit, boat.MaxHit);

            //If the weapon is an axe and the attacker is warrior or paladin try to critically strike
            if (weapon is not null)
                if (weapon.WeaponType == WeaponType.Axe && CombatSystemUtils.ClassWarriorOrPaladin(player.Class.ClassType))
                    if (ExtensionMethods.RandomNumber(1, 100) <= Constants.AXE_NPC_CRIT_CHANCE)
                        baseDamage *= Constants.AXE_CRIT_DAMAGE_MOD;

            damage = Mathf.RoundToInt(baseDamage) - npc.Info.Defense;
            if (damage < 0) damage = 0;

            if (!TryToStab(player, weapon, damage, targetNpc: npc))
            {
                //TODO Play hit animation and sound
                PacketSender.SendMultiMessage(player.Id, MultiMessage.PlayerHitNpc,  stackalloc[] {damage});
                CalculateXpGain(player, npc, damage);
                npc.Health.TakeDamage(damage, () => npc.Kill(player));
            }

            if (npc.Health.CurrentHealth > 0)
            {
                
            }
            else
            {
                //TODO check if drake and mata drako equipped?
            }
        }

        /// <summary>Makes the npc damage the player.</summary>
        private static void NpcDamagesPlayer(Npc npc, Player player)
        {
            int damage, bodyPart, headDefense = 0, bodyDefense = 0, boatDefense = 0;

            damage = ExtensionMethods.RandomNumber(npc.Info.MinHit, npc.Info.MaxHit);

            if (player.Flags.IsSailing && player.Inventory.TryGetEquippedItem(ItemType.Boat, out var boat))
                boatDefense = ExtensionMethods.RandomNumber(boat.MinDef, boat.MaxDef);

            bodyPart = ExtensionMethods.RandomNumber(1, 6);

            if ((BodyPart)bodyPart == BodyPart.Head)
            {
                if (player.Inventory.TryGetEquippedItem(ItemType.Helmet, out var helmet))
                    headDefense = ExtensionMethods.RandomNumber(helmet.MinDef, helmet.MaxDef);
            }
            else
            {
                int minDef = 0, maxDef = 0;

                if (player.Inventory.TryGetEquippedItem(ItemType.Armor, out var armor))
                {
                    minDef = armor.MinDef;
                    maxDef = armor.MaxDef;
                }

                if (player.Inventory.TryGetEquippedItem(ItemType.Shield, out var shield))
                {
                    minDef += shield.MinDef;
                    maxDef += shield.MaxDef;
                }

                bodyDefense = ExtensionMethods.RandomNumber(minDef, maxDef);
            }

            damage -= headDefense + bodyDefense + boatDefense;

            if (damage < 1) damage = 1;

            PacketSender.SendMultiMessage(player.Id, MultiMessage.NpcHitPlayer,  stackalloc[] {bodyPart, damage});
            //TODO Play animations and sounds

            //Check for GM privilege?

            player.Health.TakeDamage(damage, player.Die);
            PacketSender.PlayerIndividualResource(player, Resource.Health);

            if (player.Flags.IsMeditating)
            {
                if (damage > (player.Health.CurrentHealth / 100 * player.Attributes[Attribute.Intellect] * player.Skills[Skill.Meditation] / 100 * 12 / ExtensionMethods.RandomNumber(1, 5) + 7))
                {
                    player.Flags.IsMeditating = false;
                    PacketSender.UpdatePlayerStatus(player, PlayerStatus.Meditate);
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.StoppedMeditating);
                }
            }

            if (player.Health.CurrentHealth <= 0)
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NpcKilledPlayer);
        }

        private static void NpcDamagesNpc(Npc attacker, Npc target)
        {
            float damage = ExtensionMethods.RandomNumber(attacker.Info.MinHit, attacker.Info.MaxHit);
            damage *= attacker.IsPet ? attacker.Info.PetAttackMod : 1;
            target.Health.TakeDamage((int)damage, () => target.Kill(attacker.PetOwner));
        }

        private static float CalculateDamage(Player player, out Item weapon, Npc npc = null)
        {
            weapon = null;
            float classMod;
            int wepDamage, minWepDamage, minHit, maxHit;

            //Check whether the player is using a weapon
            if (player.Inventory.TryGetEquippedItem(ItemType.Weapon, out weapon))
            {
                minHit = weapon.MinHit;
                maxHit = weapon.MaxHit;

                if (npc is not null) //Attacks npc
                {
                    if (weapon.IsRangedWeapon)
                    {
                        classMod = player.Class.ModRangedDamage;
                        wepDamage = ExtensionMethods.RandomNumber(minHit, maxHit);

                        if (player.Inventory.TryGetEquippedItem(ItemType.Arrow, out var arrow))
                        {
                            wepDamage += ExtensionMethods.RandomNumber(arrow.MinHit, arrow.MaxHit);
                            //Check if its a paralyzing arrow and if the npc can be paralyzed
                            if (arrow.Paralyzes && npc.Info.CanBeParalyzed)
                                npc.Flags.IsParalyzed = true;
                        }
                    }
                    else
                    {
                        classMod = player.Class.ModWeaponDamage;
                        //TODO mata draco??
                        wepDamage = ExtensionMethods.RandomNumber(minHit, maxHit);
                    }
                }
                else //Attacks user
                {
                    if (weapon.IsRangedWeapon)
                    {
                        classMod = player.Class.ModRangedDamage;
                        wepDamage = ExtensionMethods.RandomNumber(minHit, maxHit);

                        if (player.Inventory.TryGetEquippedItem(ItemType.Arrow, out var arrow))
                            wepDamage += ExtensionMethods.RandomNumber(arrow.MinHit, arrow.MaxHit);
                    }
                    else
                    {
                        classMod = player.Class.ModWeaponDamage;
                        //TODO mata draco??
                        wepDamage = ExtensionMethods.RandomNumber(minHit, maxHit);
                    }
                }
            }
            else //Doesn't have wep equipped
            {
                classMod = player.Class.ModUnarmedDamage;
                minWepDamage = 4;
                maxHit = 9;

                wepDamage = ExtensionMethods.RandomNumber(minWepDamage, maxHit);
            }

            return (3 * wepDamage + (maxHit / 5 * Math.Max(0, player.Attributes[Attribute.Strength] - 15)) + player.Hit) * classMod;
        }
    }
}
