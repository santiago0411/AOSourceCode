using System;
using AO.Core.Logging;
using UnityEditor;
using UnityEngine;
using AO.Core.Utils;

namespace AO.Npcs.Utils
{
    [Serializable]
    #pragma warning disable 0649
    public class Spawner
    {
        public NpcIdEnum Npc => npc;
        public int Amount => amount;
        public NpcSpawner.RespawnType RespawnType => respawnType;
        public DateTime BeginRespawn { get; private set; }
        public DateTime EndRespawn { get; private set; }
        public float NextSpawnTime { get; private set; }
        public float LastSpawnTime { get; set; }
        public bool Spawned { get; set; }
        public int CurrentSpawnsCount { get; private set; }

        [SerializeField] private NpcIdEnum npc;
        [SerializeField] private int amount;
        [SerializeField] private NpcSpawner.RespawnType respawnType;
        [SerializeField] private float minRespawnTime, maxRespawnTime;
        [SerializeField] private string beginRespawn, endRespawn;

        private static readonly LoggerAdapter log = new(typeof(Spawner));

        public void ResetSpawnTimer()
        {
            NextSpawnTime = Time.realtimeSinceStartup  + ExtensionMethods.RandomNumber(minRespawnTime, maxRespawnTime);
        }

        public void SetDateTimes()
        {
            if (!DateTime.TryParseExact(beginRespawn, "HH:mm", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out DateTime beginAux))
                log.Warn($"Spawner couldn't parse starting datetime.");

            BeginRespawn = beginAux;

            if (!DateTime.TryParseExact(endRespawn, "HH:mm", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out DateTime endAux))
                log.Warn($"Spawner couldn't parse ending datetime.");

            EndRespawn = endAux;
        }

        public void IncreaseCount()
        {
            CurrentSpawnsCount++;
        }

        public void DecreaseCount()
        {
            CurrentSpawnsCount--;
        }
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Spawner))]
    public class SpawnerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Get all the serialized fields of Spawner class
            SerializedProperty npc = property.FindPropertyRelative("npc");
            SerializedProperty amount = property.FindPropertyRelative("amount");
            SerializedProperty respawnTypeProp = property.FindPropertyRelative("respawnType");
            SerializedProperty minRespawnTime = property.FindPropertyRelative("minRespawnTime");
            SerializedProperty maxRespawnTime = property.FindPropertyRelative("maxRespawnTime");
            SerializedProperty beginRespawn = property.FindPropertyRelative("beginRespawn");
            SerializedProperty endRespawn = property.FindPropertyRelative("endRespawn");

            //Begin property wrapper
            EditorGUI.BeginProperty(position, label, property);

            //Set the position for each label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            position.x -= 50f;
            position.width *= 1.2f;

            //Add npc property
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), npc);
            //Move the y label position down
            position.y += EditorGUIUtility.singleLineHeight;

            //Add amount property
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), amount);
            //Move the y label position down
            position.y += EditorGUIUtility.singleLineHeight;

            //Add respawn type property
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), respawnTypeProp);
            //Move the y label position down
            position.y += EditorGUIUtility.singleLineHeight;

            //Get the respawn type
            var respawnType = (NpcSpawner.RespawnType)respawnTypeProp.enumValueIndex;
            //Add the rest of the properties according to the respawn type selected
            if (respawnType == NpcSpawner.RespawnType.TimedRespawn)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), minRespawnTime);
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), maxRespawnTime);
            }
            else if (respawnType == NpcSpawner.RespawnType.TimeFrameRespawnOnce)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), beginRespawn);
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), endRespawn);
            }
            else if (respawnType == NpcSpawner.RespawnType.TimeFrameConstantRespawn)
            {
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), beginRespawn);
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), endRespawn);
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), minRespawnTime);
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), maxRespawnTime);
            }

            //End the wrapper
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty respawnTypeProp = property.FindPropertyRelative("respawnType");

            var respawnType = (NpcSpawner.RespawnType)respawnTypeProp.enumValueIndex;
            return respawnType switch
            {
                NpcSpawner.RespawnType.TimedRespawn => EditorGUIUtility.singleLineHeight * 5,
                NpcSpawner.RespawnType.TimeFrameRespawnOnce => EditorGUIUtility.singleLineHeight * 5,
                NpcSpawner.RespawnType.TimeFrameConstantRespawn => EditorGUIUtility.singleLineHeight * 8,
                _ => EditorGUIUtility.singleLineHeight * 3,
            };
        }
    }
    #endif
}
