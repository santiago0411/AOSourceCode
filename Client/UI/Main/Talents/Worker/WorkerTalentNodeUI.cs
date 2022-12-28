using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.UI.Utils;
using UnityEngine;

namespace AOClient.UI.Main.Talents.Worker
{
    public class WorkerTalentNodeUI : TalentNodeUIBase
    {
        [SerializeField] private Profession profession;

        [SerializeField] 
        [ConditionalShow(nameof(profession), (int)Profession.Mining)]
        private MiningTalent miningTalent;
        
        [SerializeField] 
        [ConditionalShow(nameof(profession), (int)Profession.WoodCutting)]
        private WoodCuttingTalent woodCuttingTalent;
        
        [SerializeField] 
        [ConditionalShow(nameof(profession), (int)Profession.Fishing)]
        private FishingTalent fishingTalent;
        
        [SerializeField] 
        [ConditionalShow(nameof(profession), (int)Profession.Blacksmithing)]
        private BlacksmithingTalent blacksmithingTalent;
        
        [SerializeField] 
        [ConditionalShow(nameof(profession), (int)Profession.Woodworking)]
        private WoodWorkingTalent woodWorkingTalent;
        
        [SerializeField] 
        [ConditionalShow(nameof(profession), (int)Profession.Tailoring)]
        private TailoringTalent tailoringTalent;

        private byte nodeId;

        protected override void Start()
        {
            base.Start();
            nodeId = profession switch
            {
                Profession.Mining => (byte)miningTalent,
                Profession.WoodCutting => (byte)woodCuttingTalent,
                Profession.Fishing => (byte)fishingTalent,
                Profession.Blacksmithing => (byte)blacksmithingTalent,
                Profession.Woodworking => (byte)woodWorkingTalent,
                Profession.Tailoring => (byte)tailoringTalent,
                _ => byte.MaxValue
            };
        }

        protected override void SendCanSkillUpPacket()
        {
            PacketSender.CanSkillUpTalent(profession, nodeId);
        }
        
        protected override void WriteSkillUpTalent(Packet packet)
        {
            packet.Write((byte)profession);
            packet.Write(nodeId);
            packet.Write(NotCommittedPoints);
        }
    }
}