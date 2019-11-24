using UnityEngine;
using System.Collections.Generic;

namespace Framework.Interpolation
{
    /// <summary>
    /// Controls the updating of all interpolated values. Must execute before any interpolated
    /// values are changed each frame.
    /// </summary>
    public static class InterpolationController
    {
        private static readonly List<IInterpolator> m_interpolators = new List<IInterpolator>();
        private static float m_lastFixedTime;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            m_interpolators.Clear();
            m_lastFixedTime = 0f;
        }

        /// <summary>
        /// Called prior to modifying any interpolated values in fixed update.
        /// </summary>
        public static void EarlyFixedUpdate()
        {
            m_lastFixedTime = Time.time;

            for (int i = 0; i < m_interpolators.Count; i++)
            {
                m_interpolators[i].FixedFrame();
            }
        }

        /// <summary>
        /// Called every frame to interpolate the values.
        /// </summary>
        public static void VisualUpdate()
        {
            float factor = (Time.time - m_lastFixedTime) / Time.fixedDeltaTime;

            for (int i = 0; i < m_interpolators.Count; i++)
            {
                m_interpolators[i].UpdateFrame(factor);
            }
        }

        /// <summary>
        /// Adds an interpolator.
        /// </summary>
        /// <param name="interpolator">The interpolator to enable.</param>
        public static void AddInterpolator(IInterpolator interpolator)
        {
            if (!m_interpolators.Contains(interpolator))
            {
                m_interpolators.Add(interpolator);
            }
        }

        /// <summary>
        /// Removes an interpolator.
        /// </summary>
        /// <param name="interpolator">The interpolator to remove.</param>
        public static void RemoveInterpolator(IInterpolator interpolator)
        {
            m_interpolators.Remove(interpolator);
        }
    }
}