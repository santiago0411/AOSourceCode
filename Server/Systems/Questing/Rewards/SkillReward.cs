using AO.Core.Utils;
using AO.Players;
using Newtonsoft.Json;
using PacketSender = AO.Network.PacketSender;

namespace AO.Systems.Questing.Rewards
{
    public sealed class SkillReward : IQuestReward
    {
        [JsonProperty("Skill")]
        private readonly Skill skill;
        [JsonProperty("Increase")]
        private readonly byte increase;
        
        public void AssignReward(Player toPlayer)
        {
            toPlayer.Skills[skill] += increase;
            
            if (toPlayer.Skills[skill] > Constants.MAX_PLAYER_SKILL)
                toPlayer.Skills[skill] = Constants.MAX_PLAYER_SKILL;
            
            PacketSender.SendMultiMessage(toPlayer.Id, MultiMessage.SkillLevelUp,  stackalloc[] {(int)skill, toPlayer.Skills[skill]});
        }
    }
}