using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Utilities for doing common math operations.
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Damps a towards b.
        /// </summary>
        /// <remarks>
        /// Use this as opposed to <see cref="Mathf.Lerp(float, float, float)"/> when
        /// lerping based on time, this will give framerate invariant results.
        /// </remarks>
        /// <param name="a">The source value.</param>
        /// <param name="b">The target value.</param>
        /// <param name="smoothing">The smoothing rate, giving the proprtion of <paramref name="a"/>
        /// remaining after one second. A zero value results in no smoothing (returns <paramref name="a"/>),
        /// while a value of one results in infinite smoothing (returns <paramref name="b"/>).</param>
        /// <returns>The damped value.</returns>
        /// <seealso href="http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/"/>
        public static float Damp(float a, float b, float smoothing)
        {
            return Damp(a, b, smoothing, Time.deltaTime);
        }

        /// <summary>
        /// Damps a towards b.
        /// </summary>
        /// <remarks>
        /// Use this as opposed to <see cref="Mathf.Lerp(float, float, float)"/> when
        /// lerping based on time, this will give framerate invariant results.
        /// </remarks>
        /// <param name="a">The source value.</param>
        /// <param name="b">The target value.</param>
        /// <param name="smoothing">The smoothing rate, giving the proprtion of <paramref name="a"/>
        /// remaining after one second. A zero value results in no smoothing (returns <paramref name="a"/>),
        /// while a value of one results in infinite smoothing (returns <paramref name="b"/>).</param>
        /// <param name="dt">The delta time in seconds.</param>
        /// <returns>The damped value.</returns>
        /// <seealso href="http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/"/>
        public static float Damp(float a, float b, float smoothing, float dt)
        {
            return Mathf.Lerp(a, b, 1f - Mathf.Pow(Mathf.Clamp01(smoothing), dt));
        }
    }
}
