using System.Collections.Generic;

using UnityEngine;

namespace Framework.Settings
{
    [CreateAssetMenu(fileName = "Int Setting", menuName = "Framework/Settings/Int", order = 2)]
    public class IntSetting : RangeSetting<int>
    {
        /// <inheritdoc/>
        public override int Min => Mathf.RoundToInt(SnapToRange(m_range.Min));

        /// <inheritdoc/>
        public override int Max => Mathf.RoundToInt(SnapToRange(m_range.Max));

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
                num += Mathf.RoundToInt(m_increment);
            }

            m_displayValues = values.ToArray();
        }

        /// <inheritdoc/>
        internal override int Sanitize(int newValue)
        {
            return Mathf.RoundToInt(SnapToRange(newValue));
        }

        /// <inheritdoc/>
        internal override bool Deserialize(string serialized, out int value)
        {
            return int.TryParse(serialized, out value);
        }

        /// <inheritdoc/>
        internal override string Serialize(int value)
        {
            return value.ToString();
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        internal override string OnInspectorGUI(Rect pos, string serializedValue)
        {
            Deserialize(serializedValue, out var deserialized);
            return Serialize(Sanitize(UnityEditor.EditorGUI.IntSlider(pos, deserialized, Min, Max)));
        }
#endif
    }
}
