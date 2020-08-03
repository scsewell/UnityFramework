using System.Collections.Generic;

using UnityEngine;

namespace Framework.Settings
{
    [CreateAssetMenu(fileName = "Float Setting", menuName = "Framework/Settings/Float", order = 3)]
    public class FloatSetting : RangeSetting<float>
    {
        /// <inheritdoc/>
        public override float Min => m_range.Min;

        /// <inheritdoc/>
        public override float Max => m_range.Max;

        /// <inheritdoc/>
        internal override void Initialize()
        {
            base.Initialize();

            var values = new List<string>();

            var num = Min;
            while (num <= Max)
            {
                if (values.Count == MAX_DISPLAY_VALUES)
                {
                    Debug.LogError($"Setting \"{name}\" has too many display values!");
                    break;
                }

                values.Add(num.ToString());
                num += m_increment;
            }

            m_displayValues = values.ToArray();
        }

        /// <inheritdoc/>
        internal override float Sanitize(float newValue)
        {
            return SnapToRange(newValue);
        }

        /// <inheritdoc/>
        internal override bool Deserialize(string serialized, out float value)
        {
            return float.TryParse(serialized, out value);
        }

        /// <inheritdoc/>
        internal override string Serialize(float value)
        {
            return value.ToString();
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        internal override string OnInspectorGUI(Rect pos, string serializedValue)
        {
            Deserialize(serializedValue, out var deserialized);
            return Serialize(Sanitize(UnityEditor.EditorGUI.Slider(pos, deserialized, Min, Max)));
        }
#endif
    }
}
