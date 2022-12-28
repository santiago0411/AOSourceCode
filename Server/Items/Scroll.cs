using System;
using System.Linq;
using AO.Core;
using AO.Core.Utils;
using AO.Players;
using PacketSender = AO.Network.PacketSender;

namespace AO.Items
{
	public class Scroll : Item
	{
		public Scroll(ItemInfo itemInfo) 
			: base(itemInfo)
		{
		}
		
		public override bool Use(Player player)
		{
			var spell = GameManager.Instance.GetSpell(SpellIndex);
			if (!player.Spells.Contains(spell))
			{
				int emptyIndex = Array.FindIndex(player.Spells, x => x is null);
				if (emptyIndex == -1)
				{
					PacketSender.SendMultiMessage(player.Id, MultiMessage.CantLearnMoreSpells);
					return false;
				}
				else
				{
					player.Spells[emptyIndex] = spell;
					PacketSender.UpdatePlayerSpell(player.Id, (byte)emptyIndex, SpellIndex);
					return true;
				}
			}

			PacketSender.SendMultiMessage(player.Id, MultiMessage.SpellAlreadyLearned);
			return false;
		}
	}
}
