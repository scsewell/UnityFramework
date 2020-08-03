using System;
using System.Collections.Generic;

using Framework.EditorTools;

using UnityEditor;

namespace Framework.Settings
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(EnumSetting))]
    internal class EnumSettingEditor : SettingEditor
    {
        protected SerializedProperty m_enumType = null;
        protected SerializedProperty m_values = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_enumType = serializedObject.FindProperty("m_enumType");
            m_values = serializedObject.FindProperty("m_values");
        }

        protected override void CustomGUI()
        {
            TypeSearchWindow.DrawDropdown(m_enumType, (t) =>
            {
                return t.IsEnum && !t.IsNestedPrivate;
            });

            // allow editing the type mapping if there are no differences among selected setting objects
            var type = Type.GetType(m_enumType.stringValue);

            var mixedValues = false;
            for (var i = 0; i < m_values.arraySize; i++)
            {
                var item = m_values.GetArrayElementAtIndex(i);
                var enumName = item.FindPropertyRelative("enumName");
                var displayName = item.FindPropertyRelative("displayName");

                if (enumName.hasMultipleDifferentValues || displayName.hasMultipleDifferentValues)
                {
                    mixedValues = true;
                    break;
                }
            }

            if (mixedValues)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Can't edit enum value mapping, the selected objects have different enum types.", MessageType.Warning);
                EditorGUILayout.Space();
            }
            else if (type != null)
            {
                // store the original value mapping in order to preserve values when the enum has changed
                var mapping = new Dictionary<string, string>();

                for (var i = 0; i < m_values.arraySize; i++)
                {
                    var item = m_values.GetArrayElementAtIndex(i);
                    var enumName = item.FindPropertyRelative("enumName");
                    var displayName = item.FindPropertyRelative("displayName");

                    mapping[enumName.stringValue] = displayName.stringValue;
                }

                // update the mapping
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Mapping", EditorStyles.boldLabel);

                var values = Enum.GetNames(type);
                m_values.arraySize = values.Length;

                for (var i = 0; i < m_values.arraySize; i++)
                {
                    var item = m_values.GetArrayElementAtIndex(i);
                    var enumName = item.FindPropertyRelative("enumName");
                    var displayName = item.FindPropertyRelative("displayName");

                    enumName.stringValue = values[i];

                    // copy the display value last used for the given enum name
                    if (mapping.TryGetValue(enumName.stringValue, out var value))
                    {
                        displayName.stringValue = value;
                    }
                    else
                    {
                        displayName.stringValue = enumName.stringValue;
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel(enumName.stringValue);
                        displayName.stringValue = EditorGUILayout.TextField(displayName.stringValue);
                    }
                }

                EditorGUILayout.Space();
            }
        }
    }
}
