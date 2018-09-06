using System;
using System.Collections;
using UnityEngine;

namespace Framework
{
    public static class MonoBehaviourExtentions
    {
        public static Coroutine DelayedCall(this MonoBehaviour behaviour, Action action, float delay)
        {
            if (delay > 0)
            {
                return behaviour.StartCoroutine(DelayedCall(action, delay));
            }
            action?.Invoke();
            return null;
        }

        private static IEnumerator DelayedCall(Action action, float delay)
        {
            yield return CoroutineUtils.Wait(delay);
            action?.Invoke();
        }
    }
}
