using AO.Core.Utils;
using JetBrains.Annotations;

namespace AO.Players
{
	public sealed class Class
    {
        public ClassType ClassType => (ClassType)id;
        
        [UsedImplicitly]
        public readonly float ModHealth;

        [UsedImplicitly]
        public readonly float ModEvasion;

        [UsedImplicitly]
        public readonly float ModWeaponAttack;

        [UsedImplicitly]
        public readonly float ModRangedAttack;

        [UsedImplicitly]
        public readonly float ModUnarmedAttack;

        [UsedImplicitly]
        public readonly float ModWeaponDamage;

        [UsedImplicitly]
        public readonly float ModRangedDamage;

        [UsedImplicitly]
        public readonly float ModUnarmedDamage;

        [UsedImplicitly]
        public readonly float ModShield;

        [UsedImplicitly]
        public readonly float ModMagicResist;

        public bool IsMagical => ClassType is not (ClassType.Warrior or ClassType.Hunter or ClassType.Worker);

        private readonly byte id;

        public static void CheckHitOverflow(Player player)
        {
            if (player.Level < 36)
            {
                if (player.Hit > Constants.MAX_PLAYER_HIT_UNDER36)
                    player.Hit = Constants.MAX_PLAYER_HIT_UNDER36;
            }
            else
            {
                if (player.Hit > Constants.MAX_PLAYER_HIT_OVER36)
                    player.Hit = Constants.MAX_PLAYER_HIT_OVER36;
            }
        }

        public static Utils.LevelUpStatsIncrease CalculateLevelUpStats(Player player)
        {
            int hpIncrease = 0, manaIncrease = 0, staminaIncrease = 0, hitIncrease = 0;

            switch (player.Class.ClassType)
            {
                case ClassType.Mage:
                    hpIncrease = CalculateMageHpIncrease(player);
                    manaIncrease = 3 * player.Race.Attributes[Attribute.Intellect];
                    staminaIncrease = Constants.DEFAULT_STAM_INCREASE;
                    hitIncrease = 1;
                    break;

                case ClassType.Druid:
                    hpIncrease = CalculateDruidHpIncrease(player);
                    manaIncrease = 2 * player.Race.Attributes[Attribute.Intellect];
                    staminaIncrease = Constants.DEFAULT_STAM_INCREASE;
                    hitIncrease = 2;
                    break;

                case ClassType.Cleric:
                    hpIncrease = CalculateClericHpIncrease(player);
                    manaIncrease = 2 * player.Race.Attributes[Attribute.Intellect];
                    staminaIncrease = Constants.DEFAULT_STAM_INCREASE;
                    hitIncrease = 2;
                    break;

                case ClassType.Bard:
                    hpIncrease = CalculateBardHpIncrease(player);
                    manaIncrease = 2 * player.Race.Attributes[Attribute.Intellect];
                    staminaIncrease = Constants.DEFAULT_STAM_INCREASE;
                    hitIncrease = 2;
                    break;

                case ClassType.Paladin:
                    hpIncrease = CalculatePaladinHpIncrease(player);
                    manaIncrease = player.Race.Attributes[Attribute.Intellect];
                    staminaIncrease = Constants.DEFAULT_STAM_INCREASE;
                    hitIncrease = player.Level > 35 ? 1 : 3;
                    break;

                case ClassType.Assassin:
                    hpIncrease = CalculateAssassinHpIncrease(player);
                    manaIncrease = player.Race.Attributes[Attribute.Intellect];
                    staminaIncrease = Constants.DEFAULT_STAM_INCREASE;
                    hitIncrease = player.Level > 35 ? 1 : 3;
                    break;

                case ClassType.Warrior:
                    hpIncrease = CalculateWarriorHpIncrease(player);
                    staminaIncrease = Constants.DEFAULT_STAM_INCREASE;
                    hitIncrease = player.Level > 35 ? 2 : 3;
                    break;

                case ClassType.Hunter:
                    hpIncrease = CalculateHunterHpIncrease(player);
                    staminaIncrease = Constants.DEFAULT_STAM_INCREASE;
                    hitIncrease = player.Level > 35 ? 2 : 3;
                    break;

                case ClassType.Worker:
                    hpIncrease = CalculateWorkerHpIncrease(player);
                    staminaIncrease = Constants.WORKER_STAM_INCREASE;
                    hitIncrease = 2;
                    break;
            }

            return new Utils.LevelUpStatsIncrease(hpIncrease, manaIncrease, staminaIncrease, hitIncrease);
        }

        private static int CalculateMageHpIncrease(Player player)
        {
            return player.Race.Attributes[Attribute.Constitution] switch
            {
                21 => ExtensionMethods.RandomNumber(6, 9),
                20 => ExtensionMethods.RandomNumber(5, 9),
                19 => ExtensionMethods.RandomNumber(4, 9),
                18 => ExtensionMethods.RandomNumber(4, 8),
                _ => ExtensionMethods.RandomNumber(5, player.Race.Attributes[Attribute.Constitution] / 2) - Constants.HUNTER_ADDITIONAL_HP,
            };
        }

        private static int CalculateDruidHpIncrease(Player player)
        {
            return player.Race.Attributes[Attribute.Constitution] switch
            {
                21 => ExtensionMethods.RandomNumber(7, 10),
                20 => ExtensionMethods.RandomNumber(6, 10),
                19 => ExtensionMethods.RandomNumber(5, 9),
                18 => ExtensionMethods.RandomNumber(4, 9),
                _ => ExtensionMethods.RandomNumber(4, player.Race.Attributes[Attribute.Constitution] / 2),
            };
        }

        private static int CalculateClericHpIncrease(Player player)
        {
            return player.Race.Attributes[Attribute.Constitution] switch
            {
                21 => ExtensionMethods.RandomNumber(7, 11),
                20 => ExtensionMethods.RandomNumber(6, 10),
                19 => ExtensionMethods.RandomNumber(5, 9),
                18 => ExtensionMethods.RandomNumber(4, 9),
                _ => ExtensionMethods.RandomNumber(4, player.Race.Attributes[Attribute.Constitution] / 2),
            };
        }

        private static int CalculateBardHpIncrease(Player player)
        {
            return player.Race.Attributes[Attribute.Constitution] switch
            {
                21 => ExtensionMethods.RandomNumber(7, 10),
                20 => ExtensionMethods.RandomNumber(6, 10),
                19 => ExtensionMethods.RandomNumber(5, 9),
                18 => ExtensionMethods.RandomNumber(4, 9),
                _ => ExtensionMethods.RandomNumber(4, player.Race.Attributes[Attribute.Constitution] / 2),
            };
        }

        private static int CalculatePaladinHpIncrease(Player player)
        {
            return player.Race.Attributes[Attribute.Constitution] switch
            {
                21 => ExtensionMethods.RandomNumber(9, 11),
                20 => ExtensionMethods.RandomNumber(7, 11),
                19 => ExtensionMethods.RandomNumber(6, 11),
                18 => ExtensionMethods.RandomNumber(6, 10),
                _ => ExtensionMethods.RandomNumber(4, player.Race.Attributes[Attribute.Constitution] / 2) + Constants.WAR_ADDITIONAL_HP,
            };
        }

        private static int CalculateAssassinHpIncrease(Player player)
        {
            return player.Race.Attributes[Attribute.Constitution] switch
            {
                21 => ExtensionMethods.RandomNumber(7, 10),
                20 => ExtensionMethods.RandomNumber(6, 10),
                19 => ExtensionMethods.RandomNumber(6, 9),
                18 => ExtensionMethods.RandomNumber(5, 9),
                _ => ExtensionMethods.RandomNumber(4, player.Race.Attributes[Attribute.Constitution] / 2),
            };
        }

        private static int CalculateWarriorHpIncrease(Player player)
        {
            return player.Race.Attributes[Attribute.Constitution] switch
            {
                21 => ExtensionMethods.RandomNumber(9, 12),
                20 => ExtensionMethods.RandomNumber(8, 12),
                19 => ExtensionMethods.RandomNumber(8, 11),
                18 => ExtensionMethods.RandomNumber(8, 10),
                _ => ExtensionMethods.RandomNumber(6, player.Race.Attributes[Attribute.Constitution] / 2) + Constants.WAR_ADDITIONAL_HP,
            };
        }

        private static int CalculateHunterHpIncrease(Player player)
        {
            return player.Race.Attributes[Attribute.Constitution] switch
            {
                21 => ExtensionMethods.RandomNumber(9, 11),
                20 => ExtensionMethods.RandomNumber(7, 11),
                19 => ExtensionMethods.RandomNumber(6, 11),
                18 => ExtensionMethods.RandomNumber(6, 10),
                _ => ExtensionMethods.RandomNumber(6, player.Race.Attributes[Attribute.Constitution] / 2),
            };
        }

        private static int CalculateWorkerHpIncrease(Player player)
        {
            return player.Race.Attributes[Attribute.Constitution] switch
            {
                21 => ExtensionMethods.RandomNumber(6, 9),
                20 => ExtensionMethods.RandomNumber(5, 9),
                19 => ExtensionMethods.RandomNumber(4, 8),
                18 => ExtensionMethods.RandomNumber(4, 7),
                _ => ExtensionMethods.RandomNumber(5, player.Race.Attributes[Attribute.Constitution] / 2),
            };
        }
    }
}
