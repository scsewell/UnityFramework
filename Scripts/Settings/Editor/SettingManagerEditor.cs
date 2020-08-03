using UnityEditor;

using UnityEngine;

namespace Framework.Settings
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SettingManager))]
    internal class SettingManagerEditor : Editor
    {
        protected SerializedProperty m_autoAddSettings = null;
        protected SerializedProperty m_settings = null;

        protected virtual void OnEnable()
        {
            m_autoAddSettings = serializedObject.FindProperty("m_autoAddSettings");
            m_settings = serializedObject.FindProperty("m_settings");
        }

        public override void OnInspectorGUI()
        {
            var manager = target as SettingManager;

            serializedObject.Update();

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(m_autoAddSettings);
                serializedObject.ApplyModifiedProperties();

                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(m_autoAddSettings.boolValue))
                {
                    if (GUILayout.Button("Add All Settings"))
                    {
                        manager.AddAllSettings();
                    }
                }
            }

            manager.RemoveDuplicateSettings();
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_settings);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
