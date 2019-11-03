using UnityEngine;
using UnityEditor;

namespace Framework.Audio
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Music))]
    public class MusicEditor : Editor
    {
        protected SerializedProperty m_track = null;
        protected SerializedProperty m_name = null;
        protected SerializedProperty m_artist = null;
        protected SerializedProperty m_loop = null;
        protected SerializedProperty m_minutes = null;
        protected SerializedProperty m_seconds = null;

        protected virtual void OnEnable()
        {
            m_track = serializedObject.FindProperty("m_track");
            m_name = serializedObject.FindProperty("m_name");
            m_artist = serializedObject.FindProperty("m_artist");
            m_loop = serializedObject.FindProperty("m_loop");
            m_minutes = serializedObject.FindProperty("m_minutes");
            m_seconds = serializedObject.FindProperty("m_seconds");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_track);
            EditorGUILayout.PropertyField(m_name);
            EditorGUILayout.PropertyField(m_artist);
            EditorGUILayout.PropertyField(m_loop);

            serializedObject.ApplyModifiedProperties();

            if ((target as Music).Loop == Music.LoopMode.AtTime)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(m_minutes);
                EditorGUILayout.PropertyField(m_seconds);

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
