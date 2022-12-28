using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using AO.Core;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Npcs.AI;
using AO.Npcs.Utils;
using AO.Players;
using AO.Spells;
using AO.Systems;
using AO.Systems.Combat;
using AO.World;
using PacketSender = AO.Network.PacketSender;

namespace AO.Npcs
{
    public sealed class Npc : MonoBehaviour, IPoolObject, INpcAITarget
    {
        [SerializeField] private NpcIdEnum npcId = 0;

        public int InstanceId => GetInstanceID();
        public NpcInfo Info { get; private set; }
        public Health Health { get; private set; }
        public Map CurrentMap { get; private set; }
        public bool IsBeingUsed { get; private set; }
        public NpcFlags Flags { get; private set; }
        public bool IsHostile => Info.HasAI && Info.AttackingBehaviour != NpcAttackingBehaviour.NoAttacking;
        public Tile StartingTile { get; private set; }
        public bool Attackable { get; set; }
        public Player PetOwner { get; private set; }
        public bool IsPet => PetOwner is not null;
        public event Action<INpcAITarget> NpcAttacked;
        public event Action<Npc> NpcDespawned;

        #region Trader
        public NpcInventorySlot[] Inventory { get; private set; }
        public HashSet<ClientId> InteractingWith { get; private set; }
        #endregion


        #region INpcTarget
        public bool IsDead => !IsBeingUsed;
        public Tile CurrentTile => ai is null ? StartingTile : ai.CurrentTile;
        public void AttackTarget(Npc attacker) => CombatSystem.NpcAttacksNpc(attacker, this);
        public void CastSpellOnTarget(Spell spell, Npc attacker) => spell.NpcCastOnNpc(attacker, this);
        public event Action<INpcAITarget> TargetMoved;
        public event Action<INpcAITarget> TargetDied
        {
            add => NpcDespawned += value;
            remove => NpcDespawned -= value;
        }
        #endregion
        
        private NpcAIBase ai;
        private readonly HashSet<Player> sentToPlayers = new();

        private IEnumerator Start()
        {
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
#if UNITY_EDITOR
            // If it's a scene npc set the color to blue for debugging purposes
            spriteRenderer.color = npcId == 0 ? Color.yellow : Color.blue;
#else
            Destroy(spriteRenderer);
#endif
            
            if (npcId == 0) 
                yield break;
            
            while (!GameManager.GameMangerLoaded)
                yield return null;
            
            Map map = GetComponentInParent<Map>();
            NpcInfo npcInfo = GameManager.Instance.GetNpcInfo((ushort)npcId);
            Vector2 position = transform.position;
            Spawn(map, position.Round(0), npcInfo);
            GameManager.Instance.AddSceneNpcToPool(this);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer != Layer.PLAYER_VISION_RANGE) 
                return;
            
            var player = collision.GetComponentInParent<Player>();
            if (!sentToPlayers.Contains(player))
            {
                sentToPlayers.Add(player);
                player.Events.PlayerDisconnected += OnPlayerDisconnected;
                PacketSender.NpcSpawn(this, player.Id);
            }

            SendQuestsForPlayer(player);
        }

        private void OnDestroy()
        {
            ClearSentToPlayers();
        }

        public void Spawn(Map map, Vector2 startingPosition, NpcInfo info)
        {
            if (!InitializeInternal(map, startingPosition, info))
                return;
            
            if (Info.HasAI)
            {
                ai = gameObject.AddComponent<RegularNpcAI>();
                ai.Init(this);
            }
            
            Activate();
        }

        public void SpawnAsPet(Map map, Vector2 startingPosition, NpcInfo info, Player owner)
        {
            if (!InitializeInternal(map, startingPosition, info))
                return;
            
            var petAI = gameObject.AddComponent<PetAI>();
            PetOwner = owner;
            PetOwner.Pet = petAI;
            ai = petAI;
            ai.Init(this);
            
            Activate();
        }

        private bool InitializeInternal(Map map, Vector2 startingPosition, NpcInfo info)
        {
            Info = info;
            gameObject.name = Info.Name;

            if (!Health)
                Health = GetComponent<Health>();
            Health.SetHealth(Info.Health, Info.Health);

            LoadProperties();
            
            // If there is no empty position the npc cannot be spawned
            if (!SetStartingPosition(startingPosition))
            {
                Despawn();
                return false;
            }

            CurrentMap = StartingTile.ParentMap ? StartingTile.ParentMap : map;
            transform.SetParent(CurrentMap.transform);
            return true;
        }

        private void Activate()
        {
            IsBeingUsed = true;
            gameObject.SetActive(true);
        }

        public void RaiseNpcMoved() => TargetMoved?.Invoke(this);
        public void RaiseNpcDespawned() => NpcDespawned?.Invoke(this);
        public void RaiseNpcAttacked(Player attacker) => NpcAttacked?.Invoke(attacker);
        public void RaiseNpcAttacked(Npc attacker) => NpcAttacked?.Invoke(attacker);

        private bool SetStartingPosition(Vector2 startingPosition)
        {
            if (!WorldMap.FindEmptyTileForNpc(this, startingPosition, out Tile tile)) 
                return false;
            
            tile.Npc = this;
            transform.position = tile.Position;
            StartingTile = tile;
            return true;
        }

        public void Kill(Player player = null)
        {
            // A player killed it
            if (player)
            {
                // Give the player the remaining experience if there's any left
                if (Flags.ExperienceCount > 0)
                    PlayerMethods.AddExperience(player, (uint)Flags.ExperienceCount);

                if (Info.GoldAmount > 0) 
                    PlayerMethods.AddGold(player, Info.GoldAmount);

                if (!IsPet) // If this npc isn't a pet send the stat to the player and drop items
                {
                    if (player.Stats[PlayerStat.NpcsKilled] < Constants.MAX_KILLS)
                    {
                        player.Stats[PlayerStat.NpcsKilled]++;
                        PacketSender.PlayerStat(player, PlayerStat.NpcsKilled);
                    }
                    
                    DropAllItems();
                }

                PacketSender.SendMultiMessage(player.Id, MultiMessage.KilledNpc);
                player.Events.RaiseKilledNpc(Info.Id);
            }
            
            Despawn();
        }

        public void Despawn()
        {
            RaiseNpcDespawned();
            PacketSender.NpcDespawned(this, sentToPlayers);

            if (PetOwner is not null)
            {
                PetOwner.Pet = null;
                PetOwner = null;
            }

            if (ai is not null)
                Destroy(ai);
            
            if (StartingTile is not null && StartingTile.Npc == this)
                StartingTile.Npc = null;

            InteractingWith = null;
            Inventory = null;
            ai = null;
            ClearSentToPlayers();

            IsBeingUsed = false;
            gameObject.SetActive(false);
        }

        public bool CanInteractWithPlayer(Player player)
        {
            return Info.NpcFaction switch
            {
                NpcFaction.Neutral => !IsHostile,
                NpcFaction.Imperial => player.Faction is Faction.Citizen or Faction.Imperial,
                NpcFaction.Chaos => player.Faction is Faction.Criminal or Faction.Chaos,
                _ => true
            };
        }

        public void DisplayInfo(Player player)
        {
            player.Flags.TargetNpc = this;

            if (Info.Type == NpcType.Regular)
            {
                int attackerOrOwnerId = IsPet ? PetOwner.Id.AsPrimitiveType() :
                    Flags.AttackedFirstBy ? Flags.AttackedFirstBy.Id.AsPrimitiveType() : 0;

                Span<int> data = stackalloc[]
                {
                    Info.Id.AsPrimitiveType(),
                    Health.CurrentHealth,
                    Health.MaxHealth,
                    IsPet ? 1 : 0,
                    attackerOrOwnerId
                };

                PacketSender.SendMultiMessage(player.Id, MultiMessage.DisplayInfo, data);
                return;
            }
            
            PacketSender.SendMultiMessage(player.Id, MultiMessage.ShowNpcDescription,  stackalloc[] {InstanceId});
        }

        public void Interact(Player player)
        {
            if (!CanInteractWithPlayer(player))
                return;
            
            switch (Info.Type)
            {
                case NpcType.Priest:
                    PriestInteraction(player);
                    break;
                case NpcType.Bank:
                    break;
                case NpcType.Auctioneer:
                    break;
                case NpcType.Gambler:
                    break;
                case NpcType.Trader:
                    StartTrade(player);
                    break;
                case NpcType.Factional:
                    break;
            }
        }

        public void EndInteraction(Player player)
        {
            switch (Info.Type)
            {
                case NpcType.Bank:
                    break;
                case NpcType.Auctioneer:
                    break;
                case NpcType.Gambler:
                    break;
                case NpcType.Trader:
                    EndTrade(player);
                    break;
                case NpcType.Factional:
                    break;
            }
        }

        public void SellToPlayer(Player player, byte slot, ushort quantity)
        {
            if (Info.Type != NpcType.Trader)
                return;

            NpcTradingSystem.SellToPlayer(player, this, slot, quantity);
        }

        public void BuyFromPlayer(Player player, byte slot, ushort quantity)
        {
            if (Info.Type != NpcType.Trader)
                return;

            NpcTradingSystem.BuyFromPlayer(player, this, slot, quantity);
        }

        public void Attacked(Player attacker)
        {
            Flags.AttackedFirstBy ??= attacker;
            RaiseNpcAttacked(attacker);
        }

        public void Attacked(Npc attacker)
        {
            RaiseNpcAttacked(attacker);
        }
        
        public void SendQuestsForPlayer(Player player)
        {
            if (Info.GiveQuests.Count == 0)
                return;
            
            if (player.Flags.NpcsQuestsSent.Contains(Info.Id))
                return;

            var eligibleQuests = new List<QuestId>();
            foreach (var quest in Info.GiveQuests)
            {
                if (player.QuestManager.HasCompletedQuest(quest.Id))
                    continue;
                if (!quest.DoesPlayerMeetAllRequirements(player))
                    continue;
                eligibleQuests.Add(quest.Id);
            }

            player.Flags.NpcsQuestsSent.Add(Info.Id);
            PacketSender.NpcQuests(player.Id, InstanceId, Info.Id, eligibleQuests);
        }

        private static void PriestInteraction(Player player)
        {
            if (player.Flags.IsDead)
                player.Revive();

            player.Flags.IsEnvenomed = false;
            player.Flags.BleedingTicksRemaining = 0;
            player.Health.Heal(player.Health.MaxHealth);
            PacketSender.PlayerIndividualResource(player, Resource.Health);
        }

        private void StartTrade(Player player)
        {
            if (player.Flags.InteractingWithNpc is null && !player.Flags.IsDead)
            {
                player.Flags.InteractingWithNpc = this;
                InteractingWith.Add(player.Id);

                PacketSender.NpcStartTrade(player.Id, Info.Id, Inventory, NpcTradingSystem.Discount(player.Skills[Skill.Trading]));
            }
        }

        private void EndTrade(Player player)
        {
            player.Flags.InteractingWithNpc = null;
            InteractingWith.Remove(player.Id);
        }
        
        private void LoadProperties()
        {
            Attackable = Info.Attackable;

            if (Info.Type == NpcType.Trader)
            {
                InteractingWith = new HashSet<ClientId>();
                Inventory = new NpcInventorySlot[Constants.NPC_INVENTORY_SPACE];
                Info.NpcInventory.CopyTo(Inventory, 0);
            }
            
            Flags = new NpcFlags
            {
                ExperienceCount = Info.XpAmount
            };
        }

        private void DropAllItems()
        {
            foreach (var item in Info.DroppableItems)
                if (ExtensionMethods.RandomNumber(1f, 100f) <= item.DropChance) //Drop the item if the random number is lower than the chance
                    GameManager.Instance.CreateWorldItem(GameManager.Instance.GetItem(item.ItemId), item.Quantity, transform.position);
        }

        private void ClearSentToPlayers()
        {
            foreach (var player in sentToPlayers)
                player.Events.PlayerDisconnected -= OnPlayerDisconnected;
            
            sentToPlayers.Clear();
        }

        private void OnPlayerDisconnected(Player player)
        {
            player.Events.PlayerDisconnected -= OnPlayerDisconnected;
            sentToPlayers.Remove(player);
        }
    }
}
