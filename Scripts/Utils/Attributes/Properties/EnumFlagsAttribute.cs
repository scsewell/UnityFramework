using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    /// <summary>
    /// An attribute placed on serialized flags enum fields that allows the editing
    /// of an enum as a bit mask in the inspector.
    /// </summary>
    public class EnumFlagsAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    internal class EnumFlagsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            using (var property = new EditorGUI.PropertyScope(pos, label, prop))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;

                var value = Enum.ToObject(fieldInfo.FieldType, prop.intValue) as Enum;
                var flags = EditorGUI.EnumFlagsField(pos, property.content, value);
                
                if (change.changed)
                {
                    prop.intValue = (int)(object)flags;
                }
            }
        }
    }
#endif
}
