using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Items;
using AO.Network.Server;
using AO.Npcs;
using AO.Npcs.AI;
using AO.Players.Talents.Worker;
using AO.Players.Utils;
using AO.Spells;
using AO.Systems.Combat;
using AO.Systems;
using AO.Systems.Mailing;
using AO.Systems.Professions;
using AO.World;
using PacketSender = AO.Network.PacketSender;

namespace AO.Players
{
    public class Player : MonoBehaviour, INpcAITarget
    {
        public Client Client { get; private set; }
        public ClientId Id => Client.Id;
        public AOCharacterInfo CharacterInfo { get; private set; }
        public string Username => CharacterInfo.CharacterName;
        public string Description { get; set; }
        public float Speed { get; set; }
        public Health Health { get; private set; }
        public PlayerResource Mana { get; private set; }
        public PlayerResource Stamina { get; private set; }
        public PlayerResource Hunger { get; private set; }
        public PlayerResource Thirst { get; private set; }
        public Facing Facing { get; private set; } = Facing.Down;
        public Map CurrentMap { get; private set; }
        public Class Class { get; private set; }
        public Race Race { get; private set; }
        public Gender Gender { get; private set; } = 0;
        public byte Head { get; private set; }
        public Dictionary<Attribute, byte> Attributes { get; private set; }
        public readonly Dictionary<Skill, byte> Skills = new();
        public byte Level { get; set; }
        public uint CurrentExperience { get; set; }
        public ushort AssignableSkills { get; set; }
        public byte AvailableTalentPoints { get; set; }
        public ushort Hit { get; set; } = 2;
        public Faction Faction { get; set; } = Faction.Citizen;
        public FactionRank FactionRank { get; set; } = FactionRank.One;
        public readonly Dictionary<PlayerStat, uint> Stats = new();
        public uint Gold { get; set; }
        public uint BankGold { get; private set; }
        public bool HasGuild { get; private set; }
        public string GuildName { get; private set; } = string.Empty;
        public PlayerInventory Inventory { get; private set; }
        public readonly Spell[] Spells = new Spell[Constants.PLAYER_SPELLS_SPACE];
        public readonly PlayerFlags Flags = new();
        public readonly PlayerTimers Timers = new();
        public bool HasBoatEquipped { get; set; }
        public Tile CurrentTile { get; set; }
        public bool IsGameMaster { get; protected set; }
        public PetAI Pet { get; set; }
        public Party Party { get; set; }
        public readonly HashSet<ClientId> NearbyPlayers = new();
        public PlayerQuestManager QuestManager { get; private set; }
        public WorkerTalentTrees WorkerTalentTrees { get; private set; }

        public readonly PlayerEvents Events = new();
        public PlayerMovementInputs MovementInputs { get; set; } = PlayerMovementInputs.Empty;

        #region INpcTarget
        public bool IsDead => Flags.IsDead;
        public void AttackTarget(Npc attacker) => CombatSystem.NpcAttacksPlayer(attacker, this);
        public void CastSpellOnTarget(Spell spell, Npc attacker) => spell.NpcCastOnPlayer(attacker, this);
        #endregion
        
        private IEnumerator attributesCoroutine;

        // Forward INpcTarget event to events class
        public event Action<INpcAITarget> TargetMoved
        {
            add => Events.PlayerMoved += value;
            remove => Events.PlayerMoved -= value;
        }
        
        public event Action<INpcAITarget> TargetDied
        {
            add => Events.PlayerDied += value;
            remove => Events.PlayerDied -= value;
        }
        
        private void OnDestroy()
        {
            QuestManager?.DisposeAllQuests();
        }

        private async void FixedUpdate()
        {
            Move();
            OnFixedUpdate();
            
            if (Flags.MailCacheExpirationTime < Time.realtimeSinceStartup && await MailManager.UpdateAndDeletePlayerMails(this))
                Flags.CachedMails.Clear();
        }

        protected virtual void OnFixedUpdate()
        {
            if (Flags.IsDead) 
                return;
            
            if (Flags.IsMeditating) 
                PlayerMethods.Meditate(this);

            if (Flags.IsEnvenomed) 
                PlayerMethods.VenomEffect(this);

            if (Flags.BleedingTicksRemaining > 0) 
                PlayerMethods.BleedingEffect(this);

            if (Flags.IsParalyzed || Flags.IsImmobilized) 
                PlayerMethods.CheckParalysis(this);

            bool sendStats = PlayerMethods.CheckHungerAndThirst(this);

            if (!Flags.IsHungry && !Flags.IsThirsty)
            {
                if (PlayerMethods.RecoverStamina(this))
                    sendStats = true;
            }

            if (sendStats)
                PacketSender.PlayerResources(this);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            switch (collision.gameObject.layer)
            {
                case Layer.PLAYER_VISION_RANGE:
                {
                    var otherPlayer = collision.GetComponentInParent<Player>();
                    NearbyPlayers.Add(otherPlayer.Id);
                    PacketSender.PlayerRangeChanged(Id, otherPlayer.Id, true);
                    otherPlayer.Inventory.SendEquippedItemsToPlayer(Id);
                    break;
                }
                case Layer.TRIGGER:
                {
                    var trigger = collision.GetComponent<Trigger>();

                    switch (trigger.Type)
                    {
                        case TriggerType.SafeZone:
                            Flags.ZoneType = ZoneType.SafeZone;
                            break;
                        case TriggerType.UnsafeZone:
                            Flags.ZoneType = ZoneType.UnsafeZone;
                            break;
                        case TriggerType.Arena:
                            Flags.ZoneType = ZoneType.Arena;
                            break;
                        case TriggerType.AntiBlock:
                            break;
                        case TriggerType.CantInvis:
                            Flags.CanInvis = false;
                            break;
                        case TriggerType.QuestExploreArea:
                            Events.RaisePlayerEnteredExploreArea(trigger.ExploreAreaId);
                            break;
                    }
                    break;
                }
                case Layer.WATER:
                {
                    if (Inventory.HasItemEquipped(ItemType.Boat))
                    {
                        Flags.IsSailing = true;
                        PacketSender.UpdatePlayerStatus(this, PlayerStatus.UsedBoat);
                    }

                    break;
                }
                case Layer.MAP:
                {
                    ChangeMap(collision.GetComponent<Map>());
                    break;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            switch (collision.gameObject.layer)
            {
                case Layer.PLAYER_VISION_RANGE:
                {
                    var otherPlayerId = collision.GetComponentInParent<Player>().Id;
                    NearbyPlayers.Remove(otherPlayerId);
                    PacketSender.PlayerRangeChanged(Id, otherPlayerId, false);
                    break;
                }
                case Layer.TRIGGER:
                {
                    var trigger = collision.GetComponent<Trigger>();

                    switch (trigger.Type)
                    {
                        case TriggerType.SafeZone:
                        case TriggerType.UnsafeZone:
                        case TriggerType.Arena:
                            Flags.ZoneType = CurrentMap.ZoneType;
                            break;
                        case TriggerType.AntiBlock:
                            break;
                        case TriggerType.CantInvis:
                            Flags.CanInvis = true;
                            break;
                    }
                    break;
                }
                case Layer.WATER:
                {
                    if (Inventory.HasItemEquipped(ItemType.Boat))
                    {
                        Flags.IsSailing = false;
                        PacketSender.UpdatePlayerStatus(this, PlayerStatus.UsedBoat);
                    }

                    break;
                }
            }
        }

        public virtual bool Initialize(Client client, AOCharacterInfo characterInfo, IDictionary<string, object> playerInfo)
        {
            //Get the map and position info first, because if no available position is found we disconnect
            CurrentMap = GameManager.Instance.GetMap(Convert.ToInt16(playerInfo["map"]));
            transform.parent = CurrentMap.transform;
            Flags.ZoneType = CurrentMap.ZoneType;
            
            // Deserialize the inventory first in case the player disconnected in the water
            Inventory = new PlayerInventory(this, playerInfo["inventory"].ToString());
            
            int xPos = (int)playerInfo["x_pos"];
            int yPos = (int)playerInfo["y_pos"];
            //Vector2 startingPosition = GameManager.Instance.MapPositionToWorldPosition(CurrentMap, new Vector2(xPos, yPos));
            var startingPosition = new Vector2(xPos, yPos);
            
            if (!WorldMap.FindEmptyTileForPlayer(this, startingPosition, out Tile tile))
            {
                gameObject.SetActive(false);
                Destroy(this);
                return false;
            }

            tile.Player = this;
            Flags.IsSailing = tile.IsWater;
            CurrentTile = tile;
            transform.position = CurrentTile.Position;

            Speed = Constants.PLAYER_MOVE_SPEED;
            
            Client = client;
            CharacterInfo = characterInfo;
            gameObject.name = CharacterInfo.CharacterName;
            
            ClassType classType = (ClassType)Convert.ToByte(playerInfo["class"]);
            Class = CharacterManager.Instance.Classes[classType];

            if (Class.ClassType == ClassType.Worker)
                WorkerTalentTrees = new WorkerTalentTrees(this, playerInfo);
            
            RaceType raceType = (RaceType)Convert.ToByte(playerInfo["race"]);
            Race = CharacterManager.Instance.Races[raceType];

            Attributes = new Dictionary<Attribute, byte>(Race.Attributes);

            Gender = Convert.ToBoolean(playerInfo["gender"]) ? Gender.Male : Gender.Female;

            Head = Convert.ToByte(playerInfo["head_id"]);
            
            Skills.Add(Skill.Magic, Convert.ToByte(playerInfo["magic"]));
            Skills.Add(Skill.ArmedCombat, Convert.ToByte(playerInfo["armed_combat"]));
            Skills.Add(Skill.RangedWeapons, Convert.ToByte(playerInfo["ranged_weapons"]));
            Skills.Add(Skill.UnarmedCombat, Convert.ToByte(playerInfo["unarmed_combat"]));
            Skills.Add(Skill.Stabbing, Convert.ToByte(playerInfo["stabbing"]));
            Skills.Add(Skill.CombatTactics, Convert.ToByte(playerInfo["combat_tactics"]));
            Skills.Add(Skill.MagicResistance, Convert.ToByte(playerInfo["magic_resistance"]));
            Skills.Add(Skill.ShieldDefense, Convert.ToByte(playerInfo["shield_defense"]));
            Skills.Add(Skill.Meditation, Convert.ToByte(playerInfo["meditation"]));
            Skills.Add(Skill.Survival, Convert.ToByte(playerInfo["survival"]));
            Skills.Add(Skill.AnimalTaming, Convert.ToByte(playerInfo["animal_taming"]));
            Skills.Add(Skill.Hiding, Convert.ToByte(playerInfo["hiding"]));
            Skills.Add(Skill.Trading, Convert.ToByte(playerInfo["trading"]));
            Skills.Add(Skill.Thieving, Convert.ToByte(playerInfo["thieving"]));
            Skills.Add(Skill.Leadership, Convert.ToByte(playerInfo["leadership"]));
            Skills.Add(Skill.Sailing, Convert.ToByte(playerInfo["sailing"]));
            Skills.Add(Skill.HorseRiding, Convert.ToByte(playerInfo["horse_riding"]));
            Skills.Add(Skill.Mining, Convert.ToByte(playerInfo["mining"]));
            Skills.Add(Skill.Blacksmithing, Convert.ToByte(playerInfo["blacksmithing"]));
            Skills.Add(Skill.Woodcutting, Convert.ToByte(playerInfo["woodcutting"]));
            Skills.Add(Skill.Woodworking, Convert.ToByte(playerInfo["woodworking"]));
            Skills.Add(Skill.Fishing, Convert.ToByte(playerInfo["fishing"]));
            Skills.Add(Skill.Tailoring, Convert.ToByte(playerInfo["tailoring"]));

            Level = Convert.ToByte(playerInfo["level"]);
            CurrentExperience = Convert.ToUInt32(playerInfo["current_experience"]);
            AssignableSkills = Convert.ToUInt16(playerInfo["assignable_skills"]);
            AvailableTalentPoints = Convert.ToByte(playerInfo["talent_points"]);

            Health = GetComponent<Health>();
            int maxHealth =  Convert.ToInt32(playerInfo["max_health"]);
            int currentHealth = Convert.ToInt32(playerInfo["current_health"]);
            Health.SetHealth(maxHealth, currentHealth);
            Flags.IsDead = Health.CurrentHealth <= 0;

            ushort maxMana = Convert.ToUInt16(playerInfo["max_mana"]);
            ushort currentMana = Convert.ToUInt16(playerInfo["current_mana"]);
            Mana = new PlayerResource(maxMana, currentMana);

            ushort maxStamina = Convert.ToUInt16(playerInfo["max_stamina"]);
            ushort currentStamina = Convert.ToUInt16(playerInfo["current_stamina"]);
            Stamina = new PlayerResource(maxStamina, currentStamina);

            ushort maxHunger = Convert.ToUInt16(playerInfo["max_hunger"]);
            ushort currentHunger = Convert.ToUInt16(playerInfo["current_hunger"]);
            Hunger = new PlayerResource(maxHunger, currentHunger);
            Flags.IsHungry = Hunger.CurrentAmount <= 0;

            ushort maxThirst = Convert.ToUInt16(playerInfo["max_thirst"]);
            ushort currentThirst = Convert.ToUInt16(playerInfo["current_thirst"]);
            Thirst = new PlayerResource(maxThirst, currentThirst);
            Flags.IsThirsty = Thirst.CurrentAmount <= 0;

            Hit = Convert.ToUInt16(playerInfo["hit"]);

            Faction = (Faction)Convert.ToByte(playerInfo["faction"]);
            Flags.SafeToggleOn = Faction is Faction.Citizen or Faction.Imperial;

            HasGuild = Convert.ToBoolean(playerInfo["has_guild"]);

            if (HasGuild)
                GuildName = playerInfo["guild_name"].ToString();
            
            Stats.Add(PlayerStat.CriminalsKilled, Convert.ToUInt32(playerInfo["criminals_killed"]));
            Stats.Add(PlayerStat.CitizensKilled, Convert.ToUInt32(playerInfo["citizens_killed"]));
            Stats.Add(PlayerStat.UsersKilled, Convert.ToUInt32(playerInfo["users_killed"]));
            Stats.Add(PlayerStat.NpcsKilled, Convert.ToUInt32(playerInfo["npcs_killed"]));
            Stats.Add(PlayerStat.Deaths, Convert.ToUInt32(playerInfo["deaths"]));
            Stats.Add(PlayerStat.RemainingJailTime, Convert.ToUInt32(playerInfo["remaining_jail_time"]));

            Gold = Convert.ToUInt32(playerInfo["gold"]);
            BankGold = Convert.ToUInt32(playerInfo["bank_gold"]);

            

            //59 bank
            //60 mailbox
            CharacterManager.ConvertJsonToSpells(playerInfo["spells"].ToString(), Spells);
            
            QuestManager = new PlayerQuestManager(this, playerInfo["quests_progresses"].ToString(), playerInfo["quests_completed"].ToString());

            Description = playerInfo["description"].ToString();
            
            return true;
        }

        public void OnLevelUp()
        {
            UpdateNearbyNpcsQuests();
        }

        public void OnFactionChanged()
        {
            UpdateNearbyNpcsQuests();
        }

        private void UpdateNearbyNpcsQuests()
        {
            Flags.NpcsQuestsSent.Clear();
            
            var nearbyNpcs = Physics2D.OverlapBoxAll(CurrentTile.Position, new Vector2(15, 14), 0f, LayerMask.GetMask(Layer.Npc.Name));
            foreach (var nearbyNpc in nearbyNpcs)
            {
                var npc = nearbyNpc.GetComponent<Npc>();
                if (npc.CanInteractWithPlayer(this))
                    npc.SendQuestsForPlayer(this);
            }
        }
        
        public void HandlePlayerExtraInput(PlayerInput playerInput)
        {
            switch (playerInput)
            {
                case PlayerInput.GrabItem:
                    Inventory.GrabItem();
                    break;
                case PlayerInput.Attack:
                    Attack();
                    break;
                case PlayerInput.SafeToggle:
                    Flags.SafeToggleOn = !Flags.SafeToggleOn;
                    PacketSender.PlayerInputReturn(this, playerInput);
                    break;
                case PlayerInput.RessToggle:
                    Flags.RessToggleOn = !Flags.RessToggleOn;
                    PacketSender.PlayerInputReturn(this, playerInput);
                    break;
                case PlayerInput.Exit:
                    DisconnectPlayerFromWorld();
                    break;
                case PlayerInput.Meditate:
                    Meditate();
                    break;
                case PlayerInput.StartParty:
                    Party ??= new Party(this);
                    break;
                case PlayerInput.LeaveParty:
                    Party?.RemoveMember(Id, false);
                    break;
            }
        }

        public void HandleLeftClick(Vector2 clickPosition, bool doubleClick)
        {
            Collider2D collision = CollisionManager.CheckClickCollision(this, clickPosition);

            if (!collision)
                return;

            switch (collision.gameObject.layer)
            {
                case Layer.PLAYER:
                    HandlePlayerClick(collision);
                    break;
                case Layer.WORLD_ITEM:
                    var worldItem = collision.GetComponent<WorldItem>();
                    PacketSender.SendMultiMessage(Id, MultiMessage.ClickedOnWorldItem, stackalloc int[] {worldItem.ItemId.AsPrimitiveType(), worldItem.Quantity});
                    break;
                case Layer.NPC:
                    HandleNpcClick(collision, doubleClick);
                    break;
                case Layer.OBSTACLES:
                    collision.GetComponentInParent<Obstacle>().Click(this, doubleClick);
                    break;
            }
        }

        private void HandlePlayerClick(Collider2D collision)
        {
            if (collision.isTrigger)
                return;

            var player = collision.GetComponent<Player>();
            var type = ConsoleMessage.PlayerClickCitizen;

            switch (player.Faction)
            {
                case Faction.Criminal:
                    type = ConsoleMessage.PlayerClickCriminal;
                    break;
                case Faction.Imperial:
                    type = ConsoleMessage.PlayerClickImperial;
                    break;
                case Faction.Chaos:
                    type = ConsoleMessage.PlayerClickChaos;
                    break;
            }

            if (IsGameMaster) 
                type = ConsoleMessage.PlayerClickGameMaster;
            
            int isNewbieInt = Convert.ToInt32(PlayerMethods.IsNewbie(player));
            PacketSender.SendMultiMessage(Id, MultiMessage.ClickedOnPlayer, stackalloc[] {player.Id.AsPrimitiveType(), isNewbieInt, (int)type});
        }

        private void HandleNpcClick(Collider2D collision, bool doubleClick)
        {
            Npc npc = collision.GetComponent<Npc>();

            if (doubleClick)
            {
                npc.Interact(this);
                return;
            }
                    
            npc.DisplayInfo(this);
        }

        public void HandleLeftClickRequest(Vector2 clickPosition, ClickRequest request)
        {
            if (Flags.IsDead || Flags.IsMeditating) 
                return;

            Collider2D collision = CollisionManager.CheckClickCollision(this, clickPosition);

            switch (request)
            {
                case ClickRequest.CastSpell:
                    TryCastSpell(collision);
                    break;
                case ClickRequest.ProjectileAttack:
                    CombatSystem.PlayerRangedAttack(this, collision);
                    break;
                case ClickRequest.TameAnimal:
                    PlayerMethods.TryToTameNpc(this, collision);
                    break;
                case ClickRequest.PetChangeTarget:
                    TryChangePetTarget(collision);
                    break;
                case ClickRequest.Steal:
                    break;
                case ClickRequest.InviteToParty:
                    if (Party is not null && collision && collision.gameObject.layer == Layer.PLAYER)
                        Party.InvitePlayer(collision.GetComponent<Player>());
                    break;
                case ClickRequest.Mine:
                    CollectionProfessions.TryStartMining(this, collision, clickPosition);
                    break;
                case ClickRequest.CutWood:
                    CollectionProfessions.TryStartCuttingWood(this, collision, clickPosition);
                    break;
                case ClickRequest.Fish:
                    CollectionProfessions.TryStartFishing(this, clickPosition);
                    break;
                case ClickRequest.Smelt:
                    CollectionProfessions.TryStartSmelting(this, collision, clickPosition);
                    break;
                case ClickRequest.CraftBlacksmithing:
                    PacketSender.OpenCraftingWindow(this, CraftingProfession.Blacksmithing);
                    break;
            }
        }

        /// <summary>Prepares the spell to be casted by the player.</summary>
        public void SelectSpell(byte spellSlot)
        {
            if (spellSlot <= Constants.PLAYER_SPELLS_SPACE)
            {
                Flags.SelectedSpell = spellSlot;
                PacketSender.ClickRequest(Id, ClickRequest.CastSpell);
            }
        }

        /// <summary>Swaps spells slots up or down.</summary>
        public void MoveSpell(byte spellSlot, bool up)
        {
            if (spellSlot <= 0 && up) return; //Can't move the first spell any higher
            if (spellSlot >= Constants.PLAYER_SPELLS_SPACE && !up) return; //Can't move the last spell any lower

            int aux = up ? -1 : 1; //If up is true we move the spell up so -1 cause its previous index we have to swap, opposite for down

            (Spells[spellSlot], Spells[spellSlot + aux]) = (Spells[spellSlot + aux], Spells[spellSlot]);

            //Finally notify the player
            PacketSender.MovePlayerSpell(Id, spellSlot, (byte)(spellSlot + aux));
        }

        /// <summary>Tries to attack.</summary>
        private void Attack()
        {
            if (Flags.IsMeditating) return;

            if (Inventory.TryGetEquippedItem(ItemType.Weapon, out var weapon))
            {
                if (weapon.IsRangedWeapon)
                {
                    PacketSender.SendMultiMessage(Id, MultiMessage.CantUseWeaponLikeThat);
                    return;
                }
            }

            CombatSystem.PlayerAttacks(this);
        }

        /// <summary>Revives the player and sets their health to 1.</summary>
        public void Revive()
        {
            Flags.IsDead = false;
            Health.Heal(1);
            PacketSender.UpdatePlayerStatus(this, PlayerStatus.Revived);
            PacketSender.PlayerIndividualResource(this, Resource.Health);
            Events.RaisePlayerRevived(this);
        }

        /// <summary>Kills the player and drops all their items.</summary>
        public void Die()
        {
            Events.RaisePlayerDied(this);
            
            //TODO play sounds
            Stamina.TakeResource(Stamina.CurrentAmount);
            PacketSender.PlayerIndividualResource(this, Resource.Stamina);
            Flags.IsDead = true;
            Flags.IsEnvenomed = false;
            Flags.BleedingTicksRemaining = 0;
            Flags.IsParalyzed = false;

            if (Flags.OwnedNpc is not null)
            {
                if (Flags.OwnedNpc.Flags.AttackedFirstBy == this)
                    Flags.OwnedNpc.Flags.AttackedFirstBy = null;
            }

            Flags.OwnedNpc = null;

            if (Flags.IsMeditating)
            {
                Flags.IsMeditating = false;
                PacketSender.UpdatePlayerStatus(this, PlayerStatus.Meditate);
            }

            if (Flags.IsInvisible || Flags.IsHidden)
            {
                Flags.IsInvisible = false;
                Flags.IsHidden = false;
                //Todo reset timers and inform the client
            }

            if (Pet)
                Pet.Dismiss();

            if (Flags.ZoneType != ZoneType.Arena)
                Inventory.DropAllItems();

            PacketSender.UpdatePlayerStatus(this, PlayerStatus.Died);
        }

        /// <summary>Modifies an attribute momentarily.</summary>
        public void ModifyAttribute(Attribute attribute, byte modValue, float duration)
        {
            if (attributesCoroutine is not null)
                StopCoroutine(attributesCoroutine);

            Attributes[attribute] += modValue;

            //If the current attribute value goes over 38
            if (Attributes[attribute] > 38)
                Attributes[attribute] = 38;

            attributesCoroutine = ResetAttributes(duration);
            StartCoroutine(attributesCoroutine);
            PacketSender.PlayerAttributes(this);
        }

        /// <summary>Tries to make the player meditate and notifies the player.</summary>
        public void Meditate()
        {
            if (Flags.IsDead)
            {
                PacketSender.SendMultiMessage(Id, MultiMessage.CantMeditateDead);
                return;
            }

            if (Mana.MaxAmount == 0)
            {
                PacketSender.SendMultiMessage(Id, MultiMessage.CantMeditate);
                return;
            }

            if (IsGameMaster)
            {
                Mana.AddResource(Mana.MaxAmount);
                PacketSender.PlayerIndividualResource(this, Resource.Mana);
                PacketSender.SendMultiMessage(Id, MultiMessage.ManaRecovered, stackalloc int[] {Mana.MaxAmount});
                return;
            }

            if (Flags.IsMeditating)
                PacketSender.SendMultiMessage(Id, MultiMessage.StoppedMeditating);

            Flags.IsMeditating = !Flags.IsMeditating;

            PacketSender.UpdatePlayerStatus(this, PlayerStatus.Meditate);
        }

        /// <summary>Assigns the player available skills and makes sure they don't cheat by sending edited packets.</summary>
        public void ChangeSkills(Dictionary<Skill, byte> skillsChanged)
        {
            int counter = 0;

            foreach (var entry in skillsChanged)
            {
                //Get the current value
                byte currentValue = Skills[entry.Key];

                //If the current value is greater than the one sent or the one sent is greater than the maximum allowed the packet was edited
                if (currentValue > entry.Value || entry.Value > Constants.MAX_PLAYER_SKILL)
                {
                    //Ban
                    Client.Disconnect();
                    return;
                }

                Skills[entry.Key] = entry.Value;
                counter += entry.Value - currentValue;
            }

            //If the counter is greater than assignable skills it means the player assigned more than available, edited packet as well
            if (counter > AssignableSkills)
            {
                //Ban
                Client.Disconnect();
                return;
            }

            AssignableSkills -= (ushort)counter;
        }

        private void ChangeMap(Map newMap)
        {
            GameManager.Instance.MovePlayerIntoNewMap(this, newMap);
            GameManager.Instance.SendDoorStatesInCurrentMap(this);
            CurrentMap = newMap;
            Flags.ZoneType = newMap.ZoneType;
        }

        /// <summary>Executes the work action every work interval while the IsWorking flag is set to true.</summary>
        public IEnumerator WorkCoroutine(Action<Player, WorkParameters> doProfession, WorkParameters workParameters)
        {
            Flags.IsWorking = true;
            PacketSender.SendMultiMessage(Id, MultiMessage.StartWorking);

            while (Flags.IsWorking)
            {
                doProfession(this, workParameters);
                yield return new WaitForSeconds(Constants.PLAYER_WORK_INTERVAL * workParameters.IntervalModifier);
            }

            PacketSender.SendMultiMessage(Id, MultiMessage.StopWorking);
        }

        /// <summary>Removes the player from the game. It does not disconnect the client from the server.</summary>
        /// <param name="force">Whether or not to wait 10 seconds if the player is in UnsafeZone.</param>
        public void DisconnectPlayerFromWorld(bool force = false)
        {
            if (!force && Flags.ZoneType != ZoneType.SafeZone)
            {
                if (!Flags.Disconnecting)
                    StartCoroutine(DisconnectCoroutine());

                return;
            }

            OnPlayerDisconnected();
        }
        
        private void OnPlayerDisconnected()
        {
            Flags.HasDisconnected = true;
            Events.RaisePlayerDisconnected(this);
            
            if (Flags.TargetNpc) 
                Flags.TargetNpc.EndInteraction(this);
            
            if (Pet)
                Pet.Dismiss();
            
            Party?.RemoveMember(Id, false);
            CharacterManager.EnqueuePlayerToSave(this);
            CharacterManager.Instance.RemoveOnlinePlayer(this);
            GameManager.Instance.RemovePlayerFromMap(this);
            PacketSender.PlayerDisconnected(Id);
            Destroy(gameObject);
        }

        /// <summary>Disconnects the player after 10 seconds. Used to log out in non safe zones.</summary>
        private IEnumerator DisconnectCoroutine()
        {
            Flags.Disconnecting = true;
            float timer = 10f;
            PacketSender.SendMultiMessage(Id, MultiMessage.ExitingInTenSeconds);

            while (Flags.Disconnecting)
            {
                yield return new WaitForFixedUpdate();
                timer -= Time.fixedDeltaTime;

                if (timer <= 0f)
                    OnPlayerDisconnected();
            }

            PacketSender.SendMultiMessage(Id, MultiMessage.ExitingCancelled);
        }

        private void TryCastSpell(Collider2D collision)
        {
            var spell = Spells[Flags.SelectedSpell];

            if (spell is null || !collision) 
                return;
            
            if (!Core.Timers.PlayerCanUseBowInterval(this, false))
                return;
            
            if (!Core.Timers.PlayerCanCastSpellInterval(this))
                return;
                
            if (collision.gameObject.layer == Layer.Player.Id)
            {
                var targetPlayer = collision.GetComponent<Player>();
                spell.PlayerCastOnPlayer(this, targetPlayer);
            }
            else if (collision.gameObject.layer == Layer.Npc.Id)
            {
                var targetNpc = collision.GetComponent<Npc>();
                spell.PlayerCastOnNpc(this, targetNpc);
            }
        }

        private void TryChangePetTarget(Collider2D collision)
        {
            if (Pet is null || !collision)
                return;
            
            if (collision.gameObject.layer == Layer.Player.Id)
            {
                var targetPlayer = collision.GetComponent<Player>();
                Pet.TrySetNewTarget(targetPlayer, true);
            }
            else if (collision.gameObject.layer == Layer.Npc.Id)
            {
                var targetNpc = collision.GetComponent<Npc>();
                Pet.TrySetNewTarget(targetNpc, true);
            }
        }

        /// <summary>Processes player input and moves the player if there aren't any collisions.</summary>
        private void Move()
        {
            if (!Flags.IsParalyzed && Vector3.Distance(transform.position, CurrentTile.Position) <= .05f)
            {
                Vector2 tentativePos = CurrentTile.Position;
                bool moving = false;

                if ((MovementInputs & PlayerMovementInputs.MoveUp) == PlayerMovementInputs.MoveUp)
                {
                    moving = true;
                    Facing = Facing.Up;
                    tentativePos += Vector2.up;
                }
                else if ((MovementInputs & PlayerMovementInputs.MoveDown) == PlayerMovementInputs.MoveDown)
                {
                    moving = true;
                    Facing = Facing.Down;
                    tentativePos += Vector2.down;
                }
                else if ((MovementInputs & PlayerMovementInputs.MoveLeft) == PlayerMovementInputs.MoveLeft)
                {
                    moving = true;
                    Facing = Facing.Left;
                    tentativePos += Vector2.left;
                }
                else if ((MovementInputs & PlayerMovementInputs.MoveRight) == PlayerMovementInputs.MoveRight)
                {
                    moving = true;
                    Facing = Facing.Right;
                    tentativePos += Vector2.right;
                }

                // Avoid trying to calculate movement every physics update if the player didn't actually move
                if (moving)
                {
                    Flags.Disconnecting = false;
                    Flags.IsWorking = false;
                    TryToMoveToTile(tentativePos);
                }
            }

            transform.position = Vector3.MoveTowards(transform.position, CurrentTile.Position, Speed * Time.fixedDeltaTime);
            PacketSender.PlayerPosition(this);
            MovementInputs = PlayerMovementInputs.Empty;
            // Don't send update position here because this is run every physics update
        }

        protected virtual void TryToMoveToTile(Vector2 tentativePos)
        {
            if (!Flags.IsImmobilized && CurrentTile.CanPlayerMoveToNeighbourTile(this, tentativePos, out Tile newTile))
            {
                CurrentTile.Player = null;
                newTile.Player = this;
                CurrentTile = newTile;
                PacketSender.PlayerUpdatePosition(Id, (int)newTile.Position.x, (int)newTile.Position.y);
                if (Flags.IsMeditating) 
                    Meditate(); //Stop meditation if player starts moving
                Events.RaisePlayerMoved(this);
            }
        }

        /// <summary>Coroutine that resets the attributes once the timer has expired.</summary>
        private IEnumerator ResetAttributes(float duration)
        {
            yield return new WaitForSeconds(duration);

            Attributes[Attribute.Strength] = Race.Attributes[Attribute.Strength];
            Attributes[Attribute.Agility] = Race.Attributes[Attribute.Agility];

            attributesCoroutine = null;
            PacketSender.PlayerAttributes(this);
        }
    }
}
