using UnityEditor;

using UnityEngine;

namespace Framework.Settings
{
    [CustomPropertyDrawer(typeof(SettingPreset))]
    internal class SettingPresetEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var setting = property.FindPropertyRelative("m_setting");
            var value = property.FindPropertyRelative("m_value");

            var width = Mathf.Min(250f, position.width / 2f);

            var settingRect = position;
            settingRect.width = width;

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.PropertyField(settingRect, setting, new GUIContent());

                if (change.changed)
                {
                    value.stringValue = string.Empty;
                }
            }

            var valueRect = position;
            valueRect.xMin = settingRect.xMax + 10f;

            SettingValueDrawer.Draw(valueRect, GUIContent.none, value, setting.objectReferenceValue as Setting);
        }
    }
}
