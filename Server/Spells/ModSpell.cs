using System;
using System.Collections.Generic;
using UnityEngine;
using AO.Core.Utils;
using AO.Npcs;
using AO.Npcs.AI;
using AO.Players;
using AO.Systems.Combat;
using Attribute = AO.Players.Attribute;
using PacketSender = AO.Network.PacketSender;

namespace AO.Spells
{
	public class ModSpell : Spell
	{
		private readonly int minMod;
		private readonly int maxMod;
		private readonly SpellModType modType;

		public ModSpell(SpellInfo spellInfo, List<SpellPropertyInfo> properties)
			: base(spellInfo)
		{
			foreach (var (property, value) in properties)
			{
				switch (property)
				{
					case SpellProperty.MinMod:
						minMod = Convert.ToInt32(value);
						break;
					case SpellProperty.MaxMod:
						maxMod = Convert.ToInt32(value);
						break;
					case SpellProperty.ModType:
						modType = (SpellModType)Convert.ToInt32(value);
						break;
				}
			}
		}

		public override void PlayerCastOnPlayer(Player caster, Player target)
		{
			if (!CanCast(caster, SpellTarget.User, target.transform.position))
				return;
			
			if (target.Flags.IsDead && modType != SpellModType.Revive)
			{
				PacketSender.SendMultiMessage(caster.Id, MultiMessage.CantCastOnSpirit);
				return;
			}

			bool success = modType switch
			{
				SpellModType.Strength => Strength(caster, target),
				SpellModType.Agility => Agility(caster, target),
				SpellModType.Invisibility => Invisibility(caster, target),
				SpellModType.Paralyze => ParalyzePlayer(caster, target, true),
				SpellModType.Immobilize => ParalyzePlayer(caster, target, false),
				SpellModType.RemovesParalysis => RemoveParalysisPlayer(caster, target),
				SpellModType.Envenom => Envenom(caster, target),
				SpellModType.RemovesEnvenomed => RemoveEnvenomed(caster, target),
				SpellModType.Revive => Revive(caster, target),
				_ => true
			};

			if (success)
			{
				InfoSpell(caster, target);

				caster.Mana.TakeResource(ManaRequired);
				caster.Stamina.TakeResource(StamRequired);
				PacketSender.PlayerResources(caster);

				PlayerMethods.TryLevelSkill(caster, Skill.Magic);
			}
		}

		public override void PlayerCastOnNpc(Player caster, Npc target)
		{
			if (!CanCast(caster, SpellTarget.Npc, target.transform.position))
				return;

			bool success = modType switch
			{
				SpellModType.Paralyze => ParalyzeNpc(caster, target),
				SpellModType.Immobilize => ImmobilizeNpc(caster, target),
				SpellModType.RemovesParalysis => RemoveParalysisNpc(caster, target),
				_ => false
			};

			if (success)
			{
				InfoSpell(caster, npcTarget: target);

				caster.Mana.TakeResource(ManaRequired);
				caster.Stamina.TakeResource(StamRequired);
				PacketSender.PlayerResources(caster);

				PlayerMethods.TryLevelSkill(caster, Skill.Magic);
			}
		}
		
		// If you ever extend this function make sure to call target.Pet.TrySetNewTarget(target); on the appropriate mods
		public override void NpcCastOnPlayer(Npc caster, Player target) { }

		// If you ever extend this function make sure to call target.Attacked(caster); on the appropriate mods
		public override void NpcCastOnNpc(Npc caster, Npc target) { }

		private bool Strength(Player caster, Player target)
		{
			if (!CanSupportPlayer(caster, target))
				return false;

			target.ModifyAttribute(Attribute.Strength, (byte)ExtensionMethods.RandomNumber(minMod, maxMod), Constants.ATTRIBUTES_BUFF_DURATION);
			return true;
		}

		private bool Agility(Player caster, Player target)
		{
			if (!CanSupportPlayer(caster, target))
				return false;

			target.ModifyAttribute(Attribute.Agility, (byte)ExtensionMethods.RandomNumber(minMod, maxMod), Constants.ATTRIBUTES_BUFF_DURATION);
			return true;
		}

		private static bool RemoveEnvenomed(Player caster, Player target)
		{
			if (!CanSupportPlayer(caster, target))
				return false;

			target.Flags.IsEnvenomed = false;
			return true;
		}

		private static bool Invisibility(Player caster, Player target)
		{
			//Check map??
			if (!CanSupportPlayer(caster, target))
				return false;

			if (!target.Flags.CanInvis)
				return false;


			target.Flags.IsInvisible = true;
			return true;
		}

		private static bool Envenom(Player caster, Player target)
		{
			if (caster == target)
			{
				PacketSender.SendMultiMessage(caster.Id, MultiMessage.CantAttackYourself);
				return false;
			}

			if (!CombatSystemUtils.CanPlayerAttackPlayer(caster, target))
				return false;

			CombatSystem.PlayerAttackedByPlayer(caster, target);
			target.Flags.IsEnvenomed = true;
			return true;
		}

		private static bool Revive(Player caster, Player target)
		{
			if (!CanSupportPlayer(caster, target))
				return false;

            if (!target.Flags.RessToggleOn)
            {
				target.Thirst.TakeResource(target.Thirst.MaxAmount);
				target.Flags.IsThirsty = true;
				target.Hunger.TakeResource(target.Hunger.MaxAmount);
				target.Flags.IsHungry = true;
				target.Stamina.TakeResource(target.Stamina.MaxAmount);
				target.Mana.TakeResource(target.Mana.MaxAmount);
				PacketSender.PlayerResources(target);

				target.Revive();

				return true;
            }

			PacketSender.SendMultiMessage(caster.Id, MultiMessage.TargetRessToggledOff);
			return false;
		}

		private static bool ParalyzePlayer(Player caster, Player target, bool paralyze)
		{
			if (caster == target)
			{
				PacketSender.SendMultiMessage(caster.Id, MultiMessage.CantAttackYourself);
				return false;
			}

			if (target.Flags.IsParalyzed && target.Flags.IsImmobilized) 
				return false;
			
			if (!CombatSystemUtils.CanPlayerAttackPlayer(caster, target))
				return false;

			CombatSystem.PlayerAttackedByPlayer(caster, target);

			target.Flags.IsParalyzed = paralyze;
			target.Flags.IsImmobilized = !paralyze;
			target.Timers.ParalyzedTime = Time.realtimeSinceStartup;

			return true;

		}

		private static bool RemoveParalysisPlayer(Player caster, Player target)
		{
			if (!CanSupportPlayer(caster, target))
				return false;

			if (!target.Flags.IsParalyzed && !target.Flags.IsImmobilized)
				return false;
			
			target.Flags.IsParalyzed = false;
			target.Flags.IsImmobilized = false;
			return true;

		}

		private static bool ParalyzeNpc(Player caster, Npc target)
		{
			if (!target.Info.CanBeParalyzed)
			{
				PacketSender.SendMultiMessage(caster.Id, MultiMessage.NpcImmuneToSpell);
				return false;
			}

			if (!CombatSystemUtils.CanPlayerAttackNpc(caster, target, true))
				return false;
			
			target.Attacked(caster);
			target.Flags.IsParalyzed = true;
			target.Flags.IsImmobilized = false;
			target.Flags.ParalyzedTime = Time.realtimeSinceStartup;
			return true;
		}

		private static bool ImmobilizeNpc(Player caster, Npc target)
		{
			if (!target.Info.CanBeParalyzed)
			{
				PacketSender.SendMultiMessage(caster.Id, MultiMessage.NpcImmuneToSpell);
				return false;
			}
			
			if (!CombatSystemUtils.CanPlayerAttackNpc(caster, target, true))
				return false;
			
			target.Attacked(caster);
			target.Flags.IsImmobilized = true;
			target.Flags.IsParalyzed = false;
			target.Flags.ParalyzedTime = Time.realtimeSinceStartup;
			return true;
		}

		private static bool RemoveParalysisNpc(Player caster, Npc target)
		{
			if (!CanSupportNpc(caster, target))
				return false;

			if (!target.Flags.IsParalyzed && !target.Flags.IsImmobilized) 
				return false;
			
			target.Flags.IsParalyzed = false;
			target.Flags.IsImmobilized = false;
			return true;
		}
	}
}
