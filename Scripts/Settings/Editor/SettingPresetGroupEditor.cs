using System.Collections.Generic;

using UnityEditor;

namespace Framework.Settings
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SettingPresetGroup))]
    internal class SettingPresetGroupEditor : Editor
    {
        protected SerializedProperty m_presets = null;

        protected virtual void OnEnable()
        {
            m_presets = serializedObject.FindProperty("m_presets");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_presets);

            // determine which settings are not valid for a preset
            var runtimeSetings = new List<Setting>();

            for (var i = 0; i < m_presets.arraySize; i++)
            {
                var item = m_presets.GetArrayElementAtIndex(i);
                var settingProp = item.FindPropertyRelative("m_setting");

                if (settingProp.objectReferenceValue is Setting setting && setting.IsRuntime)
                {
                    runtimeSetings.Add(setting);
                }
            }

            EditorGUILayout.Space();

            // display a message box with all the invalid settings
            if (runtimeSetings.Count > 0)
            {
                foreach (var setting in runtimeSetings)
                {
                    EditorGUILayout.HelpBox($"Runtime setting \"{setting.name}\" cannot have a preset value!", MessageType.Error);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
