using System;
using System.Collections.Generic;
using UnityEngine;
using AO.Core.Utils;
using AO.Items;
using AO.Npcs;
using AO.Npcs.AI;
using AO.Players;
using AO.Systems.Combat;
using PacketSender = AO.Network.PacketSender;

namespace AO.Spells
{
	public class DamageSpell : Spell
	{
		private readonly int minMod;
		private readonly int maxMod;
		private readonly bool mageIgnoresMr;

		public DamageSpell(SpellInfo spellInfo, List<SpellPropertyInfo> properties)
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
					case SpellProperty.MageIgnoresMr:
						mageIgnoresMr = true;
						break;
				}
			}
		}

		public override void PlayerCastOnPlayer(Player caster, Player target)
		{
			if (!CanCast(caster, SpellTarget.User, target.transform.position)) 
				return;
			
			if (caster.Id == target.Id)
			{
				PacketSender.SendMultiMessage(caster.Id, MultiMessage.CantAttackYourself);
				return;
			}

			if (!CombatSystemUtils.CanPlayerAttackPlayer(caster, target))
				return;

			float damage = ExtensionMethods.RandomNumber(minMod, maxMod);
			damage += ExtensionMethods.Percentage(damage, 3 * caster.Level);

			if (UsesMagicItemBonus)
				damage *= GetDamageBonus(caster);

			if (!(mageIgnoresMr && caster.Class.ClassType == ClassType.Mage))
				damage -= ExtensionMethods.Percentage(damage, ((target.Skills[Skill.MagicResistance] + 1) / 4f) + target.Class.ModMagicResist);

			if (damage < 0f) 
				damage = 0f;

			int intDamage = Mathf.RoundToInt(damage);

			CombatSystem.PlayerAttackedByPlayer(caster, target);

			InfoSpell(caster, target);

			caster.Mana.TakeResource(ManaRequired);
			caster.Stamina.TakeResource(StamRequired);
			PacketSender.PlayerResources(caster);

			target.Health.TakeDamage(intDamage, target.Die);
			PacketSender.PlayerIndividualResource(target, Resource.Health);
			
			PacketSender.SendMultiMessage(caster.Id, MultiMessage.PlayerDamageSpellEnemy,  stackalloc[] {target.Id.AsPrimitiveType(), intDamage});
			PacketSender.SendMultiMessage(target.Id, MultiMessage.EnemyDamageSpellPlayer,  stackalloc[] {caster.Id.AsPrimitiveType(), intDamage});

			PlayerMethods.TryLevelSkill(caster, Skill.Magic);
			PlayerMethods.TryLevelSkill(target, Skill.MagicResistance);
					
			if (target.Health.CurrentHealth <= 0)
				PlayerMethods.AddDeath(caster, target);
		}

		public override void PlayerCastOnNpc(Player caster, Npc target)
		{
			if (!CanCast(caster, SpellTarget.Npc, target.transform.position))
				return;
			
			if (!CombatSystemUtils.CanPlayerAttackNpc(caster, target))
				return;
			
			target.Attacked(caster);

			float damage = ExtensionMethods.RandomNumber(minMod, maxMod);
			damage += ExtensionMethods.Percentage(damage, 3 * caster.Level);

			if (UsesMagicItemBonus)
				damage *= GetDamageBonus(caster);

			InfoSpell(caster, npcTarget: target);

			int intDamage = Mathf.RoundToInt(damage);
			intDamage -= target.Info.MagicDefense;

			caster.Mana.TakeResource(ManaRequired);
			caster.Stamina.TakeResource(StamRequired);
			PacketSender.PlayerResources(caster);

			target.Health.TakeDamage(intDamage, () => target.Kill(caster));
			PacketSender.SendMultiMessage(caster.Id, MultiMessage.PlayerDamageSpellNpc,  stackalloc[] {intDamage});
			CombatSystem.CalculateXpGain(caster, target, intDamage);

			PlayerMethods.TryLevelSkill(caster, Skill.Magic);
		}

		public override void NpcCastOnPlayer(Npc caster, Player target)
		{
			if (target.Flags.IsHidden || target.Flags.IsInvisible) 
				return;
			
			if (target.Pet)
				target.Pet.TrySetNewTarget(caster);
			
			float damage = ExtensionMethods.RandomNumber(minMod, maxMod);
			damage -= ExtensionMethods.Percentage(damage, ((target.Skills[Skill.MagicResistance] + 1) / 4f) + target.Class.ModMagicResist);

			if (damage < 0) damage = 0;
			int intDamage = Mathf.RoundToInt(damage);
			//TODO play animation and sound

			target.Health.TakeDamage(intDamage, target.Die);
			PacketSender.PlayerIndividualResource(target, Resource.Health);
			PacketSender.SendMultiMessage(target.Id, MultiMessage.NpcDamageSpellPlayer,  stackalloc[] {caster.Info.Id.AsPrimitiveType(), Id.AsPrimitiveType(), intDamage});
			PacketSender.CreateParticlePlayer(Particle, target);
			PlayerMethods.TryLevelSkill(target, Skill.MagicResistance);
		}

		public override void NpcCastOnNpc(Npc caster, Npc target)
		{
			target.Attacked(caster);
			
			float damage = ExtensionMethods.RandomNumber(minMod, maxMod);
			damage *= caster.IsPet ? caster.Info.PetSpellMod : 1;
			//TODO play animation and sound

			target.Health.TakeDamage((int)damage, () => target.Kill(caster.PetOwner));
			PacketSender.CreateParticleNpc(Particle, target);
		}
		
		private static float GetDamageBonus(Player caster)
		{
			if (caster.Class.ClassType == ClassType.Mage)
			{
				// Use staff as damage bonus for mages
				if (caster.Inventory.TryGetEquippedItem(ItemType.Weapon, out var staff))
					return (staff.MagicDamageBonus + 70) / 100f;
				
				return 0.7f;
			}
			
			// Use ring (magical item) for every other class
			if (caster.Inventory.TryGetEquippedItem(ItemType.Ring, out var ring))
				return (ring.MagicDamageBonus + 70) / 100f;

			return 0.7f;
		}
	}
}
