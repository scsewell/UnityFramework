using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    /// <summary>
    /// An attribute tag applied to serialized fields that allows assigning a single physics layer.
    /// </summary>
    public class LayerAttribute : PropertyAttribute
    {
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            label = EditorGUI.BeginProperty(pos, label, prop);
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;

            EditorGUI.BeginChangeCheck();
            int layer = EditorGUI.LayerField(pos, label, prop.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                prop.intValue = layer;
            }

            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();
        }
    }
#endif
}
