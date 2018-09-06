using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public static class CollectionExtentions
    {
        public static T PickRandom<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        public static T PickRandom<T>(this IList<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }

        public static T PickRandom<T>(this ICollection<T> collection)
        {
            return collection.ElementAtOrDefault(Random.Range(0, collection.Count));
        }

        public static T PickRandom<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ElementAtOrDefault(Random.Range(0, enumerable.Count()));
        }
    }
}
