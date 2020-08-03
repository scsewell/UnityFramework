using System;
using System.Linq;

using Framework.EditorTools;

using UnityEditor;

using UnityEngine;

namespace Framework
{
    [CustomPropertyDrawer(typeof(UnityGuid))]
    internal class UnityGuidDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // get all the guid values
            var guids = property.GetPropertyFields();

            // display the guid value
            using (var prop = new EditorGUI.PropertyScope(position, label, property))
            {
                position = EditorGUI.PrefixLabel(position, prop.content);

                const float BUTTON_WIDTH = 50f;

                if (!property.hasMultipleDifferentValues)
                {
                    var guid = (UnityGuid)guids.FirstOrDefault();

                    var labelRect = position;
                    labelRect.xMax = position.xMax - BUTTON_WIDTH;

                    EditorGUI.LabelField(position, guid.ToString());

                    var buttonRect = position;
                    buttonRect.xMin = position.xMax - BUTTON_WIDTH;

                    // generate a new guid if the current value is not defined or if we want a new one
                    if (GUI.Button(buttonRect, "New") || guid == Guid.Empty)
                    {
                        var bytes = Guid.NewGuid().ToByteArray();

                        property.FindPropertyRelative("m_part0").longValue = BitConverter.ToInt64(bytes, 0);
                        property.FindPropertyRelative("m_part1").longValue = BitConverter.ToInt64(bytes, 8);
                    }
                }
            }
        }
    }
}
