using System.Collections.Generic;
using AO.Core.Ids;
using UnityEngine;
using AO.Core.Utils;
using AO.Items;
using AO.Npcs;
using AO.Players;
using AO.Systems.Combat;
using PacketSender = AO.Network.PacketSender;

namespace AO.Spells
{
    public abstract class Spell
    {
        public readonly SpellId Id;
        protected readonly ushort Particle;
        protected readonly ushort ManaRequired;
        protected readonly ushort StamRequired;
        protected readonly bool UsesMagicItemBonus;

        private readonly SpellTarget spellTarget;
        private readonly byte minSkill;
        private readonly byte magicItemPowerNeeded;

        /// <summary>Tries to make the player cast the spell on another player.</summary>
        public abstract void PlayerCastOnPlayer(Player caster, Player target);
        /// <summary>Tries to make the player cast the spell on an npc.</summary>
        public abstract void PlayerCastOnNpc(Player caster, Npc target);
        /// <summary>Tries to make an npc cast the spell on a player.</summary>
        public abstract void NpcCastOnPlayer(Npc caster, Player target);
        /// <summary>Tries to make an npc cast the spell on another npc.</summary>
        public abstract void NpcCastOnNpc(Npc caster, Npc target);

        protected Spell(SpellInfo spellInfo)
        {
            (Id, spellTarget, Particle, minSkill, ManaRequired, StamRequired, UsesMagicItemBonus,
                magicItemPowerNeeded) = spellInfo;
        }
        
        public static Spell CreateNewSpell(SpellInfo spellInfo, List<SpellPropertyInfo> properties)
        {
            Spell spell;

            switch (spellInfo.Type)
            {
                case SpellType.Damages:
                    spell = new DamageSpell(spellInfo, properties);
                    break;
                case SpellType.Heals:
                    spell = new HealingSpell(spellInfo, properties);
                    break;
                case SpellType.ModsUser:
                    spell = new ModSpell(spellInfo, properties);
                    break;
                case SpellType.SummonNpc:
                case SpellType.Metamorphosis:
                default:
                    spell = null;
                    break;
            }

            return spell;
        }

        /// <summary>
        /// Checks whether the caster meets all the conditions to cast the spell.
        /// </summary>
        protected bool CanCast(Player caster, SpellTarget target, Vector2 targetPosition)
        {
            if (caster.Flags.IsDead)
            {
                PacketSender.SendMultiMessage(caster.Id, MultiMessage.CantCastDead);
                return false;
            }
            if (caster.Skills[Skill.Magic] < minSkill)
            {
                PacketSender.SendMultiMessage(caster.Id, MultiMessage.NotEnoughSkillToCast);
                return false;
            }
            if (magicItemPowerNeeded > 0)
            {
                if (caster.Class.ClassType == ClassType.Mage)
                {
                    if (!caster.Inventory.TryGetEquippedItem(ItemType.Weapon, out var weapon))
                    {
                        PacketSender.SendMultiMessage(caster.Id, MultiMessage.StaffNotEquipped);
                        return false;
                    }
                    if (weapon.MagicPower < magicItemPowerNeeded)
                    {
                        PacketSender.SendMultiMessage(caster.Id, MultiMessage.StaffNotPowerfulEnough);
                        return false;
                    }
                }
                else
                {
                    if (!caster.Inventory.TryGetEquippedItem(ItemType.Ring, out var ring))
                    {
                        PacketSender.SendMultiMessage(caster.Id, MultiMessage.MagicItemNotEquipped);
                        return false;
                    }
                    if (ring.MagicPower < magicItemPowerNeeded)
                    {
                        PacketSender.SendMultiMessage(caster.Id, MultiMessage.MagicItemNotPowerfulEnough);
                        return false;
                    }
                }
            }
            if (caster.Mana.CurrentAmount < ManaRequired)
            {
                PacketSender.SendMultiMessage(caster.Id, MultiMessage.NotEnoughMana);
                return false;
            }
            if (caster.Stamina.CurrentAmount < StamRequired)
            {
                PacketSender.SendMultiMessage(caster.Id, MultiMessage.NotEnoughStaminaToCast);
                return false;
            }

            if ((targetPosition.y - caster.transform.position.y) > Constants.VISION_RANGE_Y)
            {
                PacketSender.SendMultiMessage(caster.Id, MultiMessage.TooFarToCast);
                return false;
            }

            switch (spellTarget)
            {
                case SpellTarget.User:
                    if (target != SpellTarget.User)
                    {
                        PacketSender.SendMultiMessage(caster.Id, MultiMessage.UsersOnlySpell);
                        return false;
                    }
                    break;
                case SpellTarget.Npc:
                    if (target != SpellTarget.Npc)
                    {
                        PacketSender.SendMultiMessage(caster.Id, MultiMessage.NpcsOnlySpell);
                        return false;
                    }
                    break;
                case SpellTarget.Both:
                    if (target != SpellTarget.User && target != SpellTarget.Npc)
                    {
                        PacketSender.SendMultiMessage(caster.Id, MultiMessage.InvalidTarget);
                        return false;
                    }
                    break;
                case SpellTarget.Terrain:
                    break;
            }

            return true;
        }

        /// <summary>
        /// Sends the players the casted spell information. (Console messages, sounds, particles, etc)
        /// </summary>
        protected void InfoSpell(Player caster, Player target = null, Npc npcTarget = null)
        {
            PacketSender.SayMagicWords(caster, Id);

            if (target is not null)
            {
                PacketSender.CreateParticlePlayer(Particle, target);

                if (caster.Id == target.Id)
                {
                    PacketSender.SendMultiMessage(caster.Id, MultiMessage.SpellSelfMessage,  stackalloc int[] {Id.AsPrimitiveType()});
                }
                else
                {
                    PacketSender.SendMultiMessage(caster.Id, MultiMessage.SpellMessage, stackalloc[] {Id.AsPrimitiveType(), target.Id.AsPrimitiveType()});
                    PacketSender.SendMultiMessage(target.Id, MultiMessage.SpellTargetMessage, stackalloc[] {Id.AsPrimitiveType(), caster.Id.AsPrimitiveType()});
                }
            }
            else if (npcTarget is not null)
            {
                PacketSender.CreateParticleNpc(Particle, npcTarget);
            }
            else
            {
                PacketSender.CreateParticlePlayer(Particle, caster);
            }
            //play sound
            
        }

        protected static bool CanSupportPlayer(Player caster, Player target)
        {
            if (caster == target)
                return true;

            if (CombatSystemUtils.BothInArena(caster, target))
                return true;

            switch (caster.Faction)
            {
                case Faction.Imperial when (target.Faction & Constants.CRIMINAL_CHAOS) == target.Faction:
                    PacketSender.SendMultiMessage(caster.Id, MultiMessage.ImperialCantHelpCriminal);
                    return false;
                
                case Faction.Citizen when (target.Faction & Constants.CRIMINAL_CHAOS) == target.Faction:
                    if (caster.Flags.SafeToggleOn)
                    {
                        PacketSender.SendMultiMessage(caster.Id, MultiMessage.HelpCriminalsToggleSafeOff);
                        return false;
                    }
                    
                    PlayerMethods.ChangePlayerFaction(caster, Faction.Criminal);
                    return true;
                
                case Faction.Chaos when (target.Faction & Constants.CITIZEN_IMPERIAL) == target.Faction:
                    PacketSender.SendMultiMessage(caster.Id, MultiMessage.ChaosCantHelpCitizen);
                    return false;
                
                case Faction.Criminal:
                    return true;
                default:
                    return false;
            }
        }

        protected static bool CanSupportNpc(Player caster, Npc target)
        {
            Player owner = target.Flags.CombatOwner;

            if (!owner)
                return true;

            if (caster == owner)
                return true;

            if (caster.Flags.ZoneType == World.ZoneType.Arena)
                return true;

            switch (caster.Faction)
            {
                case Faction.Chaos when owner.Faction == Faction.Chaos:
                case Faction.Imperial when owner.Faction == Faction.Imperial:
                    PacketSender.SendMultiMessage(caster.Id, MultiMessage.CantHelpNpcFaction);
                    return false;
                
                case Faction.Imperial when owner.Faction == Faction.Citizen:
                    PacketSender.SendMultiMessage(caster.Id, MultiMessage.CantHelpNpcCitizen);
                    return false;
                
                case Faction.Citizen when (owner.Faction & Constants.CITIZEN_IMPERIAL) == owner.Faction:
                    if (caster.Flags.SafeToggleOn)
                    {
                        PacketSender.SendMultiMessage(caster.Id, MultiMessage.HelpNpcsToggleSafeOff);
                        return false;
                    }
                    
                    PlayerMethods.ChangePlayerFaction(caster, Faction.Criminal);
                    return true;
                
                case Faction.Criminal:
                    return true;
                default:
                    return false;
            }
        }
    }
}
