using System.Collections;

using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Utilities for managing coroutines.
    /// </summary>
    public static class CoroutineUtils
    {
        public static IEnumerator Wait(float delay)
        {
            var completeTime = Time.time + delay;
            while (Time.time < completeTime)
            {
                yield return null;
            }
        }
    }
}
