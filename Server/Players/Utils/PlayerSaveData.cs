using System;
using System.Collections.Generic;
using AO.Core;
using AO.Core.Ids;
using AO.Players.Talents.Worker;
using AO.Systems.Mailing;

namespace AO.Players.Utils
{
    public class PlayerSaveData
    {
        public readonly CharacterId CharId;
        public readonly string Name;
        public object PlayerData { get; private set; }
        public object WorkerTalentsData { get; private set; }
        public readonly List<Mail> MailsToUpdate = new();
        public readonly List<uint> MailsToDelete = new();

        public PlayerSaveData(Player player)
        {
            CharId = player.CharacterInfo.CharacterId;
            Name = player.CharacterInfo.CharacterName;
            Update(player);
        }

        public void Update(Player player)
        {
            PlayerData = GetPlayerDataObject(player);
            if (player.Class.ClassType == ClassType.Worker)
                WorkerTalentsData = GetWorkerTalentsDataObject(player);

            foreach (var mail in player.Flags.CachedMails.Values)
            {
                if (mail.ShouldBeDeleted)
                    MailsToDelete.Add(mail.Id);
                else if (mail.ShouldUpdateDatabase)
                    MailsToUpdate.Add(mail);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerSaveData other && other.CharId == CharId;
        }

        public override int GetHashCode()
        {
            return CharId.GetHashCode();
        }

        public static bool operator ==(PlayerSaveData lhs, PlayerSaveData rhs)
        {
            return lhs is not null && rhs is not null && lhs.CharId == rhs.CharId;
        }

        public static bool operator !=(PlayerSaveData lhs, PlayerSaveData rhs)
        {
            return !(lhs == rhs);
        }
        
        private static object GetPlayerDataObject(Player player)
        {
            return new
            {
                magic = player.Skills[Skill.Magic],
                armed_combat = player.Skills[Skill.ArmedCombat],
                ranged_weapons = player.Skills[Skill.RangedWeapons],
                unarmed_combat = player.Skills[Skill.UnarmedCombat],
                stabbing = player.Skills[Skill.Stabbing],
                combat_tactics = player.Skills[Skill.CombatTactics],
                magic_resistance = player.Skills[Skill.MagicResistance],
                shield_defense = player.Skills[Skill.ShieldDefense],
                meditation = player.Skills[Skill.Meditation],
                survival = player.Skills[Skill.Survival],
                animal_taming = player.Skills[Skill.AnimalTaming],
                hiding = player.Skills[Skill.Hiding],
                trading = player.Skills[Skill.Trading],
                thieving = player.Skills[Skill.Thieving],
                leadership = player.Skills[Skill.Leadership],
                sailing = player.Skills[Skill.Sailing],
                horse_riding = player.Skills[Skill.HorseRiding],
                mining = player.Skills[Skill.Mining],
                blacksmithing = player.Skills[Skill.Blacksmithing],
                woodcutting = player.Skills[Skill.Woodcutting],
                woodworking = player.Skills[Skill.Woodworking],
                fishing = player.Skills[Skill.Fishing],
                tailoring = player.Skills[Skill.Tailoring],
                level = player.Level,
                current_experience = player.CurrentExperience,
                assignable_skills = player.AssignableSkills,
                talent_points = player.AvailableTalentPoints,
                max_health = player.Health.MaxHealth,
                current_health = player.Health.CurrentHealth,
                max_mana = player.Mana.MaxAmount,
                current_mana = player.Mana.CurrentAmount,
                max_stamina = player.Stamina.MaxAmount,
                current_stamina = player.Stamina.CurrentAmount,
                max_hunger = player.Hunger.MaxAmount,
                current_hunger = player.Hunger.CurrentAmount,
                max_thirst = player.Thirst.MaxAmount,
                current_thirst = player.Thirst.CurrentAmount,
                hit = player.Hit,
                faction = player.Faction,
                has_guild = Convert.ToByte(player.HasGuild),
                guild_name = player.GuildName,
                criminals_killed = player.Stats[PlayerStat.CriminalsKilled],
                citizens_killed = player.Stats[PlayerStat.CitizensKilled],
                users_killed = player.Stats[PlayerStat.UsersKilled],
                npcs_killed = player.Stats[PlayerStat.NpcsKilled],
                deaths = player.Stats[PlayerStat.Deaths],
                remaining_jail_time = player.Stats[PlayerStat.RemainingJailTime],
                gold = player.Gold,
                bank_gold = player.BankGold,
                inventory = CharacterManager.ConvertInventoryToJson(player),
                spells = CharacterManager.ConvertSpellsToJson(player),
                quests_progresses = player.QuestManager.SerializeCurrentProgresses(),
                quests_completed = player.QuestManager.SerializeCompletedQuests(),
                map = player.CurrentMap.Number,
                x_pos = player.CurrentTile.Position.x,
                y_pos = player.CurrentTile.Position.y,
                description = player.Description
            };
        }
        
        private static object GetWorkerTalentsDataObject(Player player)
        {
            var miningTree = player.WorkerTalentTrees.MiningTree;
            var wcTree = player.WorkerTalentTrees.WoodCuttingTree;
            var fishingTree = player.WorkerTalentTrees.FishingTree;
            var bsTree = player.WorkerTalentTrees.BlacksmithingTree;
            var wwTree = player.WorkerTalentTrees.WoodWorkingTree;
            var tlTree = player.WorkerTalentTrees.TailoringTree;
            
            return new
            {
                fast_mining = miningTree.GetNode(MiningTalent.FastMining).Points,
                drop_less_ore = miningTree.GetNode(MiningTalent.DropLessOre).Points,
                mine_silver = miningTree.GetNode(MiningTalent.MineSilver).Points,
                mine_gold = miningTree.GetNode(MiningTalent.MineGold).Points,
                sentinel_chance_reduction_mining = miningTree.GetNode(MiningTalent.SentinelChanceReductionMining).Points,
                fast_cutting = wcTree.GetNode(WoodCuttingTalent.FastCutting).Points,
                drop_less_wood = wcTree.GetNode(WoodCuttingTalent.DropLessWood).Points,
                cut_elfic_wood = wcTree.GetNode(WoodCuttingTalent.CutElficWood).Points,
                sentinel_chance_reduction_woodcutting = wcTree.GetNode(WoodCuttingTalent.SentinelChanceReductionWoodCutting).Points,
                fish_pejerrey = fishingTree.GetNode(FishingTalent.FishPejerrey).Points,
                fish_hake = fishingTree.GetNode(FishingTalent.FishHake).Points,
                fish_swordfish = fishingTree.GetNode(FishingTalent.FishSwordFish).Points,
                use_fishing_net = fishingTree.GetNode(FishingTalent.UseFishingNet).Points,
                galley_fishing = fishingTree.GetNode(FishingTalent.GalleyFishing).Points,
                school_fishing = fishingTree.GetNode(FishingTalent.SchoolFishing).Points,
                sentinel_chance_reduction_fishing = fishingTree.GetNode(FishingTalent.SentinelChanceReductionFishing).Points,
                helmets_shields = bsTree.GetNode(BlacksmithingTalent.HelmetsShields).Points,
                weapons_staves = bsTree.GetNode(BlacksmithingTalent.WeaponsStaves).Points,
                armors = bsTree.GetNode(BlacksmithingTalent.Armors).Points,
                rings_magical = bsTree.GetNode(BlacksmithingTalent.RingsMagical).Points,
                arrows_bows = wwTree.GetNode(WoodWorkingTalent.ArrowsBows).Points,
                bolts_crossbows = wwTree.GetNode(WoodWorkingTalent.BoltsCrossbows).Points,
                boat = wwTree.GetNode(WoodWorkingTalent.Boat).Points,
                galley = wwTree.GetNode(WoodWorkingTalent.Galley).Points,
                lute_flutes = wwTree.GetNode(WoodWorkingTalent.LuteFlutes).Points,
                magical = wwTree.GetNode(WoodWorkingTalent.Magical).Points,
                wolf_skinning = tlTree.GetNode(TailoringTalent.WolfSkinning).Points,
                bear_skinning = tlTree.GetNode(TailoringTalent.BearSkinning).Points,
                polar_bear_skinning = tlTree.GetNode(TailoringTalent.PolarBearSkinning).Points,
                hats = tlTree.GetNode(TailoringTalent.Hats).Points,
                tunics = tlTree.GetNode(TailoringTalent.Tunics).Points
            };
        }
    }
}