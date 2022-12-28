using System;
using System.Collections.Generic;

namespace AOClient.Core.Utils
{
    public static class ExtensionMethods
    {
        public static void InitializeKeys<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
                dictionary.Add(key, default);
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
                action(item);
        }
    }
}