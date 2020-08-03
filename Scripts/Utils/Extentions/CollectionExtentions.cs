using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Framework
{
    /// <summary>
    /// A class contaning extention methods for collections.
    /// </summary>
    public static class CollectionExtentions
    {
        /// <summary>
        /// Gets a random value from the collection.
        /// </summary>
        /// <typeparam name="T">The type of value stored in the collection.</typeparam>
        /// <param name="array">The colletion to pick the value from.</param>
        /// <returns>A random value from the collection.</returns>
        public static T PickRandom<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        /// <summary>
        /// Gets a random value from the collection.
        /// </summary>
        /// <typeparam name="T">The type of value stored in the collection.</typeparam>
        /// <param name="list">The colletion to pick the value from.</param>
        /// <returns>A random value from the collection.</returns>
        public static T PickRandom<T>(this IList<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Gets a random value from the collection.
        /// </summary>
        /// <typeparam name="T">The type of value stored in the collection.</typeparam>
        /// <param name="collection">The colletion to pick the value from.</param>
        /// <returns>A random value from the collection.</returns>
        public static T PickRandom<T>(this ICollection<T> collection)
        {
            return collection.ElementAt(Random.Range(0, collection.Count));
        }

        /// <summary>
        /// Gets a random value from the collection.
        /// </summary>
        /// <typeparam name="T">The type of value stored in the collection.</typeparam>
        /// <param name="enumerable">The colletion to pick the value from.</param>
        /// <returns>A random value from the collection.</returns>
        public static T PickRandom<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ElementAt(Random.Range(0, enumerable.Count()));
        }

        /// <summary>
        /// Removes a random value from the collection.
        /// </summary>
        /// <typeparam name="T">The type of value stored in the collection.</typeparam>
        /// <param name="list">The collection to remove the value from.</param>
        /// <returns>The removed element.</returns>
        public static T RemoveRandom<T>(this IList<T> list)
        {
            var index = Random.Range(0, list.Count);
            var item = list[index];
            list.RemoveAt(index);
            return item;
        }
    }
}
