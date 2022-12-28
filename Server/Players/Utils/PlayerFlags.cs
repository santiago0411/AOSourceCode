using System.Collections.Generic;
using AO.Core.Ids;
using AO.Npcs;
using AO.Systems;
using AO.Systems.Mailing;
using AO.World;

namespace AO.Players.Utils
{
    public class PlayerFlags
    {
        public bool IsDead = false;
        public bool IsMeditating = false;
        public bool IsHungry = false;
        public bool IsThirsty = false;
        public bool IsWorking = false;
        public bool IsEnvenomed = false;
        public int BleedingTicksRemaining = 0;
        public bool IsParalyzed = false;
        public bool IsImmobilized = false;
        public bool IsInvisible = false;
        public bool IsHidden = false;
        public bool IsResting = true;
        public bool IsSailing = false;
        public bool CanInvis = true;
        public bool SafeToggleOn = true;
        public bool RessToggleOn = true;
        public byte SelectedSpell = 0;
        public bool Disconnecting = false;
        public bool HasDisconnected = false;
        
        // TODO ideally move these somewhere else since they aren't really "flags"
        public NpcId CurrentPetNpcId = NpcId.Empty;
        public Party PendingPartyInvite = null;
        public QuestId SelectedQuestId;
        public ItemId SelectedItemRewardId;
        public readonly HashSet<ushort> CraftableItemsSent = new();
        public readonly HashSet<NpcId> NpcsQuestsSent = new();
        public readonly Dictionary<ushort, List<ushort>> TurnInQuestNpcsSent = new();
        public readonly Dictionary<uint, Mail> CachedMails = new();
        public float MailCacheExpirationTime = 0;

        public Npc InteractingWithNpc;
        public Npc TargetNpc;
        public Npc OwnedNpc;

        public CharacterId LastCitizenKilled;
        public CharacterId LastCriminalKilled;

        public ZoneType ZoneType = ZoneType.SafeZone;
    }
}