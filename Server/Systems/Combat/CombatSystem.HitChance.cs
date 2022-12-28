using System;
using AO.Core.Utils;
using AO.Items;
using AO.Npcs;
using AO.Npcs.AI;
using AO.Players;
using UnityEngine;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Combat
{
    public static partial class CombatSystem
    {
        private static bool PlayerHitPlayer(Player attacker, Player target)
        {
            float attackPower, dodgeChance;

            Skill skill;

            byte tacticSkill = target.Skills[Skill.CombatTactics];
            byte shieldSkill = target.Skills[Skill.ShieldDefense];
            bool targetUsingShield = target.Inventory.HasItemEquipped(ItemType.Shield);
            bool weaponIsMaze = false;

            //Check if attacker is using a weapon
            if (attacker.Inventory.TryGetEquippedItem(ItemType.Weapon, out var weapon))
            {
                weaponIsMaze = weapon.WeaponType == WeaponType.Maze;

                //Check if the weapon is ranged
                if (weapon.IsRangedWeapon)
                {
                    attackPower = RangedAttack(attacker);
                    skill = Skill.RangedWeapons;
                }
                else
                {
                    attackPower = WeaponAttack(attacker);
                    skill = Skill.ArmedCombat;
                }
            }
            else
            {
                attackPower = UnarmedAttack(attacker);
                skill = Skill.UnarmedCombat;
            }

            float evasionPower = Evasion(target);

            //Check if the target is using a shield
            if (targetUsingShield)
            {
                float shieldEvasionPower = ShieldEvasion(target);

                //If the attacker is a warrior or paladin and is using a maze reduce shield evasion
                if (weaponIsMaze && CombatSystemUtils.ClassWarriorOrPaladin(attacker.Class.ClassType))
                    shieldEvasionPower *= Constants.MAZE_SHIELD_EVASION_MOD;

                evasionPower += shieldEvasionPower;
            }

            //Round hit chance
            int hitChance = Math.Max(10, Math.Min(90, 50 + Mathf.RoundToInt((attackPower - evasionPower) * 0.4f)));

            //If the target is meditating evasion is reduced 25%
            if (target.Flags.IsMeditating)
            {
                dodgeChance = (100 - hitChance) * 0.75f;
                hitChance = Math.Min(90, Mathf.RoundToInt(100 - dodgeChance));
            }

            bool hitSuccessful = ExtensionMethods.RandomNumber(1, 100) <= hitChance;

            if (targetUsingShield)
            {
                if (!hitSuccessful)
                {
                    int parryChance = Math.Max(10, Math.Min(90, 100 * shieldSkill / (shieldSkill + tacticSkill)));
                    bool parried = ExtensionMethods.RandomNumber(1, 100) <= parryChance;

                    if (parried)
                    {
                        PacketSender.SendMultiMessage(attacker.Id, MultiMessage.BlockedWithShieldOther);
                        PacketSender.SendMultiMessage(target.Id, MultiMessage.BlockedWithShieldPlayer);
                        //TODO Play shield parried sound and shield animation
                    }

                    PlayerMethods.TryLevelSkill(target, Skill.ShieldDefense);
                }
            }

            if (hitSuccessful)
                PlayerMethods.TryLevelSkill(attacker, skill);

            return hitSuccessful;
        }

        private static bool PlayerHitNpc(Player player, Npc npc)
        {
            float attackPower;
            Skill skill;

            //Check whether the player has a weapon equipped or not
            if (player.Inventory.TryGetEquippedItem(ItemType.Weapon, out var weapon))
            {
                if (weapon.IsRangedWeapon)
                {
                    attackPower = RangedAttack(player);
                    skill = Skill.RangedWeapons;
                }
                else
                {
                    attackPower = WeaponAttack(player);
                    skill = Skill.ArmedCombat;
                }
            }
            else
            {
                //The player has no weapons equipped
                attackPower = UnarmedAttack(player);
                skill = Skill.UnarmedCombat;
            }

            //Calculate hit chance and check if the hit was successful
            int hitChance = Math.Max(10, Math.Min(90, 50 + Mathf.RoundToInt((attackPower - npc.Info.Evasion) * 0.4f)));
            bool userHitSuccessful = ExtensionMethods.RandomNumber(1, 100) <= hitChance;

            if (userHitSuccessful)
                PlayerMethods.TryLevelSkill(player, skill);

            return userHitSuccessful;
        }

        private static bool NpcHitPlayer(Npc npc, Player player)
        {
            //Get player evasion, shield evasion, and skills on combat and shield defense
            float playerEvasion = Evasion(player);
            float shieldEvasion = ShieldEvasion(player);
            byte combatTactics = player.Skills[Skill.CombatTactics], shieldDefense = player.Skills[Skill.ShieldDefense];

            //Check whether the player is using a shield
            bool isUsingShield = player.Inventory.HasItemEquipped(ItemType.Shield);

            //Add shield evasion to normal evasion if they are
            if (isUsingShield) playerEvasion += shieldEvasion;

            //Calculate hit chance and check if the hit was successful
            int hitChance = Math.Max(10, Math.Min(90, 50 + Mathf.RoundToInt((npc.Info.AttackPower - playerEvasion) * 0.4f)));
            bool npcHitSuccessful = ExtensionMethods.RandomNumber(1, 100) <= hitChance;

            if (isUsingShield)
            {
                if (!npcHitSuccessful)
                {
                    if (combatTactics + shieldDefense > 0)
                    {
                        //If the player is using a shield and they dodged the hit calculate parry chance and pretend they parried with shield
                        int parryChance = Math.Max(10, Math.Min(90, 100 * shieldDefense / (combatTactics + shieldDefense)));
                        bool parried = ExtensionMethods.RandomNumber(1, 100) <= parryChance;

                        if (parried)
                        {
                            //TODO player shield sound
                            PlayerMethods.TryLevelSkill(player, Skill.ShieldDefense);
                            PacketSender.SendMultiMessage(player.Id, MultiMessage.BlockedWithShieldPlayer);
                        }
                    }
                }
            }

            return npcHitSuccessful;
        }

        private static bool NpcHitNpc(Npc attacker, Npc target)
        {
            int attackPower = attacker.Info.AttackPower;
            int evasion = target.Info.Evasion;

            int hitChance = Math.Max(10, Math.Min(90, Mathf.RoundToInt(50 + (attackPower - evasion) * 0.4f)));
            return UnityEngine.Random.Range(0, 100) <= hitChance;
        }
    }
}
