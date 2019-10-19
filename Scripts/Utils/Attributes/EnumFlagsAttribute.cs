using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    /// <summary>
    /// An attribute tag that allows the editing of an enum as a bit mask in the inspector.
    /// </summary>
    public class EnumFlagsAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            label = EditorGUI.BeginProperty(pos, label, prop);
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;

            EditorGUI.BeginChangeCheck();
            int flags = EditorGUI.MaskField(pos, label, prop.intValue, prop.enumNames);
            if (EditorGUI.EndChangeCheck())
            {
                prop.intValue = flags;
            }

            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();
        }
    }
#endif
}
