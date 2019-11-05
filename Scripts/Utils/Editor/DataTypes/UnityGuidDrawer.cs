using System;
using System.Linq;

using UnityEngine;
using UnityEditor;

using Framework.EditorTools;

namespace Framework
{
    [CustomPropertyDrawer(typeof(UnityGuid))]
    public class UnityGuidDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // get all the guid values
            object[] guids = SerializedObjectUtils.GetPropertyField(property);

            // display the guid value
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            const float BUTTON_WIDTH = 50f;

            if (!property.hasMultipleDifferentValues)
            {
                UnityGuid guid = (UnityGuid)guids.FirstOrDefault();

                Rect labelRect = position;
                labelRect.xMax = position.xMax - BUTTON_WIDTH;

                EditorGUI.LabelField(position, guid.ToString());

                Rect buttonRect = position;
                buttonRect.xMin = position.xMax - BUTTON_WIDTH;
                
                // generate a new guid if the current value is not defined or if we want a new one
                if (GUI.Button(buttonRect, "New") || guid == Guid.Empty)
                {
                    byte[] bytes = Guid.NewGuid().ToByteArray();

                    property.FindPropertyRelative("m_part0").longValue = BitConverter.ToInt64(bytes, 0);
                    property.FindPropertyRelative("m_part1").longValue = BitConverter.ToInt64(bytes, 8);
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
