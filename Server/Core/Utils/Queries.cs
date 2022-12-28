using System.Collections.Generic;

namespace AO.Core.Utils
{
    public static class Queries
    {
        public const byte CREATE_CHARACTER_QUERY_PARAMS_COUNT = 42;

        public static object GetInsertCharacterObject(Dictionary<CharactersTableColumn, object> values)
        {
            return new
            {
                account_id =    	values[CharactersTableColumn.AccountId],
                username =    		values[CharactersTableColumn.Username],
                @class =      		values[CharactersTableColumn.Class],
                race =    			values[CharactersTableColumn.Race],
                gender =       		values[CharactersTableColumn.Gender],
                head_id =     		values[CharactersTableColumn.HeadId],
                magic = 			values[CharactersTableColumn.Magic],
                armed_combat =  	values[CharactersTableColumn.ArmedCombat],
                ranged_weapons =    values[CharactersTableColumn.RangedWeapons],
                unarmed_combat =    values[CharactersTableColumn.UnarmedCombat],
                stabbing =  		values[CharactersTableColumn.Stabbing],
                combat_tactics =    values[CharactersTableColumn.CombatTactics],
                magic_resistance =  values[CharactersTableColumn.MagicResistance],
                shield_defense =    values[CharactersTableColumn.ShieldDefense],
                meditation =    	values[CharactersTableColumn.Meditation],
                survival =  		values[CharactersTableColumn.Survival],
                animal_taming = 	values[CharactersTableColumn.AnimalTaming],
                hiding =    		values[CharactersTableColumn.Hiding],
                trading =   		values[CharactersTableColumn.Trading],
                thieving =  		values[CharactersTableColumn.Thieving],
                leadership =    	values[CharactersTableColumn.Leadership],
                sailing =       	values[CharactersTableColumn.Sailing],
                horse_riding =  	values[CharactersTableColumn.HorseRiding],
                mining =        	values[CharactersTableColumn.Mining],
                blacksmithing = 	values[CharactersTableColumn.Blacksmithing],
                woodcutting =   	values[CharactersTableColumn.WoodCutting],
                woodworking =   	values[CharactersTableColumn.WoodWorking],
                fishing =   		values[CharactersTableColumn.Fishing],
                tailoring = 		values[CharactersTableColumn.Tailoring],
                max_health =    	values[CharactersTableColumn.MaxHealth],
                current_health =    values[CharactersTableColumn.CurrentHealth],
                max_mana =  		values[CharactersTableColumn.MaxMana],
                current_mana =  	values[CharactersTableColumn.CurrentMana],
                max_stamina =   	values[CharactersTableColumn.MaxStamina],
                current_stamina =   values[CharactersTableColumn.CurrentStamina],
                max_hunger =    	values[CharactersTableColumn.MaxHunger],
                current_hunger =    values[CharactersTableColumn.CurrentHunger],
                max_thirst =    	values[CharactersTableColumn.MaxThirst],
                current_thirst =    values[CharactersTableColumn.CurrentThirst],
                map =   			values[CharactersTableColumn.Map],  
                x_pos = 			values[CharactersTableColumn.XPos],
                y_pos = 			values[CharactersTableColumn.YPos]
            };
        }
    }
}