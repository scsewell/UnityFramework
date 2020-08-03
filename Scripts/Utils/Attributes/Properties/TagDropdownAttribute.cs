using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    /// <summary>
    /// An attribute placed on serialized string fields that allows assigning a tag
    /// in the inspector.
    /// </summary>
    public class TagDropdownAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TagDropdownAttribute))]
    internal class TagDropdownDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            using (var property = new EditorGUI.PropertyScope(pos, label, prop))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;

                var value = EditorGUI.TagField(pos, property.content, prop.stringValue);

                if (change.changed)
                {
                    prop.stringValue = value;
                }
            }
        }
    }
#endif
}
