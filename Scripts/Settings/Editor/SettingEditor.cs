using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using Framework.EditorTools;

namespace Framework.Settings
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Setting), true)]
    public class SettingEditor : Editor
    {
        protected SerializedProperty m_category = null;
        protected SerializedProperty m_displayMode = null;
        protected SerializedProperty m_defaultValue = null;

        protected SerializedProperty m_range = null;
        protected SerializedProperty m_rangeMin = null;
        protected SerializedProperty m_rangeMax = null;
        protected SerializedProperty m_increment = null;

        protected SerializedProperty m_enumType = null;
        protected SerializedProperty m_values = null;

        protected virtual void OnEnable()
        {
            m_category = serializedObject.FindProperty("m_category");
            m_displayMode = serializedObject.FindProperty("m_displayMode");
            m_defaultValue = serializedObject.FindProperty("m_defaultValue");

            m_range = serializedObject.FindProperty("m_range");
            if (m_range != null)
            {
                m_rangeMin = m_range.FindPropertyRelative("m_min");
                m_rangeMax = m_range.FindPropertyRelative("m_max");
            }
            m_increment = serializedObject.FindProperty("m_increment");

            m_enumType = serializedObject.FindProperty("m_enumType");
            m_values = serializedObject.FindProperty("m_values");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_category);
            EditorGUILayout.PropertyField(m_displayMode);

            DrawRange();
            DrawEnum();

            serializedObject.ApplyModifiedProperties();

            // only allow selecting a default value if it will be valid
            if (target is Setting setting && !setting.IsRuntime)
            {
                Rect rect = EditorGUILayout.GetControlRect();
                rect = EditorGUI.PrefixLabel(rect, new GUIContent(m_defaultValue.displayName, m_defaultValue.tooltip));
                SettingValueDrawer.DrawSettingValueSelector(rect, setting, m_defaultValue);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRange()
        {
            if (m_range == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(new GUIContent(m_range.displayName));

            EditorGUILayout.LabelField(m_rangeMin.displayName, GUILayout.MaxWidth(30f));
            EditorGUILayout.PropertyField(m_rangeMin, new GUIContent());
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(m_rangeMax.displayName, GUILayout.MaxWidth(30f));
            EditorGUILayout.PropertyField(m_rangeMax, new GUIContent());

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(m_increment);
        }

        private void DrawEnum()
        {
            if (m_enumType == null)
            {
                return;
            }

            TypeSearchWindow.DrawDropdown(m_enumType, (t) =>
            {
                return t.IsEnum && !t.IsNestedPrivate;
            });

            // update the type mapping, but only if there is no difference among selected objects
            Type type = Type.GetType(m_enumType.stringValue);

            bool mixedValues = false;
            for (int i = 0; i < m_values.arraySize; i++)
            {
                SerializedProperty item = m_values.GetArrayElementAtIndex(i);
                SerializedProperty enumName = item.FindPropertyRelative("enumName");
                SerializedProperty displayName = item.FindPropertyRelative("displayName");

                if (enumName.hasMultipleDifferentValues || displayName.hasMultipleDifferentValues)
                {
                    mixedValues = true;
                    break;
                }
            }

            if (mixedValues)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Can't edit enum value mapping, the selected objects have different values.", MessageType.Warning);
                EditorGUILayout.Space();
            }
            else if (type != null)
            {
                // store the original value mapping in order to preserve values when the enum has changed
                Dictionary<string, string> mapping = new Dictionary<string, string>();

                for (int i = 0; i < m_values.arraySize; i++)
                {
                    SerializedProperty item = m_values.GetArrayElementAtIndex(i);
                    SerializedProperty enumName = item.FindPropertyRelative("enumName");
                    SerializedProperty displayName = item.FindPropertyRelative("displayName");

                    mapping[enumName.stringValue] = displayName.stringValue;
                }

                // update the mapping
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Mapping", EditorStyles.boldLabel);

                string[] values = Enum.GetNames(type);
                m_values.arraySize = values.Length;

                for (int i = 0; i < m_values.arraySize; i++)
                {
                    SerializedProperty item = m_values.GetArrayElementAtIndex(i);
                    SerializedProperty enumName = item.FindPropertyRelative("enumName");
                    SerializedProperty displayName = item.FindPropertyRelative("displayName");

                    enumName.stringValue = values[i];

                    // copy the display value last used for the given enum name
                    if (mapping.TryGetValue(enumName.stringValue, out string value))
                    {
                        displayName.stringValue = value;
                    }
                    else
                    {
                        displayName.stringValue = enumName.stringValue;
                    }

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PrefixLabel(enumName.stringValue);
                    displayName.stringValue = EditorGUILayout.TextField(displayName.stringValue);

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();
            }
        }
    }
}
