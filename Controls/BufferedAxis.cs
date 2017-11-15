using UnityEngine;
using System.Collections.Generic;

namespace Framework.InputManagement
{
    /// <summary>
    /// Stores all the mouse and joystick axes that are relevant to a specific in game command.
    /// </summary>
    public class BufferedAxis : BufferedSource<float>
    {
        private float m_exponent;

        public BufferedAxis(string displayName, bool canRebind, bool canBeMuted, float exponent, ISource<float>[] defaultSources) : base(displayName, canRebind, canBeMuted, defaultSources)
        {
            m_exponent = exponent;
        }

        /*
         * Returns the value of the axes over the last gamplay update frame, or the last visual update.
         */
        public float GetValue(bool average)
        {
            List<List<float>> input = GetRelevantInput(false);
            float maxValue = 0;
            for (int i = 0; i < m_sources.Count; i++)
            {
                float value = 0;
                foreach (float axisValue in input[i])
                {
                    value += GetInputValue(m_sources[i], axisValue);
                }
                if (average && input[i].Count > 1)
                {
                    value /= input[i].Count;
                }
                if (Mathf.Abs(maxValue) < Mathf.Abs(value))
                {
                    maxValue = value;
                }
            }
            return maxValue;
        }

        /*
         * Applies modifications to the input values based on the type of source as required.
         */
        private float GetInputValue(ISource<float> source, float value)
        {
            return (source.SourceInfo.SourceType == SourceType.Joystick) ? Mathf.Sign(value) * Mathf.Pow(Mathf.Abs(value), m_exponent) : value;
        }
    }
}