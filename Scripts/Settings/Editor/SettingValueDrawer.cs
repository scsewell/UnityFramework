using UnityEditor;

using UnityEngine;

namespace Framework.Settings
{
    internal static class SettingValueDrawer
    {
        public static void Draw(Rect pos, GUIContent label, SerializedProperty prop, Setting setting)
        {
            using (var property = new EditorGUI.PropertyScope(pos, label, prop))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                if (label != GUIContent.none)
                {
                    pos = EditorGUI.PrefixLabel(pos, property.content);
                }

                if (setting == null)
                {
                    return;
                }

                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;

                var oldValue = prop.stringValue;
                var newValue = setting.OnInspectorGUI(pos, oldValue);

                if (change.changed || string.IsNullOrEmpty(oldValue))
                {
                    prop.stringValue = newValue;
                }
            }
        }
    }
}
