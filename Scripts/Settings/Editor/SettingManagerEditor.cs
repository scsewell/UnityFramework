using UnityEngine;
using UnityEditor;

namespace Framework.Settings
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SettingManager))]
    public class SettingManagerEditor : Editor
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
            SettingManager manager = target as SettingManager;

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(m_autoAddSettings);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Add All Settings"))
            {
                manager.AddAllSettings();
            }

            EditorGUILayout.EndHorizontal();

            manager.RemoveDuplicateSettings();
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(m_settings);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
