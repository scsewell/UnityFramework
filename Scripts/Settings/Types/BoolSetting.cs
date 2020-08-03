using UnityEngine;

namespace Framework.Settings
{
    [CreateAssetMenu(fileName = "New Bool Setting", menuName = "Framework/Settings/Bool", order = 1)]
    public class BoolSetting : Setting<bool>
    {
        private static readonly string[] DISPLAY_VALUES = new string[]
        {
            "Off",
            "On",
        };

        /// <inheritdoc/>
        public override string[] DisplayValues => DISPLAY_VALUES;

        /// <inheritdoc/>
        internal override bool Deserialize(string serialized, out bool value)
        {
            if (serialized == DISPLAY_VALUES[0])
            {
                value = false;
                return true;
            }
            if (serialized == DISPLAY_VALUES[1])
            {
                value = true;
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc/>
        internal override string Serialize(bool value)
        {
            return DISPLAY_VALUES[value ? 1 : 0];
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        internal override string OnInspectorGUI(Rect pos, string serializedValue)
        {
            Deserialize(serializedValue, out var deserialized);
            return Serialize(Sanitize(UnityEditor.EditorGUI.Toggle(pos, deserialized)));
        }
#endif
    }
}
