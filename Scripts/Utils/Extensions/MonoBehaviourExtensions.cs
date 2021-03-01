using System;
using System.Collections;

using UnityEngine;

namespace Framework
{
    /// <summary>
    /// A class containing <see cref="MonoBehaviour"/> extension methods.
    /// </summary>
    public static class MonoBehaviourExtensions
    {
        /// <summary>
        /// Starts a coroutine that executes an action after a timed delay.
        /// </summary>
        /// <param name="behaviour">The MonoBehaviour to run the coroutine on.</param>
        /// <param name="action">The action to execute.</param>
        /// <param name="delay">The time in seconds to wait before executing the action.</param>
        /// <returns>The running coroutine instance, or null if <paramref name="delay"/> is not greater than 0.</returns>
        public static Coroutine DelayedCall(this MonoBehaviour behaviour, Action action, float delay)
        {
            if (delay > 0f)
            {
                return behaviour.StartCoroutine(DelayedCall(action, delay));
            }
            else
            {
                action?.Invoke();
                return null;
            }
        }

        private static IEnumerator DelayedCall(Action action, float delay)
        {
            yield return CoroutineUtils.Wait(delay);
            action?.Invoke();
        }
    }
}
