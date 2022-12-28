using AO.Players;

namespace AO.Core.Utils
{
    public static class TemplateCharacterUtils
    {
        public static CharacterInitialResources TemplateResourcesValues(ClassType @class, RaceType race)
        {
            var initialResources = new CharacterInitialResources();
            
            switch (@class)
            {
                case ClassType.Mage:
                    switch (race)
                    {
                        case RaceType.Human:
                            initialResources.Health = 335;
                            initialResources.Mana = 2484;
                            break;
                        case RaceType.Elf:
                            initialResources.Health = 313;
                            initialResources.Mana = 2898;
                            break;
                        case RaceType.NightElf:
                            initialResources.Health = 313;
                            initialResources.Mana = 2760;
                            break;
                        case RaceType.Dwarf:
                            initialResources.Health = 358;
                            initialResources.Mana = 2070;
                            break;
                        case RaceType.Gnome:
                            initialResources.Health = 290;
                            initialResources.Mana = 3036;
                            break;
                    }
                    break;
                case ClassType.Druid:
                    switch (race)
                    {
                        case RaceType.Human:
                            initialResources.Health = 380;
                            initialResources.Mana = 1670;
                            break;
                        case RaceType.Elf:
                            initialResources.Health = 335;
                            initialResources.Mana = 1940;
                            break;
                        case RaceType.NightElf:
                            initialResources.Health = 335;
                            initialResources.Mana = 1850;
                            break;
                        case RaceType.Dwarf:
                            initialResources.Health = 403;
                            initialResources.Mana = 1400;
                            break;
                        case RaceType.Gnome:
                            initialResources.Health = 313;
                            initialResources.Mana = 2030;
                            break;
                    }
                    break;
                case ClassType.Cleric:
                    switch (race)
                    {
                        case RaceType.Human:
                            initialResources.Health = 380;
                            initialResources.Mana = 1670;
                            break;
                        case RaceType.Elf:
                            initialResources.Health = 335;
                            initialResources.Mana = 1940;
                            break;
                        case RaceType.NightElf:
                            initialResources.Health = 335;
                            initialResources.Mana = 1850;
                            break;
                        case RaceType.Dwarf:
                            initialResources.Health = 425;
                            initialResources.Mana = 1400;
                            break;
                        case RaceType.Gnome:
                            initialResources.Health = 313;
                            initialResources.Mana = 2030;
                            break;
                    }
                    break;
                case ClassType.Bard:
                    switch (race)
                    {
                        case RaceType.Human:
                            initialResources.Health = 380;
                            initialResources.Mana = 1670;
                            break;
                        case RaceType.Elf:
                            initialResources.Health = 335;
                            initialResources.Mana = 1940;
                            break;
                        case RaceType.NightElf:
                            initialResources.Health = 335;
                            initialResources.Mana = 1850;
                            break;
                        case RaceType.Dwarf:
                            initialResources.Health = 425;
                            initialResources.Mana = 1400;
                            break;
                        case RaceType.Gnome:
                            initialResources.Health = 313;
                            initialResources.Mana = 2030;
                            break;
                    }
                    break;
                case ClassType.Paladin:
                    switch (race)
                    {
                        case RaceType.Human:
                            initialResources.Health = 425;
                            initialResources.Mana = 860;
                            break;
                        case RaceType.Elf:
                            initialResources.Health = 403;
                            initialResources.Mana = 995;
                            break;
                        case RaceType.NightElf:
                            initialResources.Health = 403;
                            initialResources.Mana = 950;
                            break;
                        case RaceType.Dwarf:
                            initialResources.Health = 470;
                            initialResources.Mana = 725;
                            break;
                        case RaceType.Gnome:
                            initialResources.Health = 380;
                            initialResources.Mana = 1040;
                            break;
                    }

                    break;
                case ClassType.Assassin:
                    switch (race)
                    {
                        case RaceType.Human:
                            initialResources.Health = 380;
                            initialResources.Mana = 860;
                            break;
                        case RaceType.Elf:
                            initialResources.Health = 358;
                            initialResources.Mana = 995;
                            break;
                        case RaceType.NightElf:
                            initialResources.Health = 358;
                            initialResources.Mana = 950;
                            break;
                        case RaceType.Dwarf:
                            initialResources.Health = 403;
                            initialResources.Mana = 725;
                            break;
                        case RaceType.Gnome:
                            initialResources.Health = 335;
                            initialResources.Mana = 1040;
                            break;
                    }
                    break;
                case ClassType.Warrior:
                    switch (race)
                    {
                        case RaceType.Human:
                            initialResources.Health = 470;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.Elf:
                            initialResources.Health = 448;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.NightElf:
                            initialResources.Health = 448;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.Dwarf:
                            initialResources.Health = 493;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.Gnome:
                            initialResources.Health = 425;
                            initialResources.Mana = 0;
                            break;
                    }
                    break;
                case ClassType.Hunter:
                    switch (race)
                    {
                        case RaceType.Human:
                            initialResources.Health = 425;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.Elf:
                            initialResources.Health = 403;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.NightElf:
                            initialResources.Health = 403;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.Dwarf:
                            initialResources.Health = 470;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.Gnome:
                            initialResources.Health = 380;
                            initialResources.Mana = 0;
                            break;
                    }
                    break;
                case ClassType.Worker:
                    switch (race)
                    {
                        case RaceType.Human:
                            initialResources.Health = 335;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.Elf:
                            initialResources.Health = 290;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.NightElf:
                            initialResources.Health = 290;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.Dwarf:
                            initialResources.Health = 358;
                            initialResources.Mana = 0;
                            break;
                        case RaceType.Gnome:
                            initialResources.Health = 268;
                            initialResources.Mana = 0;
                            break;
                    }
                    break;
            }

            initialResources.Stamina = 999;
            initialResources.Hunger = 100;
            initialResources.Thirst = 100;

            return initialResources;
        }

        public static int GetTemplateHit(ClassType @class)
        {
            switch (@class)
            {
                case ClassType.Mage:
                    return 46;
                case ClassType.Druid:
                case ClassType.Cleric:
                case ClassType.Bard:
                case ClassType.Worker:
                    return 90;
                case ClassType.Paladin:
                case ClassType.Assassin:
                    return 114;
                case ClassType.Warrior:
                case ClassType.Hunter:
                    return 124;
                default:
                    return 0;
            }
        }
    }
}
