using AO.Core.Ids;

namespace AOClient.Player.Utils
{
    public readonly ref struct SpawnPlayerInfo
    {
        public readonly ClientId ClientId;
        public readonly short MapNumber;
        public readonly string Username;
        public readonly string Description;
        public readonly byte Class;
        public readonly byte Race;
        public readonly byte Gender;
        public readonly byte Faction;
        public readonly bool IsGm;
        public readonly byte HeadId;

        public SpawnPlayerInfo(ClientId clientId,
            short mapNumber,
            string username,
            string description,
            byte @class,
            byte race,
            byte gender,
            byte faction,
            bool isGm,
            byte headId)
        {
            ClientId = clientId;
            MapNumber = mapNumber;
            Username = username;
            Description = description;
            Class = @class;
            Race = race;
            Gender = gender;
            Faction = faction;
            IsGm = isGm;
            HeadId = headId;
        }
    }
}