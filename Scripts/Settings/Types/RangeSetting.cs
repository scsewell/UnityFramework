using UnityEngine;

namespace Framework.Settings
{
    public abstract class RangeSetting<T> : Setting<T>
    {
        protected const int MAX_DISPLAY_VALUES = 250;

        [SerializeField]
        [Tooltip("The range the setting's values are constrained to.")]
        protected MinMaxRange m_range = new MinMaxRange(0, 100);

        [SerializeField]
        [Tooltip("The size of the increments in the value.")]
        protected float m_increment = 1.0f;

        protected string[] m_displayValues = null;

        /// <summary>
        /// The min value allowed.
        /// </summary>
        public abstract T Min { get; }

        /// <summary>
        /// The max value allowed.
        /// </summary>
        public abstract T Max { get; }

        public override string[] DisplayValues => m_displayValues;

        protected float SnapToRange(float value)
        {
            return Mathf.Round(m_range.Clamp(value) / m_increment) * m_increment;
        }
    }
}
