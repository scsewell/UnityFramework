using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public static class Utils
    {
        public static T PickRandom<T>(T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        public static T PickRandom<T>(List<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }

        public static T PickRandom<T>(IEnumerable<T> enumerable)
        {
            return enumerable.ElementAtOrDefault(Random.Range(0, enumerable.Count()));
        }

        public static T GetComponentInAnyParent<T>(GameObject go) where T : Component
        {
            return GetComponentInAnyParent<T>(go.transform);
        }

        public static T GetComponentInAnyParent<T>(Component component) where T : Component
        {
            T c = component.GetComponent<T>();
            if (c != null)
            {
                return c;
            }
            else if (component.transform.parent != null)
            {
                return GetComponentInAnyParent<T>(component.transform.parent);
            }
            else
            {
                return null;
            }
        }

        public static IEnumerator Wait(float delay)
        {
            float timer = Time.time + delay;
            while (Time.time < timer)
            {
                yield return null;
            }
        }
    }
}