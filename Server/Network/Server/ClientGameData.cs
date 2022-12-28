using System.Collections.Generic;
using AO.Core.Ids;
using AO.Players;
using AO.Players.Utils;

namespace AO.Network.Server
{
    public sealed class ClientGameData
    {
        /// <summary>Contains the account id of this particular client.</summary>
        public uint AccountId;
        /// <summary>Contains the characters id, name and game master rank of the account of this particular client.</summary>
        public Dictionary<CharacterId, AOCharacterInfo> AccountCharacters = new();

        /// <summary>Contains the player instance of this particular client.</summary>
        public Player Player;
    }
}
