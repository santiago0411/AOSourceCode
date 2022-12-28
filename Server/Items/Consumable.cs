using System;
using System.Collections;
using AO.Core;
using AO.Core.Utils;
using AO.Players;
using UnityEngine;
using Attribute = AO.Players.Attribute;
using PacketSender = AO.Network.PacketSender;

namespace AO.Items
{
	public class Consumable : Item
	{
		private readonly Action<Player> useItem;

		public Consumable(ItemInfo itemInfo, ConsumableType consumableType) 
			: base(itemInfo)
		{
			switch (consumableType)
			{
				case ConsumableType.Food:
					useItem = UseFood;
					break;
				case ConsumableType.Drink:
					useItem = UseDrink;
					break;
				case ConsumableType.EmptyBottle:
					useItem = UseEmptyBottle;
					break;
				case ConsumableType.FilledBottle:
					useItem = UseFilledBottle;
					break;
				case ConsumableType.BluePotion:
					useItem = UseBluePotion;
					break;
				case ConsumableType.RedPotion:
					useItem = UseRedPotion;
					break;
				case ConsumableType.GreenPotion:
					useItem = UseGreenPotion;
					break;
				case ConsumableType.YellowPotion:
					useItem = UseYellowPotion; 
					break;
				case ConsumableType.VioletPotion:
					useItem = UseVioletPotion;
					break;
				case ConsumableType.BlackPotion:
					useItem = UseBlackPotion;
					break;
			}
		}

		public override bool Use(Player player)
		{
			if (!Timers.PlayerCanAttackUseInterval(player, false)) return false;

            if (IsNewbie && !PlayerMethods.IsNewbie(player))
            {
	            PacketSender.SendMultiMessage(player.Id, MultiMessage.ItemOnlyNewbies);
				return false;
            }

			useItem(player);
			return true;
		}

		private void UseBluePotion(Player player)
		{
			ushort newMana = Convert.ToUInt16((player.Mana.MaxAmount * 4 / 100) + (player.Level / 2) + (MinModifier / player.Level));
			player.Mana.AddResource(newMana);
			PacketSender.PlayerIndividualResource(player, Resource.Mana);
		}

		private void UseRedPotion(Player player)
		{
			player.Health.Heal(ExtensionMethods.RandomNumber(MinModifier, MaxModifier + 1));
			PacketSender.PlayerIndividualResource(player, Resource.Health);
		}

		private void UseFood(Player player)
		{
			player.Hunger.AddResource((ushort)MaxModifier);
			player.Flags.IsHungry = player.Hunger.CurrentAmount <= 0;
			PacketSender.PlayerIndividualResource(player, Resource.HungerAndThirst);
		}

		private void UseDrink(Player player)
		{
			player.Thirst.AddResource((ushort)MaxModifier);
			player.Flags.IsThirsty = player.Thirst.CurrentAmount <= 0;
			PacketSender.PlayerIndividualResource(player, Resource.HungerAndThirst);
		}

		private void UseGreenPotion(Player player)
		{
			player.ModifyAttribute(Attribute.Strength,  (byte)ExtensionMethods.RandomNumber(MinModifier, MaxModifier), Constants.ATTRIBUTES_BUFF_DURATION);
		}

		private void UseYellowPotion(Player player)
		{
			player.ModifyAttribute(Attribute.Agility, (byte)ExtensionMethods.RandomNumber(MinModifier, MaxModifier), Constants.ATTRIBUTES_BUFF_DURATION);
		}

		private static void UseVioletPotion(Player player)
		{
			player.Flags.IsEnvenomed = false;
		}

		private static void UseEmptyBottle(Player player)
		{

		}

		private static void UseFilledBottle(Player player)
		{

		}

		private static void UseBlackPotion(Player player)
		{
			PacketSender.SendMultiMessage(player.Id, MultiMessage.BlackPotionOne);
			player.StartCoroutine(KillPlayer(player));
		}

		private static IEnumerator KillPlayer(Player player)
        {
			yield return new WaitForSeconds(2f);
			PacketSender.SendMultiMessage(player.Id, MultiMessage.BlackPotionTwo);
			player.Health.TakeDamage(player.Health.MaxHealth, player.Die);
			PacketSender.PlayerIndividualResource(player, Resource.Health);
        }
	} 
}
