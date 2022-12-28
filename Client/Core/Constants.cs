using System;
using System.Collections.Generic;
using AO.Core.Ids;
using AOClient.Player;
using AOClient.Player.Utils;
using UnityEngine;

namespace AOClient.Core
{ 
    public static class Constants
    {
        #region Combat
        public static string AttackerAppliedBleed(string username) => $"¡¡Has desangrado a {username}!!";
        public static string AttackerEnvenomed(string username) => $"¡¡Has envenenado a {username}!!";
        public const string ATTACK_PARRIED_SHIELD = "¡¡¡Has rechazado el ataque con el escudo!!!";
        public const string OTHER_PLAYER_PARRIED_ATTACK_SHIELD = "¡¡¡El usuario rechazó el ataque con su escudo!!!";
        public const string CANT_ATTACK_IN_SAFE_ZONE = "Solo puedes atacar usuarios en zonas inseguras o arenas.";
        public const string CANT_ATTACK_OWN_PET = "No puedes atacar a tu propia mascota.";
        public const string CANT_ATTACK_SPIRIT = "No puedes atacar a un espíritu.";
        public const string CANT_ATTACK_THAT_NPC = "No puedes atacar a esta criatura.";
        public const string CANT_ATTACK_YOURSELF = "No puedes atacarte a vos mismo.";
        public const string KILLED_NPC = "¡Has matado a la criatura!";
        public static string KilledPlayer(string username) => $"¡Has matado a {username}!";
        public const string NO_AMMO = "No tienes municiones.";
        public const string NPC_HIT_HEAD = "¡¡La criatura te ha pegado en la cabeza por";
        public const string NPC_HIT_LEFT_ARM = "¡¡La criatura te ha pegado en el brazo izquierdo por";
        public const string NPC_HIT_RIGHT_ARM = "¡¡La criatura te ha pegado en el brazo derecho por";
        public const string NPC_HIT_LEFT_LEG = "¡¡La criatura te ha pegado en la pierna izquierda por";
        public const string NPC_HIT_RIGHT_LEG = "¡¡La criatura te ha pegado en la pierna derecha por";
        public const string NPC_HIT_BODY = "¡¡La criatura te ha pegado en el torso por";
        public const string NPC_KILLED_PLAYER = "¡¡¡La criatura te ha matado!!!";
        public static string NpcCastSpellOnPlayer(string npcName, string spellName) => $"{npcName} ha lanzado {spellName} sobre ti.";
        public static string NpcDamageSpellPlayer(string npcName, int damage) => $"{npcName} te ha quitado {damage} puntos de vida.";
        public const string NPC_ENVENOMED_PLAYER = "¡¡La criatura te ha envenenado!!";
        public const string NPC_FAILED_HIT = "¡¡¡La criatura falló el golpe!!!";
        public static string EnemyAttackMissed(string username) => $"¡¡{username} te atacó y falló!!";
        public static string PlayerDamageSpellNpc(int damage) => $"¡Le has quitado {damage} puntos de vida a la criatura!";
        public static string PlayerDamageSpellEnemy(int damage, string username) => $"Le has quitado {damage} puntos de vida a {username}.";
        public static string EnemyDamageSpellPlayer(int damage, string username) => $"{username} te ha quitado {damage} puntos de vida.";
        public static string PlayerGotStabbed(string username, int damage) => $"¡¡{username} te ha apuñalado por {damage}!!";
        public const string ENEMY_HIT_HEAD = "te ha pegado en la cabeza por";
        public const string ENEMY_HIT_LEFT_ARM = "te ha pegado en el brazo izquierdo por";
        public const string ENEMY_HIT_RIGHT_ARM = "te ha pegado en el brazo derecho por";
        public const string ENEMY_HIT_LEFT_LEG = "te ha pegado en la pierna izquierda por";
        public const string ENEMY_HIT_RIGHT_LEG = "te ha pegado en la pierna derecha por";
        public const string ENEMY_HIT_BODY = "te ha pegado en el torso por";
        public static string PlayerHitNpc(int damage) => $"¡¡Le has pegado a la criatura por {damage}!!";
        public const string HIT_PLAYER = "¡¡Le has pegado a";
        public const string HIT_PLAYER_HEAD = "en la cabeza por";
        public const string HIT_PLAYER_LEFT_ARM = "en el brazo izquierdo por";
        public const string HIT_PLAYER_RIGHT_ARM = "en el brazo derecho por";
        public const string HIT_PLAYER_LEFT_LEG = "en la pierna izquierda por";
        public const string HIT_PLAYER_RIGHT_LEG = "en la pierna derecha por";
        public const string HIT_PLAYER_BODY = "en el torso por";
        public static string HasKilledYou(string username) => $"¡{username} te ha matado!";
        public const string ATTACK_MISSED = "¡¡¡Has fallado el golpe!!!";
        public static string StabbedNpc(int damage) => $"¡¡Has apuñalado a la criatura por {damage}!!";
        public static string StabbedPlayer(string username, int damage) => $"¡¡Has apuñalado a {username} por {damage}!!";
        public static string TargetGotBled(string username) => $"¡¡{username} te ha desangrado!!";
        public static string TargetGotEnvenomed(string username) => $"¡¡{username} te ha envenenado!!";
        public const string TOO_FAR_TO_ATTACK = "Estás demasiado lejos para atacar.";
        public const string YOU_ARE_BLEEDING = "Te estás desangrando, si no te curas morirás.";
        public const string FEMALE_YOU_ARE_ENVENOMED = "Estás envenenada, si no te curas morirás.";
        public const string MALE_YOU_ARE_ENVENOMED = "Estás envenenado, si no te curas morirás.";
        #endregion

        #region ClickRequests
        public const string CLICK_REQUEST_CAST_SPELL = "Haz click sobre el objetivo...";
        public const string CLICK_REQUEST_PROJECTILE_ATTACK = "Haz click sobre la víctima...";
        public const string CLICK_REQUEST_TAME_ANIMAL = "Haz click sobre la criatura que quieres domar...";
        public const string CLICK_REQUEST_PET_CHANGE_TARGET = "Haz click sobre el objectivo...";
        public const string CLICK_REQUEST_STEAL = "Haz click sobre la víctima...";
        public const string CLICK_REQUEST_INVITE_PARTY = "Haz click sobre la persona que quieres invitar a tú grupo...";
        public const string CLICK_REQUEST_MINE = "Haz click sobre el yacimiento...";
        public const string CLICK_REQUEST_CUT_WOOD = "Haz click sobre el árbol...";
        public const string CLICK_REQUEST_FISH = "Haz click sobre el sitio donde quieres pescar...";
        public const string CLICK_REQUEST_SMELT = "Haz click sobre la fragua...";
        public const string CLICK_REQUEST_CRAFT_BLACKSMITHING = "Haz click sobre un yunque...";
        #endregion

        #region Npc
        public const string PET_IGNORE_COMMAND = "Tu mascota ignora tu comando.";
        public const string TOO_FAR_TO_TAME = "Estás demasiado lejos de la criatura.";
        public const string CANT_TAME_NPC = "No puedes domar a esa criatura.";
        public const string CANT_TAME_NPC_IN_COMBAT = "No puedes domar una criatura que está luchando con un jugador.";
        public const string NO_NPC_TO_TAME = "¡No hay ninguna criatura allí!";
        public const string ALREADY_TAMED_THAT_NPC = "Esa criatura ya es tu mascota.";
        public const string NPC_ALREADY_HAS_OWNER = "La criatura ya tiene amo.";
        public const string ALREADY_HAVE_A_PET = "No puedes domar más de una criatura al mismo tipo.";
        public const string FAILED_TO_TAME_NPC = "No has logrado domar a la criatura";
        public const string SUCCESSFULLY_TAMED_NPC = "La criatura te ha aceptado como su amo.";
        public const string CANT_SUMMON_PET_IN_SAFE_ZONE = "No se permiten mascotas en zonas seguras. Tendrás que invocarlas afuera.";
        #endregion

        #region Items
        public const string BLACK_POTION_ONE = "No te sientes muy bien...";
        public const string BLACK_POTION_TWO = "Sientes un gran mareo y pierdes el conocimiento.";
        public const string CANT_USE_AXE_AND_SHIELD = "No puedes usar un hacha y un escudo al mismo tiempo.";
        public const string CANT_USE_CLASS = "Tu clase no puede usar este objeto.";
        public const string CANT_USE_FACTION = "Tu facción no puede usar este objeto.";
        public const string CANT_USE_GENDER = "Tu sexo no puede usar este objeto.";
        public const string CANT_USE_RACE = "Tu raza no puede usar este objeto.";
        public const string CANT_USE_MEDITATING = "¡Estás meditando! Debes dejar de meditar para usar objetos.";
        public const string CANT_USE_WEAPON_LIKE_THAT = "No puedes usar así esta arma.";
        public const string MUST_EQUIP_ITEM_FIRST = "Debés equiparte el objecto antes de poder usarlo.";
        public const string NO_SPACE_TO_DROP_ITEM = "No hay espacio en el piso.";
        public const string INVENTORY_FULL = "No puedes cargar mas objetos.";
        public const string ITEM_ONLY_NEWBIES = "Solo los newbies pueden usar este objeto.";
        public const string NOT_ENOUGH_MONEY = "No tienes suficiente dinero.";
        public const string CANT_USE_TALENT = "No tienes el talento necesario para usar este objecto.";
        public static string NotEnoughSkillToUse(Skill skill) => $"No tienes suficientes puntos en {SkillsNames[skill]} para usar este objeto.";
        public static string PlayerDroppedItemTo(string to, string itemName, int quantity) => $"Le has arrojado {quantity} {itemName} a {to}";
        public static string PlayerGotItemDropped(string from, string itemName, int quantity) => $"{from} te ha arrojado {quantity} {itemName}";
        #endregion

        #region Spells
        public const string CANT_CAST_DEAD = "No puedes lanzar hechizos estando muerto.";
        public const string CANT_CAST_ON_SPIRIT = "No puedes lanzar este hechizo a un muerto.";
        public const string CANT_LEARN_MORE_SPELLS = "No puedes aprender más hechizos.";
        public const string INVALID_TARGET = "Target inválido.";
        public const string MAGIC_ITEM_NOT_EQUIPPED = "Debés tener equipado un objeto mágico para lanzar este hechizo.";
        public const string MAGIC_ITEM_NOT_POWERFUL_ENOUGH = "No posees un objecto mágico lo suficientemente poderoso para poder lanzar este hechizo.";
        public const string NOT_ENOUGH_MANA = "No tienes suficiente mana.";
        public const string NOT_ENOUGH_SKILL_TO_CAST = "No tienes suficientes puntos en magia para lanzar este hechizo.";
        public const string SPELL_NOT_ENOUGH_STAM_FEMALE = "Estás muy cansada para lanzar este hechizo.";
        public const string SPELL_NOT_ENOUGH_STAM_MALE = "Estás muy cansado para lanzar este hechizo.";
        public const string NPC_IMMUNE_TO_SPELL = "El npc es inmune a este hechizo.";
        public const string NPCS_ONLY_SPELL = "Este hechizo solo afecta a los npcs.";
        public static string NpcHealedPlayer(string npcName, int amount) => $"{npcName} te ha restaurado {amount} puntos de vida.";
        public static string PlayerHealedNpc(int amount) => $"Le has restaurado {amount} puntos de vida a la criatura.";
        public static string PlayerHealed(int amount, string username) => $"Le has restaurado {amount} puntos de vida a {username}.";
        public static string PlayerGotHealed(int amount, string username) => $"{username} te ha restaurado {amount} puntos de vida.";
        public static string PlayerSelfHeal(int amount) => $"Te has restaurado {amount} puntos de vida.";
        public const string SPELL_ALREADY_LEARNED = "Ya has aprendido ese hechizo.";
        public const string STAFF_NOT_EQUIPPED = "Debés tener equipado un báculo para lanzar este hechizo.";
        public const string STAFF_NOT_POWERFUL_ENOUGH = "No posees un báculo lo suficientemente poderoso para poder lanzar este hechizo.";
        public const string TARGET_RESS_TOGGLED_OFF = "El espíritu no quiere ser revivido.";
        public const string TOO_FAR_TO_CAST = "Estás demasiado lejos para lanzar este hechizo.";
        public const string USERS_ONLY_SPELL = "Este hechizo solo actúa sobre usuarios.";
        #endregion

        #region Faction
        public const string CANT_ATTACK_CITIZEN_WITH_SAFE_ON = "No puedes atacar ciudadanos, para hacerlo debés desactivar el seguro.";
        public const string CANT_HELP_NPC_CITIZEN = "No puedes ayudar criaturas que están luchando contra ciudadanos.";
        public const string CANT_HELP_NPC_FACTION = "No puedes ayudar criaturas que están luchando contra un miembro de tu facción.";
        public const string CHAOS_CANT_ATTACK_CHAOS_NPC = "No puedes atacar Guardias del Caos siendo Caos.";
        public const string CHAOS_CANT_HELP_CITIZEN = "Los miembros del Ejército del Caos no pueden ayudar a los ciudadanos.";
        public const string CITIZEN_ATTACKED_CITIZEN = "Has atacado a un ciudadano. Te has convertido en criminal.";
        public const string CITIZEN_ATTACKED_CITIZEN_PET = "Has atacado la mascota de un ciudadano. Te has convertido en criminal.";
        public const string CITIZEN_ATTACKED_IMPERIAL_NPC = "¡Atacaste un Guardia Imperial! Te has convertido en criminal.";
        public const string CITIZEN_ATTACKED_NPC_FIGHTING_CITIZEN = "Has atacado una criatura perteneciente a un ciudadano. Te has convertido en criminal.";
        public const string CITIZEN_SAFE_ON_CANT_ATTACK_CITIZEN_PET = "Para atacar mascotas de ciudadanos debes quitarte el seguro.";
        public const string CITIZEN_SAFE_ON_CANT_ATTACK_IMPERIAL_NPC = "Para poder atacar Guardias Imperiales debés quitarte el seguro.";
        public const string CITIZEN_SAFE_ON_CANT_ATTACK_NPC_FIGHTING_CITIZEN = "Para atacar criaturas pertenecientes a ciudadanos debes quitarte el seguro.";
        public const string HELP_CRIMINALS_TOGGLE_SAFE_OFF = "Para poder ayudar criminales debes quitarte el seguro.";
        public const string HELP_NPCS_TOGGLE_SAFE_OFF = "Para ayudar a criaturas que están luchando contra ciudadanos debes quitarte el seguro.";
        public const string IMPERIAL_CANT_ATTACK_CITIZEN_PET = "Los miembros del Ejército Imperial no pueden atacar mascostas de ciudadanos.";
        public const string IMPERIAL_CANT_ATTACK_IMPERIAL_NPC = "No puedes atacar Guardias Imperiales siendo Imperial.";
        public const string IMPERIAL_CANT_ATTACK_NPC_FIGHTING_CITIZEN = "Los miembros del Ejército Imperial no pueden atacar criaturas pertenecientes a otros ciudadanos.";
        public const string IMPERIAL_CANT_HELP_CRIMINAL = "Los miembros del Ejército Imperial no pueden ayudar a los criminales.";
        public const string IMPERIALS_CANT_ATTACK_CITIZENS = "Los soldados del Ejército Imperial tienen prohibido atacar ciudadanos.";
        
        public static readonly Dictionary<Faction, string> FactionNames = new()
        {
            { Faction.Citizen,  "Ciudadano" },
            { Faction.Criminal, "Criminal" },
            { Faction.Imperial, "Imperial"},
            { Faction.Chaos,    "Caos"}
        };

        public static readonly Dictionary<FactionRank, string> ImperialFactionNames = new()
        {
            { FactionRank.One,   "Imperial Rank One"   },
            { FactionRank.Two,   "Imperial Rank Two"   },
            { FactionRank.Three, "Imperial Rank Three" },
            { FactionRank.Four,  "Imperial Rank Four"  },
            { FactionRank.Five,  "Imperial Rank Five"  },
            { FactionRank.Six,   "Imperial Rank Six"   },
            { FactionRank.Seven, "Imperial Rank Seven" },
            { FactionRank.Eight, "Imperial Rank Eight" },
            { FactionRank.Nine,  "Imperial Rank Nine"  },
            { FactionRank.Ten,   "Imperial Rank Ten"   }
        };
        
        public static readonly Dictionary<FactionRank, string> ChaosFactionNames = new()
        {
            { FactionRank.One,   "Chaos Rank One"   },
            { FactionRank.Two,   "Chaos Rank Two"   },
            { FactionRank.Three, "Chaos Rank Three" },
            { FactionRank.Four,  "Chaos Rank Four"  },
            { FactionRank.Five,  "Chaos Rank Five"  },
            { FactionRank.Six,   "Chaos Rank Six"   },
            { FactionRank.Seven, "Chaos Rank Seven" },
            { FactionRank.Eight, "Chaos Rank Eight" },
            { FactionRank.Nine,  "Chaos Rank Nine"  },
            { FactionRank.Ten,   "Chaos Rank Ten"   }
        };

        #endregion

        #region Professions
        public const string NO_DEPOSIT_TO_MINE = "Debes hacer click sobre un yacimiento para poder minar.";
        public const string CANT_MINE_THAT = "No tienes la habilidad para minar ese yacimiento.";
        public const string NO_FORGE_TO_SMELT = "Debes hacer click sobre una fragua para poder fundir minerales.";
        public const string CANT_SMELT_THAT = "No tienes la habilidad para fundir ese mineral.";
        public const string NO_HAMMER_EQUIPPED = "Debes tener un martillo de herrero equipado.";
        public const string NO_HANDSAW_EQUIPPED = "Debes tener un serrucho equipado.";
        public const string NO_SEWING_KIT_EQUIPPED = "Debes tener un kit de sastrería equipado.";
        public const string NO_TREE_TO_CUT = "Debes hacer click sobre un árbol para poder talar.";
        public const string CANT_CUT_THAT_TREE = "No tienes la habilidad para talar ese árbol.";
        public const string NO_WATER_TO_FISH = "Debes hacer click sobre un lugar con agua para poder pescar.";
        public const string NOT_ENOUGH_MATERIALS = "No tienes suficientes materiales.";
        public const string NOT_ENOUGH_ORE = "No tienes suficientes minerales para hacer un lingote.";
        public const string START_WORKING = "Comienzas a trabajar.";
        public const string STOP_WORKING = "Dejas de trabajar.";
        public const string TOO_FAR_FROM_ANVIL = "Estás demasiado lejos del yunque.";
        public const string TOO_FAR_TO_FISH = "Estás demasiado lejos para poder pescar.";
        public const string TOO_FAR_TO_CUT_WOOD = "Estás demasiado lejos del árbol para poder talar.";
        public const string TOO_FAR_TO_MINE = "Estás demasiado lejos del yacimiento para poder minar.";
        public const string TOO_FAR_TO_SMELT = "Estás demasiado lejos de la fragua.";
        #endregion
        
        #region Leveling
        public static string GainedExperience(uint xp) => $"¡Has ganado {xp} puntos de experiencia!";
        public static string IncreasedHit(int hit) => $"Tu golpe aumentó {hit} puntos.";
        public static string IncreasedHp(int hp) => $"Has ganado {hp} puntos de vida.";
        public static string IncreasedMana(int mana) => $"Has ganado {mana} puntos de mana.";
        public const string INCREASED_SKILL_POINTS = "Has ganado 5 skillpoints.";
        public static string IncreasedStamina(int stam) => $"Has ganado {stam} puntos de energía.";
        public const string LEVELED_UP = "¡Has subido de nivel!";
        public const string REACHED_MAX_LEVEL = "¡Has alcanzado el nivel máximo!";
        #endregion
        
        #region Party
        public const string PARTY_IS_FULL = "El grupo esta lleno.";
        public const string PLAYER_ALREADY_IN_PARTY = "La persona que deseas invitar ya se encuentra en un grupo.";
        public const string PLAYER_DIFFERENT_FACTION = "La persona que deseas invitar es de otra facción.";
        public static string PlayerInvitedToParty(string username) =>  $"{username} te ha invitado a unirte a su grupo. Escribe /aceptar si deseas unirte al grupo.";
        public static string YouInvitedPlayerToParty(string username) =>  $"Has invitado a {username} a unirse al grupo.";
        public const string YOU_HAVE_BEEN_KICKED = "Has sido expulsado del grupo.";
        public static string PlayerJoinedParty(string username) => $"{username} se ha unido al grupo.";
        public const string YOU_JOINED_PARTY = "Te uniste al grupo.";
        public static string PlayerIsNowPlayerLeader(string username) => $"{username} es el nuevo líder del grupo.";
        public const string YOU_ARE_NOW_PARTY_LEADER = "Eres el nuevo líder del grupo.";
        public static string PlayerLeftParty(string username) => $"{username} ha abandonado el grupo.";
        public const string YOU_LEFT_PARTY = "Has abandonado el grupo.";
        #endregion
        
        #region Questing
        public const string MUST_CHOOSE_REWARD = "Debe seleccionar una recompenza antes de completar la quest.";
        public const string NOT_ALL_STEPS_COMPLETED = "No has completado todos los objetivos aún.";
        public static string QuestAccepted(string questName) => $"Quest '{questName}' aceptada.";
        public const string QUEST_COMPLETED = "Quest completada!!";
        public const string QUEST_LOG_FULL = "No puedes aceptar más misiones.";
        public const string QUEST_REQUIREMENTS_NOT_MET = "No cumplés con los requisitos necesarios para aceptar esta quest.";
        #endregion

        #region Mailing
        public static string ReceivedMailEntry(string from, string subject, TimeSpan expiresIn) => $"De: {from} - {subject}\n{FormatExpiresIn(expiresIn)}.";
        public const string RECIPIENT_FIELD_EMPTY = "Debes ingresar el nombre del destinatario.";
        public const string SUBJECT_FIELD_EMPTY = "Debes ingresar un asunto para el correo.";
        public const string NOT_ENOUGH_GOLD = "No tienes esa cantidad de oro.";
        public const byte MAX_MAIL_ITEMS = 10;
        public const string CANT_SEND_MAIL_RIGHT_NOW = "No se pudo enviar el mail.";
        public const string CHARACTER_DOESNT_EXIST = "No existe un personaje con ese nombre.";
        public const string RECIPIENT_INBOX_IS_FULL = "La casilla del destinario está llena.";
        public const string MAIL_SENT_SUCCESSFULLY = "El correo ha sido enviado con exito.";
        public static string NewMailReceived(string from) => $"Has recibido un nuevo correo de: {from}.";

        private static string FormatExpiresIn(TimeSpan expiresIn)
        {
            if (expiresIn.Days > 0)
            {
                string day = expiresIn.Days == 1 ? "día" : "días";
                return $"Expira en {expiresIn.Days} {day}";
            }

            if (expiresIn.Hours > 0)
            {
                string hour = expiresIn.Hours == 1 ? "hora" : "horas";
                return $"Expira en {expiresIn.Hours} {hour}";
            }
            
            // Always show 1 minute remaining even if only seconds remain
            int minutes = expiresIn.Minutes > 0 ? expiresIn.Minutes : 1; 
            string strMinutes = minutes == 1 ? "minuto" : "minutos";
            return $"Expira en {minutes} {strMinutes}";
        }
        #endregion
        
        #region Misc
        public const string YOU_HAVE_BEEN_DISCONNECTED = "Has sido desconectado del servidor.";
        public const string COULD_NOT_CONNECT_TO_SERVER = "No se ha podido conectar con el servidor.";
        
        public const string CANT_MEDITATE = "No puedes meditar.";
        public const string CANT_MEDITATE_DEAD = "¡¡Estás muerto!! Sólo puedes meditar cuando estás vivo.";
        public static string ClickedOnWorldItem(Item item, ushort quantity) => $"{item.Name} - {item.Description} ({quantity})";
        public const string EXITING_CANCELLED = "Salir ha sido cancelado.";
        public const string EXITING_IN_10_SECONDS = "Saliendo en 10 segundos.";
        public const string FINISHED_MEDITATING = "Has terminado de meditar.";
        public static string ManaRecovered(int mana) => $"¡Has recuperado {mana} puntos de mana!";
        public const string NO_STAM_MALE = "Estás muy cansado.";
        public const string NO_STAM_FEMALE = "Estás muy cansada.";
        public const string RESS_TOGGLE_OFF = ">>SEGURO DE RESURRECCIÓN DESACTIVADO<<";
        public const string RESS_TOGGLE_ON = ">>SEGURO DE RESURRECCIÓN ACTIVADO<<";
        public const string SAFE_TOGGLE_OFF = ">>SEGURO DESACTIVADO<<";
        public const string SAFE_TOGGLE_ON = ">>SEGURO ACTIVADO<<";
        public static string SkilledLeveledUp(Skill skill, byte value) => $"¡Has mejorado tu skill {SkillsNames[skill]}! Ahora tienes {value} puntos.";
        public static string TalentPointsObtained(byte points) => $"Has ganado {points} puntos de talento.";
        public const string STOPPED_MEDITATING = "Dejas de meditar.";
        public const string FEMALE_TOO_TIRED_TO_FIGHT = "Estás muy cansada para luchar.";
        public const string MALE_TOO_TIRED_TO_FIGHT = "Estás muy cansado para luchar.";
        public const string YOU_ARE_DEAD = "¡¡Estás muerto!!";
        public static string FightingWith(string username) => $"Peleando con {username}";
        public static string PetOf(string username) => $"Mascota de {username}";
        public static string ClickedOnPlayer(bool isNewbie, string username) => isNewbie ?  $"Ves a > {username} (Newbie)" : $"Ves a > {username}";
        public static string ClickedOnPlayer(bool isNewbie, string username, string description) => isNewbie ?  $"Ves a > {username} (Newbie) - {description}" : $"Ves a > {username} - {description}";
        public const string DESCRIPTION_CHANGED = "Tú descripción ha sido cambiada.";
        public const string DESCRIPTION_TOO_LONG = "La descripción es muy larga.";
        public const string DESCRIPTION_INVALID = "La descripción contiene carácteres invalidos.";
        #endregion

        #region Extras
        public static readonly ItemId[] Hats = {  };
        public const ushort BOAT_ANIMATION = 16;
        public const ushort DEAD_BOAT_ANIMATION = 17;
        public const ushort GHOST_ANIMATION = 14;
        public const byte GHOST_HEAD = 3;

        public const byte MAX_QUESTS = 20;
        public const byte PLAYER_INV_SPACE = 30;
        public const byte PLAYER_SPELL_SPACE = 36;
        public const byte NPC_INV_SPACE = 30;
        #endregion

        #region HeadPositions
        public static readonly Vector3 TallHeadPosition = new(0f, 0.82f, -1f);
        public static readonly Vector3 ShortHeadPosition = new(0f, 0.52f, -1f);
        public static readonly Vector3 HatRendererPosition = new(0f, 1.25f, -1f);
        public static readonly Vector3 HelmRendererPosition = new(0f, 0.9f, -1f);
        #endregion

        #region Colors
        public static readonly Color32 GameMasterColor = new(0, 240, 0, 255);
        public static readonly Color32 CitizenColor = new(39, 87, 255, 255);
        public static readonly Color32 CriminalColor = new(244, 27, 27, 255);
        public static readonly Color32 ImperialColor = new(56, 182, 255, 255);
        public static readonly Color32 ChaosColor = new(133, 18, 18, 255);
        #endregion

        #region Skills/Talents
        public static readonly Dictionary<Skill, string> SkillsNames = new()
        {
            { Skill.Magic, 			"Magía" },
            { Skill.ArmedCombat, 	"Combate con armas" },
            { Skill.RangedWeapons, 	"Armas de proyectiles" },
            { Skill.UnarmedCombat, 	"Combate sin armas" },
            { Skill.Stabbing,	 	"Apuñalar" },
            { Skill.CombatTactics, 	"Tácticas de combate" },
            { Skill.MagicResistance,"Resistencia mágica" },
            { Skill.ShieldDefense, 	"Defensa con escudos" },
            { Skill.Meditation, 	"Meditación" },
            { Skill.Survival, 		"Supervivencia" },
            { Skill.AnimalTaming, 	"Domar animales" },
            { Skill.Hiding, 		"Ocultarse" },
            { Skill.Trading, 		"Comercio" },
            { Skill.Thieving, 		"Robar" },
            { Skill.Leadership, 	"Liderazgo" },
            { Skill.Sailing, 		"Navegación" },
            { Skill.HorseRiding, 	"Equitación" },
            { Skill.Mining, 		"Minería" },
            { Skill.Blacksmithing, 	"Herrería" },
            { Skill.Woodcutting, 	"Talar árboles" },
            { Skill.Woodworking, 	"Carpintería" },
            { Skill.Fishing, 		"Pesca" },
            { Skill.Tailoring, 		"Sastrería" }
        };

        public const string MINING_TREE_NAME = "Talentos Minería";
        public const string WOODCUTTING_TREE_NAME = "Talentos Talar Árboles";
        public const string FISHING_TREE_NAME = "Talentos Pesca";
        public const string BLACKSMITHING_TREE_NAME = "Talentos Herrería";
        public const string WOODWORKING_TREE_NAME = "Talentos Carpintería";
        public const string TAILORING_TREE_NAME = "Talentos Sastrería";
        #endregion
        
        #region Tags
        public static readonly Dictionary<byte, string> TagNames = new()
        {
            { 0, "Árbol" },
            { 1, "Yacimiento de Hierro" },
            { 2, "Yacimiento de Plata" },
            { 3, "Yacimiento de Oro" },
            { 4, "Árbol Elfico" },
            { 5, "Fragua" },
            { 6, "Yunque" },
            { 7, "Puerta" },
            { 8, "Techo" },
        };
        #endregion

        #region Animator
        public static readonly int FacingHash = Animator.StringToHash("Facing");
        public static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        public static readonly int BodyUpHash = Animator.StringToHash("Body-Up");
        public static readonly int ShieldUpHash = Animator.StringToHash("Shield-Up");
        public static readonly int WeaponUpHash = Animator.StringToHash("Weapon-Up");
        public static readonly int BodyDownHash = Animator.StringToHash("Body-Down");
        public static readonly int ShieldDownHash = Animator.StringToHash("Shield-Down");
        public static readonly int WeaponDownHash = Animator.StringToHash("Weapon-Down");
        public static readonly int BodyLeftHash = Animator.StringToHash("Body-Left");
        public static readonly int ShieldLeftHash = Animator.StringToHash("Shield-Left");
        public static readonly int WeaponLeftHash = Animator.StringToHash("Weapon-Left");
        public static readonly int BodyRightHash = Animator.StringToHash("Body-Right");
        public static readonly int ShieldRightHash = Animator.StringToHash("Shield-Right");
        public static readonly int WeaponRightHash = Animator.StringToHash("Weapon-Right");
        #endregion
    }
}