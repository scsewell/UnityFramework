using UnityEditor;

using UnityEngine;

namespace Framework.Settings
{
    internal class RangeSettingEditor : SettingEditor
    {
        protected SerializedProperty m_range = null;
        protected SerializedProperty m_rangeMin = null;
        protected SerializedProperty m_rangeMax = null;
        protected SerializedProperty m_increment = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_range = serializedObject.FindProperty("m_range");
            m_rangeMin = m_range.FindPropertyRelative("m_min");
            m_rangeMax = m_range.FindPropertyRelative("m_max");
            m_increment = serializedObject.FindProperty("m_increment");
        }

        protected override void CustomGUI()
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(m_range.displayName));

                EditorGUILayout.LabelField(m_rangeMin.displayName, GUILayout.MaxWidth(30f));
                EditorGUILayout.PropertyField(m_rangeMin, GUIContent.none);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField(m_rangeMax.displayName, GUILayout.MaxWidth(30f));
                EditorGUILayout.PropertyField(m_rangeMax, GUIContent.none);
            }

            EditorGUILayout.PropertyField(m_increment);
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(IntSetting))]
    internal class IntSettingEditor : RangeSettingEditor
    {
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(FloatSetting))]
    internal class FloatSettingEditor : RangeSettingEditor
    {
    }
}
