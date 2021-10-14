using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    /// <summary>
    /// A struct representing a continuous number range.
    /// </summary>
    [Serializable]
    public struct MinMaxRange
    {
        [SerializeField]
        private float m_min;

        /// <summary>
        /// The lower bound on this range.
        /// </summary>
        public float Min
        {
            get => m_min;
            set => m_min = value;
        }

        [SerializeField]
        private float m_max;

        /// <summary>
        /// The upper bound on this range.
        /// </summary>
        public float Max
        {
            get => m_max;
            set => m_max = value;
        }

        /// <summary>
        /// Gets this range as a vector with x as the min value and y as the max value.
        /// </summary>
        public Vector2 AsVector => new Vector2(m_min, m_max);

        /// <summary>
        /// Creates a new range.
        /// </summary>
        /// <param name="min">The lower bound on this range.</param>
        /// <param name="max">The upper bound on this range.</param>
        public MinMaxRange(float min, float max)
        {
            m_min = min;
            m_max = max;
        }

        /// <summary>
        /// Clamps a value to this range.
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The clamped value.</returns>
        public float Clamp(float value) => Mathf.Clamp(value, m_min, m_max);

        /// <summary>
        /// Interpolates from the lower bound to the upper bound by t. 
        /// </summary>
        /// <param name="t">The interpolation factor.</param>
        /// <returns>The interpolated value on the range.</returns>
        public float Lerp(float t) => Mathf.Lerp(m_min, m_max, t);

        /// <summary>
        /// Gets the interpolation factor which results in a value on the range. 
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns>The interpolation factor.</returns>
        public float InverseLerp(float value) => Mathf.InverseLerp(m_min, m_max, value);
    }

    /// <summary>
    /// An attribute palced on serialized <see cref="MinMaxRange"/> fields to make the
    /// range editable using a min-max range slider in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        internal readonly float min;
        internal readonly float max;
        internal readonly float increment;

        /// <summary>
        /// Configures how a field supporting a min-max slider behaves in the inspector.
        /// </summary>
        /// <param name="min">The minimum value which can be assigned to the range.</param>
        /// <param name="max">The maximum value which can be assigned to the range.</param>
        public MinMaxSliderAttribute(float min, float max) : this(min, max, 0f)
        {
        }

        /// <summary>
        /// Configures how a field supporting a min-max slider behaves in the inspector.
        /// </summary>
        /// <param name="min">The minimum value which can be assigned to the range.</param>
        /// <param name="max">The maximum value which can be assigned to the range.</param>
        /// <param name="increment">The snapping amount the range endpoints are constrained to.</param>
        public MinMaxSliderAttribute(float min, float max, float increment)
        {
            this.min = min;
            this.max = max;
            this.increment = increment;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    internal class MinMaxSliderDrawer : PropertyDrawer
    {
        private const float FLOAT_FIELD_WIDTH = 30f;
        private const float SPACING = 2f;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            // get the required properties and check that the property is a valid type
            var min = prop.FindPropertyRelative("m_min");
            var max = prop.FindPropertyRelative("m_max");

            if (min == null || max == null)
            {
                EditorGUI.HelpBox(pos, $"{nameof(MinMaxSliderAttribute)} only supports fields of type {nameof(MinMaxRange)}!", MessageType.Error);
                return;
            }

            var attr = attribute as MinMaxSliderAttribute;

            using (var property = new EditorGUI.PropertyScope(pos, label, prop))
            {
                pos = EditorGUI.PrefixLabel(pos, property.content);

                // draw the slider
                var minValue = min.floatValue;
                var maxValue = max.floatValue;

                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    var spacing = SPACING * EditorGUIUtility.pixelsPerPoint;
                    var sliderPos = pos;
                    sliderPos.x += EditorGUIUtility.labelWidth + FLOAT_FIELD_WIDTH + spacing;
                    sliderPos.width -= EditorGUIUtility.labelWidth + (2f * (FLOAT_FIELD_WIDTH + spacing));

                    EditorGUI.showMixedValue = min.hasMultipleDifferentValues || max.hasMultipleDifferentValues;

                    EditorGUI.MinMaxSlider(sliderPos, ref minValue, ref maxValue, attr.min, attr.max);

                    if (change.changed)
                    {
                        min.floatValue = Round(attr, minValue);
                        max.floatValue = Round(attr, maxValue);
                    }
                }

                // draw the min value
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    var minPos = pos;
                    minPos.x += EditorGUIUtility.labelWidth;
                    minPos.width = FLOAT_FIELD_WIDTH;

                    EditorGUI.showMixedValue = min.hasMultipleDifferentValues;

                    minValue = EditorGUI.FloatField(minPos, minValue);

                    if (change.changed)
                    {
                        min.floatValue = Mathf.Max(Round(attr, minValue), attr.min);
                    }
                }

                // draw the max value
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    var maxPos = pos;
                    maxPos.x += maxPos.width - FLOAT_FIELD_WIDTH;
                    maxPos.width = FLOAT_FIELD_WIDTH;

                    EditorGUI.showMixedValue = max.hasMultipleDifferentValues;

                    maxValue = EditorGUI.FloatField(maxPos, maxValue);

                    if (change.changed)
                    {
                        max.floatValue = Mathf.Min(Round(attr, maxValue), attr.max);
                    }
                }
            }
        }

        private float Round(MinMaxSliderAttribute attr, float value)
        {
            if (Mathf.Approximately(attr.increment, 0f))
            {
                return value;
            }
            return Mathf.Round(value / attr.increment) * attr.increment;
        }
    }
#endif
}
