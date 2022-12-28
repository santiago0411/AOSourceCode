using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AO.Core.Utils
{
    public static class ExtensionMethods
    {
        /// <summary>Rounds this Vector2.</summary>
        public static Vector2 Round(this ref Vector2 vector2, int decimalPlaces = 2)
        {
            float multiplier = 1;
            for (int i = 0; i < decimalPlaces; i++)
                multiplier *= 10f;

            vector2.x = Mathf.Round(vector2.x * multiplier) / multiplier;
            vector2.y = Mathf.Round(vector2.y * multiplier) / multiplier;

            return vector2;
        }

        /// <summary>Casts this Vector2 to a Vector2Int.</summary>
        public static Vector2Int AsInt(this ref Vector2 v2)
        {
            return new Vector2Int((int)v2.x, (int)v2.y);
        }

        /// <summary>Calculates a percentage of a number.</summary>
        public static float Percentage(float total, float percentage)
        {
            return total * percentage / 100;
        }

        /// <summary>Returns a random integer between min and max. Both are inclusive.</summary>
        public static int RandomNumber(int minInclusive, int maxInclusive)
        {
            return Random.Range(minInclusive, maxInclusive + 1);
        }

        /// <summary>Returns a random float between min and max. Both are inclusive.</summary>
        public static float RandomNumber(float minInclusive, float maxInclusive)
        {
            return Random.Range(minInclusive, maxInclusive + 1);
        }

        public static void InitializeKeys<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
                dictionary.Add(key, default);
        }

        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return;
            }
            
            dictionary.Add(key, value);
        }

        public static TValue PopKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            var value = dictionary[key];
            dictionary.Remove(key);
            return value;
        }

        public static double GetMilliseconds(this System.Diagnostics.Stopwatch sw)
        {
            return ((double)sw.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency) * 1000;
        }
    }
}
