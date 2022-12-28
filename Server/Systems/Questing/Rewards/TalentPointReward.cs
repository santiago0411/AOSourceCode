using AO.Core.Utils;
using AO.Network;
using AO.Players;
using Newtonsoft.Json;

namespace AO.Systems.Questing.Rewards
{
    public class TalentPointReward : IQuestReward
    {
        [JsonProperty("Points")] 
        private readonly byte points;
        
        public void AssignReward(Player toPlayer)
        {
            toPlayer.AvailableTalentPoints += points;
            PacketSender.SendMultiMessage(toPlayer.Id, MultiMessage.TalentPointsObtained, stackalloc int[] {points});
        }
    }
}