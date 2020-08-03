using UnityEditor;

using UnityEngine;

namespace Framework.Settings
{
    /// <summary>
    /// The base settings editor.
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Setting), true)]
    public class SettingEditor : Editor
    {
        protected SerializedProperty m_category = null;
        protected SerializedProperty m_description = null;
        protected SerializedProperty m_displayMode = null;
        protected SerializedProperty m_defaultValue = null;

        protected virtual void OnEnable()
        {
            m_category = serializedObject.FindProperty("m_category");
            m_description = serializedObject.FindProperty("m_description");
            m_displayMode = serializedObject.FindProperty("m_displayMode");
            m_defaultValue = serializedObject.FindProperty("m_defaultValue");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_category);
            EditorGUILayout.PropertyField(m_description);
            EditorGUILayout.PropertyField(m_displayMode);

            CustomGUI();

            serializedObject.ApplyModifiedProperties();

            if (target is Setting setting && !setting.IsRuntime)
            {
                var rect = EditorGUILayout.GetControlRect();
                var label = new GUIContent(m_defaultValue.displayName, m_defaultValue.tooltip);
                SettingValueDrawer.Draw(rect, label, m_defaultValue, setting);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Implement to add UI needed for custom setting types.
        /// </summary>
        protected virtual void CustomGUI()
        {
        }
    }
}
