using System.Collections.Generic;
using AO.Core.Ids;
using AO.Core.Utils;
using AO.Network;
using AO.Players;
using AO.World;
using UnityEngine;

namespace AO.Items
{
    public class WorldItem : MonoBehaviour, Core.IPoolObject
    {
        public int InstanceId => GetInstanceID();
        public ItemId ItemId { get; private set; }
        public bool Grabbable { get; private set; }
        public ushort Quantity { get; set; }
        public Map CurrentMap { get; private set; }
        public bool IsBeingUsed { get; private set; }

        private Tile currentTile;
        
        private readonly HashSet<Player> sentToPlayers = new();

        public void Initialize(ItemId itemId, string itemName, bool grabbable, ushort quantity, Tile tile)
        {
            ItemId = itemId;
            Grabbable = grabbable;
            Quantity = quantity;
            CurrentMap = tile.ParentMap;
            gameObject.name = $"{itemName} ({Quantity})";

            var t = transform;
            t.SetParent(CurrentMap.transform);
            t.position = tile.Position;
            tile.WorldItem = this;
            currentTile = tile;
            
            IsBeingUsed = true;
            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            ClearSentToPlayers();
        }

        public void ResetWorldItem()
        {
            PacketSender.WorldItemDestroyed(InstanceId, sentToPlayers);
            ClearSentToPlayers();
            currentTile.WorldItem = null;
            IsBeingUsed = false;
            gameObject.SetActive(false);
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
                PacketSender.SendWorldItemToPlayer(player, this);
            }
        }

        private void OnPlayerDisconnected(Player player)
        {
            player.Events.PlayerDisconnected -= OnPlayerDisconnected;
            sentToPlayers.Remove(player);
        }
        
        private void ClearSentToPlayers()
        {
            foreach (var player in sentToPlayers)
                player.Events.PlayerDisconnected -= OnPlayerDisconnected;
            
            sentToPlayers.Clear();
        }
    }
}
