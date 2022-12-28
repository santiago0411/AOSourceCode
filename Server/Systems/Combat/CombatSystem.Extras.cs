using System;
using AO.Core.Utils;
using AO.Items;
using AO.Npcs;
using AO.Players;
using UnityEngine;
using Attribute = AO.Players.Attribute;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Combat
{
    public static partial class CombatSystem
    {
        private static void TryNpcEnvenomPlayer(Player player)
        {
            if (!player.Flags.IsDead && !player.Flags.IsEnvenomed)
            {
                int random = ExtensionMethods.RandomNumber(1, 100);
                if (random < 30)
                {
                    player.Flags.IsEnvenomed = true;
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.NpcEnvenomedPlayer);
                }
            }
        }

        private static void TryPlayerEnvenomPlayer(Player attacker, Player target, Item attackerWeapon)
        {
            if (attackerWeapon is null) return;

            if (attackerWeapon.IsRangedWeapon)
            {
                if (attacker.Inventory.TryGetEquippedItem(ItemType.Arrow, out var arrow))
                {
                    if (!arrow.Envenoms)
                        return;
                }
            }

            if (!attackerWeapon.Envenoms)
                return;

            if (ExtensionMethods.RandomNumber(1, 100) < 60)
            {
                target.Flags.IsEnvenomed = true;
                PacketSender.SendMultiMessage(attacker.Id, MultiMessage.AttackerEnvenomed,  stackalloc[] {target.Id.AsPrimitiveType()});
                PacketSender.SendMultiMessage(target.Id, MultiMessage.TargetGotEnvenomed,  stackalloc[] {attacker.Id.AsPrimitiveType()});
            }
        }

        private static bool TryToStab(Player attacker, Item attackerWeapon, int damage, Npc targetNpc = null, Player targetPlayer = null)
        {
            if (!PlayerMethods.CanStab(attacker, attackerWeapon))
                return false;

            int luck, skill = attacker.Skills[Skill.Stabbing];

            luck = attacker.Class.ClassType switch
            {
                ClassType.Assassin => Mathf.RoundToInt(((0.00003f * skill - 0.002f) * skill + 0.098f) * skill + 4.25f),
                ClassType.Cleric => Mathf.RoundToInt(((0.000003f * skill + 0.0006f) * skill + 0.0107f) * skill + 4.93f),
                ClassType.Paladin => Mathf.RoundToInt(((0.000003f * skill + 0.0006f) * skill + 0.0107f) * skill + 4.93f),
                ClassType.Bard => Mathf.RoundToInt(((0.000002f * skill + 0.0002f) * skill + 0.032f) * skill + 4.81f),
                _ => Mathf.RoundToInt(0.0361f * skill + 4.39f)
            };

            if (ExtensionMethods.RandomNumber(1, 100) > luck)
                return false;

            if (targetNpc is not null) //Attacking npc
            {
                //TODO Play hit animation and sound
                targetNpc.Health.TakeDamage(damage * 2, () => targetNpc.Kill(attacker));
                PacketSender.SendMultiMessage(attacker.Id, MultiMessage.StabbedNpc, stackalloc[] { damage * 2 });
                CalculateXpGain(attacker, targetNpc, damage * 2);
                return true;
            }

            //Attacking another player
            float modifier = attacker.Class.ClassType == ClassType.Assassin ? 1.4f : 1.5f;
            damage = Mathf.RoundToInt(damage * modifier);

            targetPlayer!.Health.TakeDamage(damage, targetPlayer.Die);
            PacketSender.PlayerIndividualResource(targetPlayer, Resource.Health);

            PacketSender.SendMultiMessage(attacker.Id, MultiMessage.StabbedPlayer, stackalloc[] { targetPlayer.Id.AsPrimitiveType(), damage });
            PacketSender.SendMultiMessage(targetPlayer.Id, MultiMessage.PlayerGotStabbed,  stackalloc[] {attacker.Id.AsPrimitiveType(), damage});

            PlayerMethods.TryLevelSkill(attacker, Skill.Stabbing);

            return true;
        }

        private static void TryToApplyBleed(Player attacker, Player target, Item attackerWeapon, int chance)
        {
            if (!target) 
                return;

            if (attacker.Class.ClassType != ClassType.Assassin)
                return;

            if (!attackerWeapon.AppliesBleed)
                return;

            if (ExtensionMethods.RandomNumber(1, 100) > chance)
                return;

            target.Flags.BleedingTicksRemaining = 5;
            PacketSender.SendMultiMessage(attacker.Id, MultiMessage.AttackerAppliedBleed, stackalloc[] {target.Id.AsPrimitiveType()});
            PacketSender.SendMultiMessage(target.Id, MultiMessage.TargetGotBled,  stackalloc[] {attacker.Id.AsPrimitiveType()});
        }

        /// <summary>Calculates and returns the player's shield evasion.</summary>
        private static float ShieldEvasion(Player player)
        {
            return player.Skills[Skill.ShieldDefense] * (player.Class.ModShield / 2);
        }

        /// <summary>Calculates and returns the player's evasion.</summary>
        private static float Evasion(Player player)
        {
            float aux = (player.Skills[Skill.CombatTactics] + player.Skills[Skill.CombatTactics] / 33 * player.Attributes[Attribute.Agility]) * player.Class.ModEvasion;
            return aux + (2.5f * Math.Max(player.Level - 12, 0));
        }

        /// <summary>Calculates and returns the player's weapon attack power.</summary>
        private static float WeaponAttack(Player player)
        {
            byte combatSkill = player.Skills[Skill.ArmedCombat];
            float aux;

            if (combatSkill < 31)
                aux = combatSkill * player.Class.ModWeaponAttack;
            else if (combatSkill < 61)
                aux = combatSkill + player.Attributes[Attribute.Agility] * player.Class.ModWeaponAttack;
            else if (combatSkill < 91)
                aux = combatSkill + 2 * player.Attributes[Attribute.Agility] * player.Class.ModWeaponAttack;
            else
                aux = combatSkill + 3 * player.Attributes[Attribute.Agility] * player.Class.ModWeaponAttack;

            return aux + (2.5f * Math.Max(player.Level - 12, 0));
        }

        /// <summary>Calculates and returns the player's ranged attack power.</summary>
        private static float RangedAttack(Player player)
        {
            byte rangedSkill = player.Skills[Skill.RangedWeapons];

            float aux = rangedSkill switch
            {
                < 31 => rangedSkill * player.Class.ModRangedAttack,
                < 61 => rangedSkill + player.Attributes[Attribute.Agility] * player.Class.ModRangedAttack,
                < 91 => rangedSkill + 2 * player.Attributes[Attribute.Agility] * player.Class.ModRangedAttack,
                _ => rangedSkill + 3 * player.Attributes[Attribute.Agility] * player.Class.ModRangedAttack
            };

            return aux + (2.5f * Math.Max(player.Level - 12, 0));
        }

        /// <summary>Calculates and returns the player's unarmed attack power.</summary>
        private static float UnarmedAttack(Player player)
        {
            byte unarmedSkill = player.Skills[Skill.UnarmedCombat];

            float aux = unarmedSkill switch
            {
                < 31 => unarmedSkill * player.Class.ModRangedAttack,
                < 61 => unarmedSkill + player.Attributes[Attribute.Agility] * player.Class.ModUnarmedAttack,
                < 91 => unarmedSkill + 2 * player.Attributes[Attribute.Agility] * player.Class.ModUnarmedAttack,
                _ => unarmedSkill + 3 * player.Attributes[Attribute.Agility] * player.Class.ModUnarmedAttack
            };

            return aux + (2.5f * Math.Max(player.Level - 12, 0));
        }
    }
}
