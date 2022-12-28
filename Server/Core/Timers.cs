using UnityEngine;
using AO.Core.Utils;
using AO.Npcs;
using AO.Players;

namespace AO.Core
{
    public static class Timers
    {
        public static bool PlayerCanAttackInterval(Player player, bool update = true)
        {
            float now = Time.realtimeSinceStartup;

            if ((now - player.Timers.LastAttackTime) >= Constants.PLAYER_CAN_ATTACK_TIME)
            {
                if (update)
                {
                    player.Timers.LastAttackTime = now;
                    player.Timers.LastAttackUseTime = now;
                    player.Timers.LastCastTime = now - (Constants.PLAYER_CAN_CAST_SPELL_TIME - Constants.PLAYER_CAN_ATTACK_CAST_TIME);
                }

                return true;
            }

            return false;
        }

        public static bool PlayerCanUseInterval(Player player, bool update = true)
        {
            if ((Time.realtimeSinceStartup - player.Timers.LastUseTime) >= Constants.PLAYER_CAN_USE_TIME)
            {
                if (update)
                    player.Timers.LastUseTime = Time.realtimeSinceStartup;

                return true;
            }

            return false;
        }

        public static bool PlayerCanCastSpellInterval(Player player, bool update = true)
        {
            float now = Time.realtimeSinceStartup;

            if ((now - player.Timers.LastCastTime) >= Constants.PLAYER_CAN_CAST_SPELL_TIME)
            {
                if (update)
                {
                    player.Timers.LastCastTime = now;
                    player.Timers.LastAttackTime = now - (Constants.PLAYER_CAN_ATTACK_TIME - Constants.PLAYER_CAN_CAST_ATTACK_TIME);
                }

                return true;
            }

            return false;
        }

        public static bool PlayerCanAttackUseInterval(Player player, bool update = true)
        {
            if ((Time.realtimeSinceStartup - player.Timers.LastAttackUseTime) >= Constants.PLAYER_CAN_ATTACK_USE_TIME)
            {
                if (update)
                    player.Timers.LastAttackUseTime = Time.realtimeSinceStartup;

                return true;
            }

            return false;
        }

        public static bool PlayerCanUseBowInterval(Player player, bool update = true)
        {
            if ((Time.realtimeSinceStartup - player.Timers.LastUseBowTime) >= Constants.PLAYER_CAN_USE_BOW_TIME)
            {
                if (update)
                    player.Timers.LastUseBowTime = Time.realtimeSinceStartup;

                return true;
            }

            return false;
        }

        public static bool NpcCanAttackInterval(Npc npc)
        {
            if ((Time.realtimeSinceStartup - npc.Flags.LastAttackTime) >= Constants.NPC_CAN_ATTACK_TIME)
            {
                npc.Flags.LastAttackTime = Time.realtimeSinceStartup;
                return true;
            }

            return false;
        }

        public static bool NpcCanCastSpellInterval(Npc npc)
        {
            if ((Time.realtimeSinceStartup - npc.Flags.LastSpellTime) >= Constants.NPC_CAN_CAST_SPELL_TIME)
            {
                npc.Flags.LastSpellTime = Time.realtimeSinceStartup;
                return true;
            }

            return false;
        }

        public static bool PlayerLostNpcInterval(Player player, bool update = false)
        {
            if (update)
            {
                player.Timers.OwnsNpcTime = Time.realtimeSinceStartup;
                return false;
            }

            if ((Time.realtimeSinceStartup - player.Timers.OwnsNpcTime) >= Constants.PLAYER_OWNS_NPC_TIME)
                return true;

            return false;
        }
    }
}
