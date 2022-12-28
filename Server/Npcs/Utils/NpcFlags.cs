using AO.Players;

namespace AO.Npcs.Utils
{ 
    public class NpcFlags
    {
        public bool IsParalyzed;
        public bool IsImmobilized;
        public int ExperienceCount;
        public float LastSpellTime;
        public float LastAttackTime;
        public float ParalyzedTime;
        public Player AttackedFirstBy;
        public Player CombatOwner;
    }
}
