using System;
using System.Collections.Generic;
using UnityEngine;
using AO.Core.Utils;
using AO.Npcs;
using AO.Npcs.AI;
using AO.Players;
using PacketSender = AO.Network.PacketSender;

namespace AO.Spells
{
	public class HealingSpell : Spell
	{
		private readonly int minMod;
		private readonly int maxMod;

		public HealingSpell(SpellInfo spellInfo, List<SpellPropertyInfo> properties)
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
				}
			}
		}

		public override void PlayerCastOnPlayer(Player caster, Player target)
		{
			if (!CanCast(caster, SpellTarget.User, target.transform.position)) 
				return;
			
			if (!CanSupportPlayer(caster, target)) 
				return;
			
			float healing = ExtensionMethods.RandomNumber(minMod, maxMod);
			healing += ExtensionMethods.Percentage(healing, 3 * caster.Level);
			int intHealing = Mathf.RoundToInt(healing);

			InfoSpell(caster, target);

			caster.Mana.TakeResource(ManaRequired);
			caster.Stamina.TakeResource(StamRequired);
			PacketSender.PlayerResources(caster);
			
			target.Health.Heal(intHealing);
			PacketSender.PlayerIndividualResource(target, Resource.Health);

			if (caster != target)
			{
				PacketSender.SendMultiMessage(caster.Id, MultiMessage.PlayerHealed, stackalloc[] {intHealing, target.Id.AsPrimitiveType()});
				PacketSender.SendMultiMessage(target.Id, MultiMessage.PlayerGotHealed, stackalloc[] {intHealing, caster.Id.AsPrimitiveType()});
			}
			else
			{
				PacketSender.SendMultiMessage(caster.Id, MultiMessage.PlayerSelfHeal, stackalloc[] {intHealing});
			}

			PlayerMethods.TryLevelSkill(caster, Skill.Magic);
		}

		public override void PlayerCastOnNpc(Player caster, Npc target)
		{
			if (!CanCast(caster, SpellTarget.Npc, target.transform.position)) 
				return;
			
			if (!CanSupportNpc(caster, target)) 
				return;
			
			float healing = ExtensionMethods.RandomNumber(minMod, maxMod);
			healing += ExtensionMethods.Percentage(healing, 3 * caster.Level);
			int intHealing = Mathf.RoundToInt(healing);

			InfoSpell(caster, npcTarget: target);

			caster.Mana.TakeResource(ManaRequired);
			caster.Stamina.TakeResource(StamRequired);
			PacketSender.PlayerResources(caster);

			target.Health.Heal(intHealing);

			PacketSender.SendMultiMessage(caster.Id, MultiMessage.PlayerHealedNpc, stackalloc[] {intHealing});
		}

		public override void NpcCastOnPlayer(Npc caster, Player target)
		{
			int healing = ExtensionMethods.RandomNumber(minMod, maxMod);
			target.Health.Heal(healing);
			PacketSender.PlayerIndividualResource(target, Resource.Health);
			PacketSender.SendMultiMessage(target.Id, MultiMessage.NpcHealedPlayer, stackalloc[] {caster.Info.Id.AsPrimitiveType(), healing});
			PacketSender.CreateParticlePlayer(Particle, target);
		}

		public override void NpcCastOnNpc(Npc caster, Npc target)
		{
			// Npcs don't heal other npcs
		}
	}
}
