using System.Collections.Generic;
using AO.Network.Server;
using AO.Players.Utils;
using PacketSender = AO.Network.PacketSender;

namespace AO.Players
{
    public class GameMaster : Player
    {
        public override bool Initialize(Client client, AOCharacterInfo characterInfo, IDictionary<string, object> playerInfo)
        {
            IsGameMaster = true;
            return base.Initialize(client, characterInfo, playerInfo);
        }

        protected override void OnFixedUpdate()
        {
            if (PlayerMethods.RecoverStamina(this))
                PacketSender.PlayerIndividualResource(this, Resource.Stamina);
        }
    }
}
