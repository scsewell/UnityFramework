using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    /// <summary>
    /// Represents a continuous range.
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
        /// Gets a uniformly random value on the range.
        /// </summary>
        public float Random => UnityEngine.Random.Range(m_min, m_max);

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
        /// Interpolation from the lower bound to the upper bound by t. 
        /// </summary>
        /// <param name="t">The interpolation factor.</param>
        /// <returns>The interplated value on the range.</returns>
        public float Lerp(float t)
        {
            return Mathf.Lerp(m_min, m_max, t);
        }

        /// <summary>
        /// Gets the interpolation factor which results in a value on the range. 
        /// </summary>
        /// <param name="value">A value.</param>
        /// <returns>The interplation factor.</returns>
        public float InverseLerp(float value)
        {
            return Mathf.InverseLerp(m_min, m_max, value);
        }
    }

    /// <summary>
    /// An attribute used to draw a field as a min-max range slider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public readonly float min;
        public readonly float max;
        public readonly float increment;

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
    class MinMaxSliderDrawer : PropertyDrawer
    {
        private const float cFloatFieldWidth = 50f;
        private const float cSpacing = 5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // get the required properties and check that the property is a valid type
            SerializedProperty min = property.FindPropertyRelative("m_min");
            SerializedProperty max = property.FindPropertyRelative("m_max");

            if (min == null || max == null)
            {
                EditorGUI.HelpBox(position, "MinMaxSlider attribute only supports fields of type MixMaxRange!", MessageType.Error);
                return;
            }

            // start the property
            MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;

            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PrefixLabel(position, label);

            // draw the slider
            float spacing = cSpacing * EditorGUIUtility.pixelsPerPoint;
            Rect sliderPos = position;
            sliderPos.x += EditorGUIUtility.labelWidth + cFloatFieldWidth + spacing;
            sliderPos.width -= EditorGUIUtility.labelWidth + (cFloatFieldWidth + spacing) * 2;

            float minValue = min.floatValue;
            float maxValue = max.floatValue;

            EditorGUI.showMixedValue = min.hasMultipleDifferentValues || max.hasMultipleDifferentValues;

            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(sliderPos, ref minValue, ref maxValue, attr.min, attr.max);
            if (EditorGUI.EndChangeCheck())
            {
                min.floatValue = Round(attr, minValue);
                max.floatValue = Round(attr, maxValue);
            }

            // draw the min value
            Rect minPos = position;
            minPos.x += EditorGUIUtility.labelWidth;
            minPos.width = cFloatFieldWidth;

            EditorGUI.showMixedValue = min.hasMultipleDifferentValues;

            EditorGUI.BeginChangeCheck();
            minValue = EditorGUI.FloatField(minPos, minValue);
            if (EditorGUI.EndChangeCheck())
            {
                min.floatValue = Mathf.Max(Round(attr, minValue), attr.min);
            }

            // draw the max value
            Rect maxPos = position;
            maxPos.x += maxPos.width - cFloatFieldWidth;
            maxPos.width = cFloatFieldWidth;

            EditorGUI.showMixedValue = max.hasMultipleDifferentValues;

            EditorGUI.BeginChangeCheck();
            maxValue = EditorGUI.FloatField(maxPos, maxValue);
            if (EditorGUI.EndChangeCheck())
            {
                max.floatValue = Mathf.Min(Round(attr, maxValue), attr.max);
            }

            // end the property
            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();
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
