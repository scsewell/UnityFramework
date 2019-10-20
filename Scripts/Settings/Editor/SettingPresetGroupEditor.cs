using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Framework.Settings
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SettingPresetGroup))]
    public class SettingPresetGroupEditor : Editor
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
            List<Setting> runtimeSetings = new List<Setting>();

            SerializedProperty list = m_presets.FindPropertyRelative("array");
            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty item = list.GetArrayElementAtIndex(i);
                SerializedProperty settingProp = item.FindPropertyRelative("m_setting");

                if (settingProp.objectReferenceValue is Setting setting)
                {
                    if (setting.IsRuntime)
                    {
                        runtimeSetings.Add(setting);
                    }
                }
            }

            EditorGUILayout.Space();

            // display a message box with all the invalid settings
            if (runtimeSetings.Count > 0)
            {
                foreach (Setting setting in runtimeSetings)
                {
                    EditorGUILayout.HelpBox($"Runtime setting \"{setting.name}\" cannot have a preset value!", MessageType.Error);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
