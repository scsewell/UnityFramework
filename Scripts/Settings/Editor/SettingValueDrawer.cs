using System;

using UnityEngine;
using UnityEditor;

namespace Framework.Settings
{
    public static class SettingValueDrawer
    {
        public static void DrawSettingValueSelector(Rect rect, Setting setting, SerializedProperty value)
        {
            if (setting == null)
            {
                return;
            }

            EditorGUI.BeginProperty(rect, null, value);
            EditorGUI.showMixedValue = value.hasMultipleDifferentValues;

            EditorGUI.BeginChangeCheck();

            string oldValue = value.stringValue;
            string newValue = null;

            switch (setting)
            {
                case BoolSetting s:
                {
                    s.Deserialize(oldValue, out bool deserialized);
                    newValue = s.Serialize(s.Sanitize(EditorGUI.Toggle(rect, deserialized)));
                    break;
                }
                case IntSetting s:
                {
                    s.Deserialize(oldValue, out int deserialized);
                    newValue = s.Serialize(s.Sanitize(EditorGUI.IntSlider(rect, deserialized, s.Min, s.Max)));
                    break;
                }
                case FloatSetting s:
                {
                    s.Deserialize(oldValue, out float deserialized);
                    newValue = s.Serialize(s.Sanitize(EditorGUI.Slider(rect, deserialized, s.Min, s.Max)));
                    break;
                }
                case EnumSetting s:
                {
                    if (s.Type != null)
                    {
                        Array values = Enum.GetValues(s.Type);

                        s.Deserialize(oldValue, out Enum deserialized);
                        int oldSelected = Mathf.Max(0, Array.IndexOf(values, deserialized));
                        int newSelected = Mathf.Max(0, EditorGUI.Popup(rect, oldSelected, s.DisplayValues));
                        Enum selected = (Enum)values.GetValue(newSelected);

                        newValue = s.Serialize(s.Sanitize(selected));
                    }
                    else
                    {
                        EditorGUI.LabelField(rect, "Assign enum type");
                    }
                    break;
                }
                case StringSetting s:
                {
                    s.Deserialize(oldValue, out string deserialized);
                    newValue = s.Serialize(s.Sanitize(EditorGUI.TextField(rect, deserialized)));
                    break;
                }
                default:
                {
                    EditorGUI.LabelField(rect, "Not Configurable");
                    break;
                }
            }

            if (EditorGUI.EndChangeCheck() || oldValue == string.Empty)
            {
                value.stringValue = newValue;
            }

            EditorGUI.EndProperty();
        }
    }
}
