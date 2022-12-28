using AO.Core.Ids;
using AOClient.Network;
using AOClient.Player;
using AOClient.Player.Utils;
using AOClient.UI;
using AOClient.UI.Main;

namespace AOClient.Core.Utils
{
    public static class MultiMessageWriter
    {
        private static ConsoleUI Console => UIManager.GameUI.Console;
        
        public static void WriteMultiMessage(Packet packet)
        {
            var multiMessage = (MultiMessage)packet.ReadByte();
            if (WriteCombatMultiMessage(multiMessage, packet)) return;
            if (WriteNpcMultiMessage(multiMessage, packet)) return;
            if (WriteItemsMultiMessage(multiMessage, packet)) return;
            if (WriteSpellsMultiMessage(multiMessage, packet)) return;
            if (WriteFactionMultiMessage(multiMessage)) return;
            if (WriteProfessionMultiMessage(multiMessage, packet)) return;
            if (WriteLevelingMultiMessage(multiMessage, packet)) return;
            if (WritePartyMultiMessage(multiMessage, packet)) return;
            if (WriteQuestingMultiMessage(multiMessage)) return;
            if (WriteMailingMultiMessage(multiMessage, packet)) return;
            WriteMiscMultiMessage(multiMessage, packet);
        }

        private static bool WriteCombatMultiMessage(MultiMessage multiMessage, Packet packet)
        {
            BodyPart bodyPart;
            int damage;
            string username;
            
            switch (multiMessage)
            {
                case MultiMessage.AttackerAppliedBleed:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.AttackerAppliedBleed(username), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.AttackerEnvenomed:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.AttackerEnvenomed(username), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.BlockedWithShieldOther:
                    Console.WriteLine(Constants.OTHER_PLAYER_PARRIED_ATTACK_SHIELD, ConsoleMessage.Combat);
                    return true;
                case MultiMessage.BlockedWithShieldPlayer:
                    Console.WriteLine(Constants.ATTACK_PARRIED_SHIELD, ConsoleMessage.Combat);
                    return true;
                case MultiMessage.CantAttackInSafeZone:
                    Console.WriteLine(Constants.CANT_ATTACK_IN_SAFE_ZONE);
                    return true;
                case MultiMessage.CantAttackOwnPet:
                    Console.WriteLine(Constants.CANT_ATTACK_OWN_PET);
                    return true;
                case MultiMessage.CantAttackSpirit:
                    Console.WriteLine(Constants.CANT_ATTACK_SPIRIT);
                    return true;
                case MultiMessage.CantAttackThatNpc:
                    Console.WriteLine(Constants.CANT_ATTACK_THAT_NPC);
                    return true;
                case MultiMessage.CantAttackYourself:
                    Console.WriteLine(Constants.CANT_ATTACK_YOURSELF);
                    return true;
                case MultiMessage.KilledNpc:
                    Console.WriteLine(Constants.KILLED_NPC, ConsoleMessage.Combat);
                    return true;
                case MultiMessage.KilledPlayer:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.KilledPlayer(username), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.NoAmmo:
                    Console.WriteLine(Constants.NO_AMMO);
                    return true;
                case MultiMessage.NpcHitPlayer:
                    bodyPart = (BodyPart)packet.ReadByte();
                    damage = packet.ReadInt();
                    WriteNpcHitPlayer(bodyPart, damage);
                    return true;
                case MultiMessage.NpcKilledPlayer:
                    Console.WriteLine(Constants.NPC_KILLED_PLAYER, ConsoleMessage.Combat);
                    return true;
                case MultiMessage.NpcDamageSpellPlayer:
                    string npcName = GameManager.Instance.GetNpcInfo(packet.ReadNpcId()).Name;
                    string spellName = GameManager.Instance.GetSpell(packet.ReadSpellId()).Name;
                    damage = packet.ReadInt();
                    Console.WriteLine(Constants.NpcCastSpellOnPlayer(npcName, spellName), ConsoleMessage.Combat);
                    Console.WriteLine(Constants.NpcDamageSpellPlayer(npcName, damage), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.NpcEnvenomedPlayer:
                    Console.WriteLine(Constants.NPC_ENVENOMED_PLAYER, ConsoleMessage.Combat);
                    return true;
                case MultiMessage.NpcSwing:
                    Console.WriteLine(Constants.NPC_FAILED_HIT, ConsoleMessage.Combat);
                    return true;
                case MultiMessage.PlayerAttackedSwing:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.EnemyAttackMissed(username), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.PlayerDamageSpellNpc:
                    damage = packet.ReadInt();
                    Console.WriteLine(Constants.PlayerDamageSpellNpc(damage), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.PlayerDamageSpellEnemy:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    damage = packet.ReadInt();
                    Console.WriteLine(Constants.PlayerDamageSpellEnemy(damage, username), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.EnemyDamageSpellPlayer:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    damage = packet.ReadInt();
                    Console.WriteLine(Constants.EnemyDamageSpellPlayer(damage, username), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.PlayerGotStabbed:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    damage = packet.ReadInt();
                    Console.WriteLine(Constants.PlayerGotStabbed(username, damage), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.PlayerHitByPlayer:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    bodyPart = (BodyPart)packet.ReadByte();
                    damage = packet.ReadInt();
                    WritePlayerHitByPlayer(username, bodyPart, damage);
                    return true;
                case MultiMessage.PlayerHitNpc:
                    Console.WriteLine(Constants.PlayerHitNpc(packet.ReadInt()), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.PlayerHitPlayer:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    bodyPart = (BodyPart)packet.ReadByte();
                    damage = packet.ReadInt();
                    WritePlayerHitPlayer(username, bodyPart, damage);
                    return true;
                case MultiMessage.PlayerKilled:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.HasKilledYou(username), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.PlayerSwing:
                    Console.WriteLine(Constants.ATTACK_MISSED, ConsoleMessage.Combat);
                    return true;
                case MultiMessage.StabbedNpc:
                    damage = packet.ReadInt();
                    Console.WriteLine(Constants.StabbedNpc(damage), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.StabbedPlayer:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    damage = packet.ReadInt();
                    Console.WriteLine(Constants.StabbedPlayer(username, damage), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.TargetGotBled:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.TargetGotBled(username), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.TargetGotEnvenomed:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.TargetGotEnvenomed(username), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.TooFarToAttack:
                    Console.WriteLine(Constants.TOO_FAR_TO_ATTACK);
                    return true;
                case MultiMessage.YouAreBleeding:
                    Console.WriteLine(Constants.YOU_ARE_BLEEDING, ConsoleMessage.Warning);
                    return true;
                case MultiMessage.YouAreEnvenomed:
                    var messageAux = GameManager.Instance.LocalPlayer.Gender == Gender.Female ? Constants.FEMALE_YOU_ARE_ENVENOMED : Constants.MALE_YOU_ARE_ENVENOMED;
                    Console.WriteLine(messageAux, ConsoleMessage.Warning);
                    return true;
            }

            return false;
        }

        private static bool WriteNpcMultiMessage(MultiMessage multiMessage, Packet packet)
        {
            switch (multiMessage)
            {
                case MultiMessage.AlreadyTamedThatNpc:
                    Console.WriteLine(Constants.ALREADY_TAMED_THAT_NPC);
                    return true;
                case MultiMessage.CantSummonPetInSafeZone:
                    Console.WriteLine(Constants.CANT_SUMMON_PET_IN_SAFE_ZONE);
                    return true;
                case MultiMessage.AlreadyHaveAPet:
                    Console.WriteLine(Constants.ALREADY_HAVE_A_PET);
                    return true;
                case MultiMessage.CantTameNpc:
                    Console.WriteLine(Constants.CANT_TAME_NPC);
                    return true;
                case MultiMessage.CantTameNpcInCombat:
                    Console.WriteLine(Constants.CANT_TAME_NPC_IN_COMBAT);
                    return true;
                case MultiMessage.DisplayInfo:
                    Console.WriteLine(ReadDisplayNpcInfo(packet));
                    return true;
                case MultiMessage.FailedToTameNpc:
                    Console.WriteLine(Constants.FAILED_TO_TAME_NPC);
                    return true;
                case MultiMessage.NoNpcToTame:
                    Console.WriteLine(Constants.NO_NPC_TO_TAME);
                    return true;
                case MultiMessage.NpcAlreadyHasOwner:
                    Console.WriteLine(Constants.NPC_ALREADY_HAS_OWNER);
                    return true;
                case MultiMessage.ShowNpcDescription:
                    GameManager.Instance.NpcsPool.FindObject(packet.ReadInt()).ShowDescription();
                    return true;
                case MultiMessage.SuccessfullyTamedNpc:
                    Console.WriteLine(Constants.SUCCESSFULLY_TAMED_NPC);
                    return true;
                case MultiMessage.TooFarToTame:
                    Console.WriteLine(Constants.TOO_FAR_TO_TAME);
                    return true;
                case MultiMessage.PetIgnoreCommand:
                    Console.WriteLine(Constants.PET_IGNORE_COMMAND);
                    return true;
            }

            return false;
        }
        
        private static bool WriteItemsMultiMessage(MultiMessage multiMessage, Packet packet)
        {
            string username, itemName;
            ushort quantity;
            
            switch (multiMessage)
            {
                case MultiMessage.BlackPotionOne:
                    Console.WriteLine(Constants.BLACK_POTION_ONE);
                    return true;
                case MultiMessage.BlackPotionTwo:
                    Console.WriteLine(Constants.BLACK_POTION_TWO);
                    return true;
                case MultiMessage.CantUseAxeAndShield:
                    Console.WriteLine(Constants.CANT_USE_AXE_AND_SHIELD);
                    return true;
                case MultiMessage.CantUseClass:
                    Console.WriteLine(Constants.CANT_USE_CLASS);
                    return true;
                case MultiMessage.CantUseFaction:
                    Console.WriteLine(Constants.CANT_USE_FACTION);
                    return true;
                case MultiMessage.CantUseGender:
                    Console.WriteLine(Constants.CANT_USE_GENDER);
                    return true;
                case MultiMessage.CantUseRace:
                    Console.WriteLine(Constants.CANT_USE_RACE);
                    return true;
                case MultiMessage.CantUseWhileMeditating:
                    Console.WriteLine(Constants.CANT_USE_MEDITATING, ConsoleMessage.Warning);
                    return true;
                case MultiMessage.CantUseWeaponLikeThat:
                    Console.WriteLine(Constants.CANT_USE_WEAPON_LIKE_THAT, ConsoleMessage.Warning);
                    return true;
                case MultiMessage.InventoryFull:
                    Console.WriteLine(Constants.INVENTORY_FULL);
                    return true;
                case MultiMessage.ItemOnlyNewbies:
                    Console.WriteLine(Constants.ITEM_ONLY_NEWBIES);
                    return true;
                case MultiMessage.MustEquipItemFirst:
                    Console.WriteLine(Constants.MUST_EQUIP_ITEM_FIRST, ConsoleMessage.Warning);
                    return true;
                case MultiMessage.NotEnoughMoney:
                    Console.WriteLine(Constants.NOT_ENOUGH_MONEY);
                    return true;
                case MultiMessage.NotEnoughSkillToUse:
                    var skill = (Skill)packet.ReadByte();
                    Console.WriteLine(Constants.NotEnoughSkillToUse(skill));
                    return true;
                case MultiMessage.CantUseTalent:
                    Console.WriteLine(Constants.CANT_USE_TALENT);
                    return true;
                case MultiMessage.NoSpaceToDropItem:
                    Console.WriteLine(Constants.NO_SPACE_TO_DROP_ITEM);
                    return true;
                case MultiMessage.PlayerDroppedItemTo:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    itemName = GameManager.Instance.GetItem(packet.ReadItemId()).Name;
                    quantity = packet.ReadUShort();
                    Console.WriteLine(Constants.PlayerDroppedItemTo(username, itemName, quantity));
                    return true;
                case MultiMessage.PlayerGotItemDropped:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    itemName = GameManager.Instance.GetItem(packet.ReadItemId()).Name;
                    quantity = packet.ReadUShort();
                    Console.WriteLine(Constants.PlayerGotItemDropped(username, itemName, quantity));
                    return true;
            }

            return false;
        }
        
        private static bool WriteSpellsMultiMessage(MultiMessage multiMessage, Packet packet)
        {
            Spell spell;
            int healing;
            string username;
            
            switch (multiMessage)
            {
                case MultiMessage.CantCastDead:
                    Console.WriteLine(Constants.CANT_CAST_DEAD);
                    return true;
                case MultiMessage.CantCastOnSpirit:
                    Console.WriteLine(Constants.CANT_CAST_ON_SPIRIT);
                    return true;
                case MultiMessage.CantLearnMoreSpells:
                    Console.WriteLine(Constants.CANT_LEARN_MORE_SPELLS);
                    return true;
                case MultiMessage.InvalidTarget:
                    Console.WriteLine(Constants.INVALID_TARGET);
                    return true;
                case MultiMessage.MagicItemNotEquipped:
                    Console.WriteLine(Constants.MAGIC_ITEM_NOT_EQUIPPED);
                    return true;
                case MultiMessage.MagicItemNotPowerfulEnough:
                    Console.WriteLine(Constants.MAGIC_ITEM_NOT_POWERFUL_ENOUGH);
                    return true;
                case MultiMessage.NotEnoughMana:
                    Console.WriteLine(Constants.NOT_ENOUGH_MANA);
                    return true;
                case MultiMessage.NotEnoughSkillToCast:
                    Console.WriteLine(Constants.NOT_ENOUGH_SKILL_TO_CAST);
                    return true;
                case MultiMessage.NotEnoughStaminaToCast:
                    var messageAux = GameManager.Instance.LocalPlayer.Gender == Gender.Female ? Constants.SPELL_NOT_ENOUGH_STAM_FEMALE : Constants.SPELL_NOT_ENOUGH_STAM_MALE;
                    Console.WriteLine(messageAux);
                    return true;
                case MultiMessage.NpcImmuneToSpell:
                    Console.WriteLine(Constants.NPC_IMMUNE_TO_SPELL);
                    return true;
                case MultiMessage.NpcsOnlySpell:
                    Console.WriteLine(Constants.NPCS_ONLY_SPELL);
                    return true;
                case MultiMessage.NpcHealedPlayer:
                    string npcName = GameManager.Instance.GetNpcInfo(packet.ReadNpcId()).Name;
                    healing = packet.ReadInt();
                    Console.WriteLine(Constants.NpcHealedPlayer(npcName, healing), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.PlayerHealedNpc:
                    healing = packet.ReadInt();
                    Console.WriteLine(Constants.PlayerHealedNpc(healing), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.PlayerHealed:
                    healing = packet.ReadInt();
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.PlayerHealed(healing, username), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.PlayerGotHealed:
                    healing = packet.ReadInt();
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.PlayerGotHealed(healing, username), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.PlayerSelfHeal:
                    healing = packet.ReadInt();
                    Console.WriteLine(Constants.PlayerSelfHeal(healing), ConsoleMessage.Combat);
                    return true;
                case MultiMessage.SpellAlreadyLearned:
                    Console.WriteLine(Constants.SPELL_ALREADY_LEARNED);
                    return true;
                case MultiMessage.SpellMessage:
                    spell = GameManager.Instance.GetSpell(packet.ReadSpellId());
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine($"{spell.Message} {username}.", ConsoleMessage.Combat);
                    return true;
                case MultiMessage.SpellSelfMessage:
                    spell = GameManager.Instance.GetSpell(packet.ReadSpellId());
                    Console.WriteLine($"{spell.SelfMessage}", ConsoleMessage.Combat);
                    return true;
                case MultiMessage.SpellTargetMessage:
                    spell = GameManager.Instance.GetSpell(packet.ReadSpellId());
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine($"{username} {spell.TargetMessage}", ConsoleMessage.Combat);
                    return true;
                case MultiMessage.StaffNotEquipped:
                    Console.WriteLine(Constants.STAFF_NOT_EQUIPPED);
                    return true;
                case MultiMessage.StaffNotPowerfulEnough:
                    Console.WriteLine(Constants.STAFF_NOT_POWERFUL_ENOUGH);
                    return true;
                case MultiMessage.TargetRessToggledOff:
                    Console.WriteLine(Constants.TARGET_RESS_TOGGLED_OFF);
                    return true;
                case MultiMessage.TooFarToCast:
                    Console.WriteLine(Constants.TOO_FAR_TO_CAST);
                    return true;
                case MultiMessage.UsersOnlySpell:
                    Console.WriteLine(Constants.USERS_ONLY_SPELL);
                    return true;
            }

            return false;
        }
        
        private static bool WriteFactionMultiMessage(MultiMessage multiMessage)
        {
            switch (multiMessage)
            {
                case MultiMessage.CantAttackCitizenWithSafeOn:
                    Console.WriteLine(Constants.CANT_ATTACK_CITIZEN_WITH_SAFE_ON);
                    return true;
                case MultiMessage.CantHelpNpcCitizen:
                    Console.WriteLine(Constants.CANT_HELP_NPC_CITIZEN);
                    return true;
                case MultiMessage.CantHelpNpcFaction:
                    Console.WriteLine(Constants.CANT_HELP_NPC_FACTION);
                    return true;
                case MultiMessage.ChaosCantAttackChaosNpc:
                    Console.WriteLine(Constants.CHAOS_CANT_ATTACK_CHAOS_NPC);
                    return true;
                case MultiMessage.ChaosCantHelpCitizen:
                    Console.WriteLine(Constants.CHAOS_CANT_HELP_CITIZEN);
                    return true;
                case MultiMessage.CitizenAttackedCitizen:
                    Console.WriteLine(Constants.CITIZEN_ATTACKED_CITIZEN);
                    return true;
                case MultiMessage.CitizenAttackedCitizenPet:
                    Console.WriteLine(Constants.CITIZEN_ATTACKED_CITIZEN_PET);
                    return true;
                case MultiMessage.CitizenAttackedImperialNpc:
                    Console.WriteLine(Constants.CITIZEN_ATTACKED_IMPERIAL_NPC);
                    return true;
                case MultiMessage.CitizenAttackedNpcFightingCitizen:
                    Console.WriteLine(Constants.CITIZEN_ATTACKED_NPC_FIGHTING_CITIZEN);
                    return true;
                case MultiMessage.CitizenSafeOnCantAttackCitizenPet:
                    Console.WriteLine(Constants.CITIZEN_SAFE_ON_CANT_ATTACK_CITIZEN_PET);
                    return true;
                case MultiMessage.CitizenSafeOnCantAttackImperialNpc:
                    Console.WriteLine(Constants.CITIZEN_SAFE_ON_CANT_ATTACK_IMPERIAL_NPC);
                    return true;
                case MultiMessage.CitizenSafeOnCantAttackNpcFightingCitizen:
                    Console.WriteLine(Constants.CITIZEN_SAFE_ON_CANT_ATTACK_NPC_FIGHTING_CITIZEN);
                    return true;
                case MultiMessage.HelpCriminalsToggleSafeOff:
                    Console.WriteLine(Constants.HELP_CRIMINALS_TOGGLE_SAFE_OFF);
                    return true;
                case MultiMessage.HelpNpcsToggleSafeOff:
                    Console.WriteLine(Constants.HELP_NPCS_TOGGLE_SAFE_OFF);
                    return true;
                case MultiMessage.ImperialCantAttackCitizenPet:
                    Console.WriteLine(Constants.IMPERIAL_CANT_ATTACK_CITIZEN_PET);
                    return true;
                case MultiMessage.ImperialCantAttackImperialNpc:
                    Console.WriteLine(Constants.IMPERIAL_CANT_ATTACK_IMPERIAL_NPC);
                    return true;
                case MultiMessage.ImperialCantAttackNpcFightingCitizen:
                    Console.WriteLine(Constants.IMPERIAL_CANT_ATTACK_NPC_FIGHTING_CITIZEN);
                    return true;
                case MultiMessage.ImperialCantHelpCriminal:
                    Console.WriteLine(Constants.IMPERIAL_CANT_HELP_CRIMINAL);
                    return true;
                case MultiMessage.ImperialsCantAttackCitizens:
                    Console.WriteLine(Constants.IMPERIALS_CANT_ATTACK_CITIZENS);
                    return true;
            }

            return false;
        }
        
        private static bool WriteProfessionMultiMessage(MultiMessage multiMessage, Packet packet)
        {
            switch (multiMessage)
            {
                case MultiMessage.NoDepositToMine:
                    Console.WriteLine(Constants.NO_DEPOSIT_TO_MINE);
                    return true;
                case MultiMessage.CantMineThat:
                    Console.WriteLine(Constants.CANT_MINE_THAT);
                    return true;
                case MultiMessage.NoForgeToSmelt:
                    Console.WriteLine(Constants.NO_FORGE_TO_SMELT);
                    return true;
                case MultiMessage.CantSmeltThat:
                    Console.WriteLine(Constants.CANT_SMELT_THAT);
                    return true;
                case MultiMessage.NoHammerEquipped:
                    Console.WriteLine(Constants.NO_HAMMER_EQUIPPED);
                    return true;
                case MultiMessage.NoHandsawEquipped:
                    Console.WriteLine(Constants.NO_HANDSAW_EQUIPPED);
                    return true;
                case MultiMessage.NoSewingKitEquipped:
                    Console.WriteLine(Constants.NO_SEWING_KIT_EQUIPPED);
                    return true;
                case MultiMessage.NoTreeToCut:
                    Console.WriteLine(Constants.NO_TREE_TO_CUT);
                    return true;
                case MultiMessage.CantCutThatTree:
                    Console.WriteLine(Constants.CANT_CUT_THAT_TREE);
                    return true;
                case MultiMessage.NoWaterToFish:
                    Console.WriteLine(Constants.NO_WATER_TO_FISH);
                    return true;
                case MultiMessage.NotEnoughMaterials:
                    Console.WriteLine(Constants.NOT_ENOUGH_MATERIALS);
                    return true;
                case MultiMessage.NotEnoughOre:
                    Console.WriteLine(Constants.NOT_ENOUGH_ORE);
                    return true;
                case MultiMessage.StartWorking:
                    Console.WriteLine(Constants.START_WORKING);
                    return true;
                case MultiMessage.StopWorking:
                    Console.WriteLine(Constants.STOP_WORKING);
                    return true;
                case MultiMessage.TooFarFromAnvil:
                    Console.WriteLine(Constants.TOO_FAR_FROM_ANVIL);
                    return true;
                case MultiMessage.TooFarToCutWood:
                    Console.WriteLine(Constants.TOO_FAR_TO_CUT_WOOD);
                    return true;
                case MultiMessage.TooFarToFish:
                    Console.WriteLine(Constants.TOO_FAR_TO_FISH);
                    return true;
                case MultiMessage.TooFarToMine:
                    Console.WriteLine(Constants.TOO_FAR_TO_MINE);
                    return true;
                case MultiMessage.TooFarToSmelt:
                    Console.WriteLine(Constants.TOO_FAR_TO_SMELT);
                    return true;
            }

            return false;
        }

        private static bool WriteLevelingMultiMessage(MultiMessage multiMessage, Packet packet)
        {
            int increase;

            switch (multiMessage)
            {
                case MultiMessage.IncreasedHit:
                    increase = packet.ReadUShort();
                    Console.WriteLine(Constants.IncreasedHit(increase));
                    return true;
                case MultiMessage.IncreasedHp:
                    increase = packet.ReadInt();
                    Console.WriteLine(Constants.IncreasedHp(increase));
                    return true;
                case MultiMessage.IncreasedMana:
                    increase = packet.ReadUShort();
                    Console.WriteLine(Constants.IncreasedMana(increase));
                    return true;
                case MultiMessage.IncreasedSkillPoints:
                    Console.WriteLine(Constants.INCREASED_SKILL_POINTS);
                    return true;
                case MultiMessage.IncreasedStamina:
                    increase = packet.ReadUShort();
                    Console.WriteLine(Constants.IncreasedStamina(increase));
                    return true;
                case MultiMessage.LeveledUp:
                    Console.WriteLine(Constants.LEVELED_UP);
                    return true;
                case MultiMessage.ReachedMaxLevel:
                    Console.WriteLine(Constants.REACHED_MAX_LEVEL);
                    return true;
            }

            return false;
        }
        
        private static bool WritePartyMultiMessage(MultiMessage multiMessage, Packet packet)
        {
            string username;
            
            switch (multiMessage)
            {
                case MultiMessage.PartyIsFull:
                    Console.WriteLine(Constants.PARTY_IS_FULL);
                    return true;
                case MultiMessage.PlayerAlreadyInParty:
                    Console.WriteLine(Constants.PLAYER_ALREADY_IN_PARTY);
                    return true;
                case MultiMessage.PlayerDifferentFaction:
                    Console.WriteLine(Constants.PLAYER_DIFFERENT_FACTION);
                    return true;
                case MultiMessage.PlayerInvitedToParty:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.PlayerInvitedToParty(username));
                    return true;
                case MultiMessage.YouInvitedPlayerToParty:
                    username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.YouInvitedPlayerToParty(username));
                    break;
            }

            return false;
        }

        private static bool WriteQuestingMultiMessage(MultiMessage multiMessage)
        {
            switch (multiMessage)
            {
                case MultiMessage.MustChooseQuestReward:
                    Console.WriteLine(Constants.MUST_CHOOSE_REWARD);
                    return true;
                case MultiMessage.NotAllStepsAreCompleted:
                    Console.WriteLine(Constants.NOT_ALL_STEPS_COMPLETED);
                    return true;
                case MultiMessage.QuestLogFull:
                    Console.WriteLine(Constants.QUEST_LOG_FULL);
                    return true;
                case MultiMessage.QuestRequirementsNotMet:
                    Console.WriteLine(Constants.QUEST_REQUIREMENTS_NOT_MET);
                    return true;
            }

            return false;
        }

        private static bool WriteMailingMultiMessage(MultiMessage multiMessage, Packet packet)
        {
            switch (multiMessage)
            {
                case MultiMessage.CantSendMailRightNow:
                    Console.WriteLine(Constants.CANT_SEND_MAIL_RIGHT_NOW, ConsoleMessage.Warning);
                    return true;
                case MultiMessage.CharacterDoesntExist:
                    Console.WriteLine(Constants.CHARACTER_DOESNT_EXIST, ConsoleMessage.Warning);
                    return true;
                case MultiMessage.RecipientInboxFull:
                    Console.WriteLine(Constants.RECIPIENT_INBOX_IS_FULL, ConsoleMessage.Warning);
                    return true;
                case MultiMessage.MailSentSuccessfully:
                    Console.WriteLine(Constants.MAIL_SENT_SUCCESSFULLY);
                    UIManager.GameUI.MailWindow.SendMailWindow.Reset();
                    return true;
                case MultiMessage.NewMailReceived:
                    string username = GameManager.Instance.GetPlayer(packet.ReadClientId()).Username;
                    Console.WriteLine(Constants.NewMailReceived(username));
                    return true;
            }

            return false;
        }
        
        private static void WriteMiscMultiMessage(MultiMessage multiMessage, Packet packet)
        {
            string messageAux;
            
            switch (multiMessage)
            {
                case MultiMessage.CantMeditate:
                    Console.WriteLine(Constants.CANT_MEDITATE);
                    break;
                case MultiMessage.CantMeditateDead:
                    Console.WriteLine(Constants.CANT_MEDITATE_DEAD);
                    break;
                case MultiMessage.ClickedOnPlayer:
                    WriteClickedOnPlayer(packet);
                    break;
                case MultiMessage.ClickedOnWorldItem:
                    var item = GameManager.Instance.GetItem(packet.ReadItemId());
                    Console.WriteLine(Constants.ClickedOnWorldItem(item, packet.ReadUShort()));
                    break;
                case MultiMessage.ExitingCancelled:
                    Console.WriteLine(Constants.EXITING_CANCELLED);
                    break;
                case MultiMessage.ExitingInTenSeconds:
                    Console.WriteLine(Constants.EXITING_IN_10_SECONDS);
                    break;
                case MultiMessage.FinishedMeditating:
                    Console.WriteLine(Constants.FINISHED_MEDITATING);
                    break;
                case MultiMessage.ManaRecovered:
                    Console.WriteLine(Constants.ManaRecovered(packet.ReadShort()));
                    break;
                case MultiMessage.NotEnoughStamina:
                    messageAux = GameManager.Instance.LocalPlayer.Gender == Gender.Female ? Constants.NO_STAM_FEMALE : Constants.NO_STAM_MALE;
                    Console.WriteLine(messageAux);
                    break;
                case MultiMessage.ObstacleClick:
                    Console.WriteLine(Constants.TagNames[packet.ReadByte()]);
                    break;
                case MultiMessage.SkillLevelUp:
                    var skill = (Skill)packet.ReadByte();
                    byte value = packet.ReadByte();
                    Console.WriteLine(Constants.SkilledLeveledUp(skill, value));
                    UIManager.GameUI.StatsWindow.SetSkill(skill, value);
                    UIManager.GameUI.CraftingWindow.UpdateSkill(skill, value);
                    break;
                case MultiMessage.TalentPointsObtained:
                    byte points = packet.ReadByte();
                    Console.WriteLine(Constants.TalentPointsObtained(points));
                    break;
                case MultiMessage.StoppedMeditating:
                    Console.WriteLine(Constants.STOPPED_MEDITATING);
                    break;
                case MultiMessage.TooTiredToFight:
                    messageAux = GameManager.Instance.LocalPlayer.Gender == Gender.Female ? Constants.FEMALE_TOO_TIRED_TO_FIGHT : Constants.MALE_TOO_TIRED_TO_FIGHT;
                    Console.WriteLine(messageAux);
                    break;
                case MultiMessage.YouAreDead:
                    Console.WriteLine(Constants.YOU_ARE_DEAD);
                    break;
                case MultiMessage.DescriptionChanged:
                    Console.WriteLine(Constants.DESCRIPTION_CHANGED);
                    break;
                case MultiMessage.DescriptionTooLong:
                    Console.WriteLine(Constants.DESCRIPTION_TOO_LONG);
                    break;
                case MultiMessage.DescriptionInvalid:
                    Console.WriteLine(Constants.DESCRIPTION_INVALID);
                    break;
                
            }
        }
        
        private static void WriteNpcHitPlayer(BodyPart bodyPart, int damage)
        {
            switch (bodyPart)
            {
                case BodyPart.Head:
                    Console.WriteLine($"{Constants.NPC_HIT_HEAD} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.LeftLeg:
                    Console.WriteLine($"{Constants.NPC_HIT_LEFT_LEG} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.RightLeg:
                    Console.WriteLine($"{Constants.NPC_HIT_RIGHT_LEG} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.LeftArm:
                    Console.WriteLine($"{Constants.NPC_HIT_LEFT_ARM} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.RightArm:
                    Console.WriteLine($"{Constants.NPC_HIT_RIGHT_ARM} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.Chest:
                    Console.WriteLine($"{Constants.NPC_HIT_BODY} {damage}!!", ConsoleMessage.Combat);
                    break;
            }
        }

        private static void WritePlayerHitByPlayer(string username, BodyPart bodyPart, int damage)
        {
            switch (bodyPart)
            {
                case BodyPart.Head:
                    Console.WriteLine($"¡¡{username} {Constants.ENEMY_HIT_HEAD} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.LeftLeg:
                    Console.WriteLine($"¡¡{username} {Constants.ENEMY_HIT_LEFT_LEG} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.RightLeg:
                    Console.WriteLine($"¡¡{username} {Constants.ENEMY_HIT_RIGHT_LEG} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.LeftArm:
                    Console.WriteLine($"¡¡{username} {Constants.ENEMY_HIT_LEFT_ARM} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.RightArm:
                    Console.WriteLine($"¡¡{username} {Constants.ENEMY_HIT_RIGHT_ARM} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.Chest:
                    Console.WriteLine($"¡¡{username} {Constants.ENEMY_HIT_BODY} {damage}!!", ConsoleMessage.Combat);
                    break;
            }
        }

        private static void WritePlayerHitPlayer(string username, BodyPart bodyPart, int damage)
        {
            switch (bodyPart)
            {
                case BodyPart.Head:
                    Console.WriteLine($"{Constants.HIT_PLAYER} {username} {Constants.HIT_PLAYER_HEAD} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.LeftLeg:
                    Console.WriteLine($"{Constants.HIT_PLAYER} {username} {Constants.HIT_PLAYER_LEFT_LEG} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.RightLeg:
                    Console.WriteLine($"{Constants.HIT_PLAYER} {username} {Constants.HIT_PLAYER_RIGHT_LEG} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.LeftArm:
                    Console.WriteLine($"{Constants.HIT_PLAYER} {username} {Constants.HIT_PLAYER_LEFT_ARM} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.RightArm:
                    Console.WriteLine($"{Constants.HIT_PLAYER} {username} {Constants.HIT_PLAYER_RIGHT_ARM} {damage}!!", ConsoleMessage.Combat);
                    break;
                case BodyPart.Chest:
                    Console.WriteLine($"{Constants.HIT_PLAYER} {username} {Constants.HIT_PLAYER_BODY} {damage}!!", ConsoleMessage.Combat);
                    break;
            }
        }

        private static string ReadDisplayNpcInfo(Packet packet)
        {
            string npcName = GameManager.Instance.GetNpcInfo(packet.ReadNpcId()).Name;
            int currentHealth = packet.ReadInt();
            int maxHealth = packet.ReadInt();
            bool isPet = packet.ReadBool();
            var ownerOrFightingPlayerId = packet.ReadClientId();
            
            if (isPet)
            {
                string username = GameManager.Instance.GetPlayer(ownerOrFightingPlayerId).Username;
                return $"{npcName} - ({currentHealth}/{maxHealth}) - {Constants.PetOf(username)}";
            }
            
            if (ownerOrFightingPlayerId != ClientId.Empty)
            {
                string username = GameManager.Instance.GetPlayer(ownerOrFightingPlayerId).Username;
                return $"{npcName} - ({currentHealth}/{maxHealth}) - {Constants.FightingWith(username)}";
            }

            return $"{npcName} - ({currentHealth}/{maxHealth})";
        }

        private static void WriteClickedOnPlayer(Packet packet)
        {
            var player = GameManager.Instance.GetPlayer(packet.ReadClientId());
            bool isNewbie = packet.ReadBool();
            var msgType = (ConsoleMessage)packet.ReadByte();
            if (string.IsNullOrEmpty(player.Description))
                Console.WriteLine(Constants.ClickedOnPlayer(isNewbie, player.Username), msgType);
            else
                Console.WriteLine(Constants.ClickedOnPlayer(isNewbie, player.Username, player.Description), msgType);
        }
    }
}
