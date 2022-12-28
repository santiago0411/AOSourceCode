using UnityEngine;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Items;
using AO.Npcs;
using AO.Systems.Combat;
using PacketSender = AO.Network.PacketSender;

namespace AO.Players
{
    public static class PlayerMethods
    {
        public static void AddGold(Player player, uint amount)
        {
            player.Gold += amount;
            PacketSender.PlayerGold(player);
        }

        public static void RemoveGold(Player player, uint amount)
        {
            player.Gold -= amount;
            PacketSender.PlayerGold(player);
        }
        
        public static bool TakeStamina(Player player, ushort amount)
        {
            if (player.Stamina.CurrentAmount < amount)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NotEnoughStamina);
                player.Flags.IsWorking = false;
                return false;
            }

            player.Stamina.TakeResource(amount);
            PacketSender.PlayerIndividualResource(player, Resource.Stamina);
            return true;
        }
        
        public static bool RecoverStamina(Player player)
        {
            if (player.Stamina.CurrentAmount >= player.Stamina.MaxAmount) return false;
            
            float interval = player.Flags.IsResting ? Constants.RECOVER_STAM_RESTING_TICK_RATE : Constants.RECOVER_STAM_NOT_RESTING_TICK_RATE;

            if ((Time.realtimeSinceStartup - player.Timers.LastStaminaTick) < interval)
                return false;

            player.Timers.LastStaminaTick = Time.realtimeSinceStartup;
            
            if (!player.Inventory.HasItemEquipped(ItemType.Armor))
                return false;
            
            int extraStam = ExtensionMethods.RandomNumber(1, Mathf.RoundToInt(ExtensionMethods.Percentage(player.Stamina.MaxAmount, 5)));
            player.Stamina.AddResource((ushort)extraStam);
            return true;
        }

        public static bool CheckHungerAndThirst(Player player)
        {
            bool hunger = false, thirst = false;

            if (player.Hunger.CurrentAmount > 0)
                hunger = CheckHunger(player);

            if (player.Thirst.CurrentAmount > 0) 
                thirst = CheckThirst(player);

            return hunger || thirst;
        }

        private static bool CheckHunger(Player player)
        {
            if ((Time.realtimeSinceStartup - player.Timers.LastHungerTick) < Constants.HUNGER_TICK_RATE)
                return false;

            player.Timers.LastHungerTick = Time.realtimeSinceStartup;
            player.Hunger.TakeResource(10);
            player.Flags.IsHungry = player.Hunger.CurrentAmount <= 0;

            return true;
        }

        private static bool CheckThirst(Player player)
        {
            if ((Time.realtimeSinceStartup - player.Timers.LastThirstTick) < Constants.THIRST_TICK_RATE)
                return false;

            player.Timers.LastThirstTick = Time.realtimeSinceStartup;
            player.Thirst.TakeResource(10);
            player.Flags.IsThirsty = player.Thirst.CurrentAmount <= 0;

            return true;
        }

        public static void Meditate(Player player)
        {
            if (player.Mana.CurrentAmount >= player.Mana.MaxAmount)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.FinishedMeditating);
                player.Flags.IsMeditating = false;
                PacketSender.UpdatePlayerStatus(player, PlayerStatus.Meditate);
                return;
            }

            byte meditationSkill = player.Skills[Skill.Meditation];
            int luck = CalculateMeditationLuck(meditationSkill);

            if (ExtensionMethods.RandomNumber(1, luck) == 1)
            {
                ushort manaRestored = (ushort)ExtensionMethods.Percentage(player.Mana.MaxAmount, Constants.MANA_RECOVER_PERCENTAGE);

                if (manaRestored <= 0) manaRestored = 1;

                player.Mana.AddResource(manaRestored);
                PacketSender.SendMultiMessage(player.Id, MultiMessage.ManaRecovered, stackalloc int[] {manaRestored});
                PacketSender.PlayerIndividualResource(player, Resource.Mana);

                TryLevelSkill(player, Skill.Meditation);
            }
        }

        private static int CalculateMeditationLuck(byte meditationSkill)
        {
            return meditationSkill switch
            {
                <= 10 => 35,
                <= 20 => 30,
                <= 30 => 28,
                <= 40 => 24,
                <= 50 => 22,
                <= 60 => 20,
                <= 70 => 18,
                <= 80 => 15,
                <= 90 => 10,
                _ => meditationSkill < 100 ? 7 : 5
            };
        }

        public static void VenomEffect(Player player)
        {
            if ((Time.realtimeSinceStartup - player.Timers.LastVenomTick) < Constants.VENOM_TICK_RATE)
                return;

            PacketSender.SendMultiMessage(player.Id, MultiMessage.YouAreEnvenomed);

            player.Timers.LastVenomTick = Time.realtimeSinceStartup;

            int damage = ExtensionMethods.RandomNumber(1, 5);
            player.Health.TakeDamage(damage, player.Die);
            PacketSender.PlayerIndividualResource(player, Resource.Health);
        }

        public static void BleedingEffect(Player player)
        {
            if ((Time.realtimeSinceStartup - player.Timers.LastBleedTick) < Constants.BLEEDING_TICK_RATE)
                return;

            player.Timers.LastBleedTick = Time.realtimeSinceStartup;
            player.Flags.BleedingTicksRemaining--;

            PacketSender.SendMultiMessage(player.Id, MultiMessage.YouAreBleeding);

            int damage = ExtensionMethods.RandomNumber(8, 15);
            player.Health.TakeDamage(damage, player.Die);
            PacketSender.PlayerIndividualResource(player, Resource.Health);
        }

        public static void TryLevelSkill(Player player, Skill skill)
        {
            if (player.Skills[skill] == Constants.MAX_PLAYER_SKILL) return;
            if (player.Skills[skill] >= CharacterManager.Instance.Levels[player.Level].MaxSkill) return;

            if (!player.Flags.IsHungry && !player.Flags.IsThirsty)
            {
                int probability = CalculateLevelSkillProbability(player, skill);

                if (Random.Range(0, 100) <= probability)
                {
                    player.Skills[skill]++;
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.SkillLevelUp, stackalloc[] {(int)skill, player.Skills[skill]});
                    AddExperience(player, 50);
                }
            }
        }

        private static int CalculateLevelSkillProbability(Player player, Skill skill)
        {
            return player.Skills[skill] switch
            {
                <= 20 => 40,
                <= 40 => 30,
                <= 60 => 20,
                _ => player.Skills[skill] <= 80 ? 15 : 10
            };
        }

        public static void CheckParalysis(Player player)
        {
            if ((Time.realtimeSinceStartup - player.Timers.ParalyzedTime) >= Constants.PLAYER_PARALYZE_TIME)
            {
                player.Flags.IsParalyzed = false;
                player.Flags.IsImmobilized = false;
            }
        }

        public static bool CanStab(Player player, Item weapon)
        {
            if (weapon is { WeaponType: WeaponType.Dagger })
                return player.Skills[Skill.Stabbing] >= Constants.MIN_SKILL_TO_STAB || player.Class.ClassType == ClassType.Assassin;

            return false;
        }

        public static void ChangePlayerFaction(Player player, Faction newFaction)
        {
            player.Faction = newFaction;
            PacketSender.UpdatePlayerStatus(player, PlayerStatus.ChangedFaction);
            player.OnFactionChanged();
        }

        public static void AddDeath(Player attacker, Player killed)
        {
            AddExperience(attacker, (uint)killed.Level * 2);

            PacketSender.SendMultiMessage(attacker.Id, MultiMessage.KilledPlayer, stackalloc[] {killed.Id.AsPrimitiveType()});
            PacketSender.SendMultiMessage(killed.Id, MultiMessage.PlayerKilled, stackalloc[] {attacker.Id.AsPrimitiveType()});

            if (IsNewbie(killed)) return;

            if (CombatSystemUtils.BothInArena(attacker, killed)) return;

            AddCriminalOrCitizenKilled(attacker, killed);
            AddUserKilled(attacker);
            AddPlayerDeath(killed);
            attacker.Events.RaiseKilledPlayer(killed);
        }

        private static void AddCriminalOrCitizenKilled(Player attacker, Player killed)
        {
            //Killed player is Criminal or Chaos
            if (killed.Faction == Faction.Criminal || killed.Faction == Faction.Chaos)
            {
                if (attacker.Flags.LastCriminalKilled != killed.CharacterInfo.CharacterId)
                {
                    attacker.Flags.LastCriminalKilled = killed.CharacterInfo.CharacterId;

                    if (attacker.Stats[PlayerStat.CriminalsKilled] < Constants.MAX_KILLS)
                    {
                        attacker.Stats[PlayerStat.CriminalsKilled]++;
                        PacketSender.PlayerStat(attacker, PlayerStat.CriminalsKilled);
                    }
                }

                return;
            }

            //Killed player is Citizen or Imperial
            if (attacker.Flags.LastCitizenKilled != killed.CharacterInfo.CharacterId)
            {
                attacker.Flags.LastCitizenKilled = killed.CharacterInfo.CharacterId;

                if (attacker.Stats[PlayerStat.CitizensKilled] < Constants.MAX_KILLS)
                {
                    attacker.Stats[PlayerStat.CitizensKilled]++;
                    PacketSender.PlayerStat(attacker, PlayerStat.CitizensKilled);
                }
            }
        }

        private static void AddUserKilled(Player attacker)
        {
            if (attacker.Stats[PlayerStat.UsersKilled] < Constants.MAX_KILLS)
            {
                attacker.Stats[PlayerStat.UsersKilled]++;
                PacketSender.PlayerStat(attacker, PlayerStat.UsersKilled);
            }
        }

        private static void AddPlayerDeath(Player killed)
        {
            if (killed.Stats[PlayerStat.Deaths] < Constants.MAX_KILLS)
            {
                killed.Stats[PlayerStat.Deaths]++;
                PacketSender.PlayerStat(killed, PlayerStat.Deaths);
            }
        }

        public static void LostNpc(Player player)
        {
            Npc ownedNpc = player.Flags.OwnedNpc;

            if (ownedNpc)
            {
                if (player.Pet)
                    player.Pet.StopAttacking();

                ownedNpc.Flags.CombatOwner = null;
                ownedNpc.Flags.AttackedFirstBy = null;
                player.Flags.OwnedNpc = null;
            }
        }
    
        public static void AppropriatedNpc(Player player, Npc npc)
        {
            if (player.IsGameMaster) return;
            if (player.Flags.ZoneType == World.ZoneType.SafeZone) return;

            if (player.Flags.OwnedNpc is not null)
                player.Flags.OwnedNpc.Flags.CombatOwner = null; //If the player already owned an npc set that npc's owner to null cause it won't be theirs anymore

            npc.Flags.CombatOwner = player;
            player.Flags.OwnedNpc = npc;

            Timers.PlayerLostNpcInterval(player, true);
        }

        public static void TryToTameNpc(Player player, Collider2D collision)
        {
            if (!collision) 
                return;

            if (collision.gameObject.layer != Layer.Npc.Id)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.NoNpcToTame);
                return;
            }

            Npc npc = collision.GetComponent<Npc>();
            
            if ((player.CurrentTile.Position - npc.CurrentTile.Position).magnitude > 2f)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.TooFarToTame);
                return;
            }

            if (npc.IsPet)
            {
                if (npc.PetOwner == player)
                {
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.AlreadyTamedThatNpc);
                    return;
                }

                PacketSender.SendMultiMessage(player.Id, MultiMessage.NpcAlreadyHasOwner);
                return;
            }

            if (npc.Info.SkillToTame is null)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantTameNpc);
                return;
            }

            if (npc.Flags.AttackedFirstBy is not null)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantTameNpcInCombat);
                return;
            }

            if (player.Flags.CurrentPetNpcId != NpcId.Empty)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.AlreadyHaveAPet);
                return;
            }

            int tamingSkill = player.Attributes[Attribute.Charisma] * player.Skills[Skill.AnimalTaming];

            if (npc.Info.SkillToTame.Value > tamingSkill || ExtensionMethods.RandomNumber(1, 5) != 1)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.FailedToTameNpc);
                TryLevelSkill(player, Skill.AnimalTaming);
                return;
            }

            PacketSender.SendMultiMessage(player.Id, MultiMessage.SuccessfullyTamedNpc);
            
            player.Flags.CurrentPetNpcId = npc.Info.Id;
            TryLevelSkill(player, Skill.AnimalTaming);
            
            // Save the variables that are needed before despawning the npc
            var currentMap = npc.CurrentMap;
            var currentPosition = npc.CurrentTile.Position;
            var npcInfo = npc.Info;
            var currentHealth = npc.Health.CurrentHealth;
            
            npc.Despawn();

            // If it's in a safe zone don't spawn the pet
            if (player.Flags.ZoneType == World.ZoneType.SafeZone)
            {
                PacketSender.SendMultiMessage(player.Id, MultiMessage.CantSummonPetInSafeZone);
                return;
            }
            
            // Otherwise convert the npc to a pet
            npc.SpawnAsPet(currentMap, currentPosition, npcInfo, player);
            npc.Health.TakeDamage(npc.Health.MaxHealth - currentHealth);
        }

        public static bool IsNewbie(Player player)
        {
            return player.Level <= Constants.MAX_NEWBIE_LEVEL;
        }

        public static void AddExperience(Player player, uint experience)
        {
            if (player.Level >= Constants.MAX_PLAYER_LEVEL) 
                return;
            
            PacketSender.PlayerGainedXp(player.Id, experience);

            uint currentAux = player.CurrentExperience + experience;
            uint xpForNextLevel;

            do
            {
                xpForNextLevel = CharacterManager.Instance.Levels[player.Level].MaxXp;

                if (currentAux < xpForNextLevel)
                {
                    player.CurrentExperience += experience;
                    return;
                }

                player.Level++;
                currentAux -= xpForNextLevel;
                player.CurrentExperience = currentAux;
                LevelUp(player);
                PacketSender.PlayerStats(player);

                if (player.Level >= Constants.MAX_PLAYER_LEVEL)
                {
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.ReachedMaxLevel);
                    player.CurrentExperience = 0;
                    break;
                }

            } while (currentAux >= xpForNextLevel);
        }

        private static void LevelUp(Player player)
        {
            PacketSender.SendMultiMessage(player.Id, MultiMessage.LeveledUp);

            if (player.Level == 13) 
                player.Inventory.LoseNewbieItems();

            if (player.Level < 45)
            {
                Utils.LevelUpStatsIncrease statsIncreases = Class.CalculateLevelUpStats(player);

                int newHealth = player.Health.MaxHealth + statsIncreases.HpIncrease;
                player.Health.SetHealth(newHealth, newHealth);

                ushort newMana = (ushort)(player.Mana.MaxAmount + statsIncreases.ManaIncrease);
                player.Mana.SetResource(newMana, newMana);

                ushort newStam = (ushort)(player.Stamina.MaxAmount + statsIncreases.StaminaIncrease);

                if (newStam > 999) 
                    newStam = 999;

                player.Stamina.SetResource(newStam, newStam);

                player.Hit += (ushort)statsIncreases.HitIncrease;
                Class.CheckHitOverflow(player);

                PacketSender.PlayerMaxResources(player);
                PacketSender.PlayerResources(player);

                if (statsIncreases.HpIncrease > 0)
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.IncreasedHp, stackalloc[] {statsIncreases.HpIncrease});

                if (statsIncreases.StaminaIncrease > 0)
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.IncreasedStamina, stackalloc[] {statsIncreases.StaminaIncrease});

                if (statsIncreases.ManaIncrease > 0)
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.IncreasedMana, stackalloc[] {statsIncreases.ManaIncrease});

                if (statsIncreases.HitIncrease > 0)
                    PacketSender.SendMultiMessage(player.Id, MultiMessage.IncreasedHit, stackalloc[] {statsIncreases.HitIncrease});
            }

            player.AssignableSkills += 5;
            PacketSender.SendMultiMessage(player.Id, MultiMessage.IncreasedSkillPoints);
            player.OnLevelUp();
        }
    }
}
