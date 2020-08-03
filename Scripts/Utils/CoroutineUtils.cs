using System.Collections;

using UnityEngine;

namespace Framework
{
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
