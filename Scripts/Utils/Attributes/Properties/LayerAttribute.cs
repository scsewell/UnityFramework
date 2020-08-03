using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    /// <summary>
    /// An attribute placed on serialized integer fields that allows assigning a physics layer.
    /// </summary>
    public class LayerAttribute : PropertyAttribute
    {
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    internal class LayerPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            using (var property = new EditorGUI.PropertyScope(pos, label, prop))
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;

                var layer = EditorGUI.LayerField(pos, property.content, prop.intValue);

                if (change.changed)
                {
                    prop.intValue = layer;
                }
            }
        }
    }
#endif
}
