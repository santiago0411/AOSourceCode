using System;
using AO.Core.Ids;

namespace AO.Players
{
    public class PlayerEvents
    {
        public event Action<Player> PlayerMoved;
        public event Action<NpcId> KilledNpc;
        public event Action<Player> KilledPlayer;
        public event Action<ItemId, ushort> ResourceGathered;
        public event Action<int> PlayerEnteredExploreArea;
        public event Action<Player> PlayerDisconnected;
        public event Action<Player> PlayerDied;
        public event Action<Player> PlayerRevived; 

        public void RaisePlayerMoved(Player player) => PlayerMoved?.Invoke(player);

        public void RaiseKilledNpc(NpcId npcId) => KilledNpc?.Invoke(npcId);

        public void RaiseKilledPlayer(Player playerKilled) => KilledPlayer?.Invoke(playerKilled);

        public void RaiseResourceGathered(ItemId itemId, ushort quantity) => ResourceGathered?.Invoke(itemId, quantity);

        public void RaisePlayerEnteredExploreArea(int areaId) => PlayerEnteredExploreArea?.Invoke(areaId);

        public void RaisePlayerDisconnected(Player player) => PlayerDisconnected?.Invoke(player);

        public void RaisePlayerDied(Player player) => PlayerDied?.Invoke(player);
        public void RaisePlayerRevived(Player player) => PlayerRevived?.Invoke(player);
    }
}