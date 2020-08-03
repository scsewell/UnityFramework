using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    /// <summary>
    /// An attribute placed on serialized boolean fields that moves the check box
    /// to the left of the property name.
    /// </summary>
    public class LeftToggleAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LeftToggleAttribute))]
    internal class LeftToggleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            using (var property = new EditorGUI.PropertyScope(pos, label, prop))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;

                var value = EditorGUI.ToggleLeft(pos, property.content, prop.boolValue);

                if (change.changed)
                {
                    prop.boolValue = value;
                }
            }
        }
    }
#endif
}
