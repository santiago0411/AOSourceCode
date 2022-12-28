using AO.Core.Ids;

namespace AO.Players.Utils
{
    public readonly struct AOCharacterInfo
    {
        public readonly CharacterId CharacterId;
        public readonly string CharacterName;
        public readonly GameMasterRank? GameMasterRank;

        public AOCharacterInfo(CharacterId charId, string charName, GameMasterRank? gmRank = null)
        {
            CharacterId = charId;
            CharacterName = charName;
            GameMasterRank = gmRank;
        }
    }
}
