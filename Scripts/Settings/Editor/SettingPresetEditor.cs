using UnityEngine;
using UnityEditor;

namespace Framework.Settings
{
    [CustomPropertyDrawer(typeof(SettingPreset))]
    public class SettingPresetEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty setting = property.FindPropertyRelative("m_setting");
            SerializedProperty value = property.FindPropertyRelative("m_value");

            float width = Mathf.Min(250f, position.width / 2f);

            Rect settingRect = position;
            settingRect.width = width;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(settingRect, setting, new GUIContent());
            if (EditorGUI.EndChangeCheck())
            {
                value.stringValue = string.Empty;
            }

            Rect valueRect = position;
            valueRect.xMin = settingRect.xMax + 10f;

            SettingValueDrawer.DrawSettingValueSelector(valueRect, setting.objectReferenceValue as Setting, value);
        }
    }
}
