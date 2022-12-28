using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AO.Core;
using AO.Core.Database;
using JetBrains.Annotations;

namespace AO.Players
{
    public class Race
    {
        public readonly RaceType RaceType;
        public readonly byte DefaultAnimationMale;
        public readonly byte DefaultAnimationFemale;
        
        public ReadOnlyDictionary<Attribute, sbyte> RaceModifiers { get; private set; }
        /// <summary>Contains all the attributes final values for the race. Used to restore back to the original value after buffing the player.</summary>
        public ReadOnlyDictionary<Attribute, byte> Attributes { get; private set; }
        public ReadOnlyCollection<byte> FemaleHeads { get; private set; }
        public ReadOnlyCollection<byte> MaleHeads { get; private set; }
        
        private Race() {}
        
        [UsedImplicitly]
        public Race(byte raceType, byte defaultAnimMale, byte defaultAnimFemale)
        {
            // This parameter has to be byte because underlying enum type is Int32 but db stores a byte so it will fail to match to this ctor
            RaceType = (RaceType)raceType; 
            DefaultAnimationMale = defaultAnimMale;
            DefaultAnimationFemale = defaultAnimFemale;
            LoadAttributes();
            LoadHeads();
        }

        private async void LoadAttributes()
        {
            IEnumerable<(byte, sbyte)> modifiers = await DatabaseOperations.FetchRaceAttributes((byte)RaceType);

            var raceModDic = new Dictionary<Attribute, sbyte>();
            var attDic = new Dictionary<Attribute, byte>();

            foreach (var (attId, attValue) in modifiers)
            {
                var att = (Attribute) attId;
                raceModDic.Add(att, attValue);
                //Add the base attribute value to the race modifier
                attDic.Add(att, (byte)(CharacterManager.Instance.BaseAttributesValues[att] + attValue));
            }

            RaceModifiers = new ReadOnlyDictionary<Attribute, sbyte>(raceModDic);
            Attributes = new ReadOnlyDictionary<Attribute, byte>(attDic);
        }

        private async void LoadHeads()
        {
            var maleHeads = await DatabaseOperations.FetchRaceHeads((byte)RaceType, (byte)Gender.Male);
            MaleHeads = new ReadOnlyCollection<byte>(maleHeads.ToList());

            var femaleHeads = await DatabaseOperations.FetchRaceHeads((byte)RaceType, (byte)Gender.Female);
            FemaleHeads = new ReadOnlyCollection<byte>(femaleHeads.ToList());
        }
    }
}
