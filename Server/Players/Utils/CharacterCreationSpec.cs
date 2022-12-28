using System.Collections.Generic;
using System.Threading;
using AO.Core.Database;
using AO.Network.Server;

namespace AO.Players.Utils
{
    public sealed class CharacterCreationSpec
    {
        public Client Client;
        public string CharacterName;
        public byte @Class;
        public byte Race;
        public byte HeadId;
        public byte Gender;
        public Dictionary<Skill, byte> Skills;
        public bool IsTemplate;
        
        public CancellationTokenSource CancellationTokenSource;
        public Transaction Transaction;
    }
}