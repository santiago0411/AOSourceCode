using AO.Core;
using AO.Core.Utils;
using AO.Items;
using AO.Npcs;
using AO.Players;
using UnityEngine;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Combat
{
    public static partial class CombatSystem
    {
        public static void PlayerAttacks(Player player)
        {
            if (!Timers.PlayerCanUseBowInterval(player, false)) return;

            if (!Timers.PlayerCanAttackInterval(player))
                return;

            if (player.Stamina.CurrentAmount < 10)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.TooTiredToFight);
                return;
            }

            player.Stamina.TakeResource((ushort)ExtensionMethods.RandomNumber(1, 10));

            //Cast ray to see if the player hit something
            RaycastHit2D hit = CollisionManager.CheckLinearCollision(player.CurrentTile.Position + player.Facing.Direction, player.Facing.Direction, CollisionManager.PlayerAndNpcLayerMask);

            if (!hit) return;

            //Check if its a user or an npc
            if (hit.collider.gameObject.layer == Layer.Player.Id)
            {
                PlayerAttacksPlayer(player, hit.collider.GetComponent<Player>());
            }
            else if (hit.collider.gameObject.layer == Layer.Npc.Id)
            {
                PlayerAttacksNpc(player, hit.collider.GetComponent<Npc>());
            }

            //TODO play swing sound
        }

        public static void PlayerRangedAttack(Player player, Collider2D collision)
        {
            if (!Timers.PlayerCanAttackInterval(player, false))
                return;
            if (!Timers.PlayerCanCastSpellInterval(player, false))
                return;
            if (!Timers.PlayerCanUseBowInterval(player))
                return;
            
            //Check if the player has a weapon equipped and if it is a bow
            if (!player.Inventory.TryGetEquippedItem(ItemType.Weapon, out var weapon) && !weapon.IsRangedWeapon) 
                return;
            
            //Check if the player has arrows equipped
            if (!player.Inventory.TryGetEquippedItemSlot(ItemType.Arrow, out var arrowsSlot)) 
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NoAmmo);
                return;
            }

            if (player.Stamina.CurrentAmount < 10) //Check they have enough stamina
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.TooTiredToFight);
                return;
            }

            bool attacked = true; //Starts true because if the player hits nothing they lose an arrow

            if (collision) //Avoid null pointer
            {
                if (collision.gameObject.layer == Layer.Player.Id)
                {
                    var targetPlayer = collision.GetComponent<Player>();

                    if ((targetPlayer.transform.position.y - player.transform.position.y) > Constants.VISION_RANGE_Y)
                    {
                        PacketSender.SendMultiMessage(player.Id, MultiMessage.TooFarToAttack);
                        return;
                    }

                    if (targetPlayer == player)
                    {
                        PacketSender.SendMultiMessage(player.Id, MultiMessage.CantAttackYourself);
                        return;
                    }

                    attacked = PlayerAttacksPlayer(player, targetPlayer);
                }
                else if (collision.gameObject.layer == Layer.Npc.Id)
                {
                    var targetNpc = collision.GetComponent<Npc>();
                    var distanceToNpc = targetNpc.transform.position - player.transform.position;

                    if (distanceToNpc.x > Constants.VISION_RANGE_X && distanceToNpc.y > Constants.VISION_RANGE_Y)
                    {
                        PacketSender.SendMultiMessage(player.Id, MultiMessage.TooFarToAttack);
                        return;
                    }

                    attacked = PlayerAttacksNpc(player, targetNpc);
                }
            }

            if (attacked)
            {
                player.Stamina.TakeResource((ushort)ExtensionMethods.RandomNumber(1, 10));
                player.Inventory.RemoveQuantityFromSlot(arrowsSlot.Slot, 1);
            }
        }

        public static void NpcAttacksPlayer(Npc npc, Player player)
        {
            //TODO check if player is GM

            if (npc.Info.Sounds.Count > 0)
            {
                int randomSound = Random.Range(0, npc.Info.Sounds.Count); //Don't use extension methods random here because Count isn't inclusive
                //TODO play 
            }
            
            // If the player has a pet, notify so that it may attack back 
            if (player.Pet)
                player.Pet.TrySetNewTarget(npc);

            if (NpcHitPlayer(npc, player))
            {
                //Play hit sound

                if (!player.Flags.IsMeditating && !player.Flags.IsSailing)
                {
                    //Play blood effect
                }

                NpcDamagesPlayer(npc, player);

                if (npc.Info.Envenoms) 
                    TryNpcEnvenomPlayer(player);
            }
            else
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NpcSwing);
                //TODO play dodged sound
            }

            PlayerMethods.TryLevelSkill(player, Skill.CombatTactics);
        }

        public static void NpcAttacksNpc(Npc attacker, Npc target)
        {
            //If a pet attacks an npc the player is in combat with it refreshes the lost npc timer
            if (attacker.IsPet && target.Flags.CombatOwner == attacker.PetOwner)
                Timers.PlayerLostNpcInterval(attacker.PetOwner, true);
            
            target.Attacked(attacker);

            if (attacker.Info.Sounds.Count > 0)
            {
                int randomSound = Random.Range(0, attacker.Info.Sounds.Count); //Don't use extension methods random here because Count isn't inclusive
                //TODO play notStaticNpc.Sounds[randomSound]
            }

            if (NpcHitNpc(attacker, target))
            {
                NpcDamagesNpc(attacker, target);
            }
            else
            {
                //TODO play dodged sound
            }
        }

        public static bool PlayerAttacksPlayer(Player attacker, Player target)
        {
            if (!CombatSystemUtils.CanPlayerAttackPlayer(attacker, target))
                return false;

            PlayerAttackedByPlayer(attacker, target);

            if (PlayerHitPlayer(attacker, target))
            {
                //TODO play hit sound

                if (!target.Flags.IsSailing)
                {
                    //TODO play blood FX
                }

                //Nudillos paralizan?

                PlayerDamagesPlayer(attacker, target);
            }
            else
            {
                //Check invisible admin
                //TODO message
                PacketSender.SendMultiMessage(attacker.Id, MultiMessage.PlayerSwing);
                PacketSender.SendMultiMessage(target.Id, MultiMessage.PlayerAttackedSwing, stackalloc[] {attacker.Id.AsPrimitiveType()});
            }

            PlayerMethods.TryLevelSkill(target, Skill.CombatTactics);
            return true;
        }

        private static bool PlayerAttacksNpc(Player player, Npc npc)
        {
            if (!CombatSystemUtils.CanPlayerAttackNpc(player, npc)) 
                return false;

            npc.Attacked(player);

            // If player has a pet notify it so it may attack the npc
            if (player.Pet)
                player.Pet.TrySetNewTarget(npc);

            if (PlayerHitNpc(player, npc))
            {
                //TODO play hit sound

                PlayerDamagesNpc(player, npc);
            }
            else
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.PlayerSwing);
                //TODO play dodged sound
            }

            return true;
        }

        public static void PlayerAttackedByPlayer(Player attacker, Player target)
        {
            //Only convert citizens to criminal if they aren't in an arena
            if (!CombatSystemUtils.BothInArena(attacker, target))
            {
                if (attacker.Faction == Faction.Citizen && (target.Faction & Constants.CITIZEN_IMPERIAL) == target.Faction)
                {
                    PacketSender.SendMultiMessage(attacker.Id, MultiMessage.CitizenAttackedCitizen);
                    PlayerMethods.ChangePlayerFaction(attacker, Faction.Criminal);
                }
            }

            if (target.Flags.IsMeditating)
            {
                target.Flags.IsMeditating = false;
                PacketSender.UpdatePlayerStatus(target, PlayerStatus.Meditate);
                PacketSender.SendMultiMessage(target.Id, MultiMessage.StoppedMeditating);
            }
            
            // If the players have pets notify them so they may attack back
            if (attacker.Pet)
                attacker.Pet.TrySetNewTarget(target);

            if (target.Pet)
                target.Pet.TrySetNewTarget(attacker);

            target.Flags.Disconnecting = false;
        }

        /// <summary>Calculates the experience the player shall receive based on the amount of damage done.</summary>
        public static void CalculateXpGain(Player player, Npc npc, int damage)
        {
            if (npc.Health.MaxHealth <= 0) return;
            if (damage > npc.Health.CurrentHealth) 
                damage = npc.Health.CurrentHealth;

            int xpToGive = Mathf.RoundToInt(damage * ((float)npc.Info.XpAmount / npc.Health.MaxHealth));

            if (xpToGive <= 0) return;

            if (xpToGive >= npc.Flags.ExperienceCount)
            {
                xpToGive = npc.Flags.ExperienceCount;
                npc.Flags.ExperienceCount = 0;
            }
            else
            {
                npc.Flags.ExperienceCount -= xpToGive;
            }

            if (player.Party is null)
            {
                PlayerMethods.AddExperience(player, (uint)xpToGive);
                return;
            }
            
            player.Party.DistributeNpcExperience(npc.CurrentTile.Position, (uint)xpToGive);
        }
    }
}