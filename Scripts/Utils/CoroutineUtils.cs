using System.Collections;

using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Utilities for managing coroutines.
    /// </summary>
    public static class CoroutineUtils
    {
        /// <summary>
        /// Skips frames until the givem amout of game time has passed.
        /// </summary>
        /// <param name="duration">The time in seconds to wait.</param>
        public static IEnumerator Wait(float duration)
        {
            var startTime = Time.time;

            while (Time.time - startTime < duration)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Waits for the given amout of frames.
        /// </summary>
        /// <param name="frames">The number of frames to wait.</param>
        public static IEnumerator Wait(int frames)
        {
            var startFrame = Time.frameCount;

            while (Time.frameCount - startFrame != frames)
            {
                yield return null;
            }
        }
    }
}
