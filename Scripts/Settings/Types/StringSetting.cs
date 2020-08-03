using UnityEngine;

namespace Framework.Settings
{
    [CreateAssetMenu(fileName = "New String Setting", menuName = "Framework/Settings/String", order = 5)]
    public class StringSetting : Setting<string>
    {
        /// <inheritdoc/>
        public override string[] DisplayValues => new string[0];

        /// <inheritdoc/>
        internal override bool Deserialize(string serialized, out string value)
        {
            if (serialized != null)
            {
                value = serialized;
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc/>
        internal override string Serialize(string value)
        {
            return value;
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        internal override string OnInspectorGUI(Rect pos, string serializedValue)
        {
            Deserialize(serializedValue, out var deserialized);
            return Serialize(Sanitize(UnityEditor.EditorGUI.TextField(pos, deserialized)));
        }
#endif
    }
}
