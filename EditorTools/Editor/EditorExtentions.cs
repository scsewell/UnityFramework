using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.EditorTools
{
    public static class EditorExtentions
    {
        public static void DrawList(SerializedProperty list)
        {
            if (!list.isArray)
            {
                EditorGUILayout.HelpBox(list.name + " is not an array!", MessageType.Error);
                return;
            }

            EditorGUILayout.PropertyField(list);
            EditorGUI.indentLevel++;

            if (list.isExpanded)
            {
                SerializedProperty size = list.FindPropertyRelative("Array.size");

                if (size.hasMultipleDifferentValues)
                {
                    EditorGUILayout.HelpBox("Cannot multi-edit lists with different sizes.", MessageType.Info);
                }
                else
                {
                    for (int i = 0; i < list.arraySize; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent());

                        GUILayoutOption miniButtonWidth = GUILayout.Width(20f);

                        if (GUILayout.Button(new GUIContent("\u2191", "Move Up"), EditorStyles.miniButtonLeft, miniButtonWidth))
                        {
                            list.MoveArrayElement(i, i - 1);
                            GUI.FocusControl("");
                        }
                        if (GUILayout.Button(new GUIContent("\u2193", "Move Down"), EditorStyles.miniButtonMid, miniButtonWidth))
                        {
                            list.MoveArrayElement(i, i + 1);
                            GUI.FocusControl("");
                        }
                        if (GUILayout.Button(new GUIContent("+", "Insert"), EditorStyles.miniButtonMid, miniButtonWidth))
                        {
                            list.InsertArrayElementAtIndex(i);
                            GUI.FocusControl("");
                        }
                        if (GUILayout.Button(new GUIContent("-", "Delete"), EditorStyles.miniButtonRight, miniButtonWidth))
                        {
                            int oldSize = list.arraySize;
                            list.DeleteArrayElementAtIndex(i);
                            if (list.arraySize == oldSize)
                            {
                                list.DeleteArrayElementAtIndex(i);
                            }
                            GUI.FocusControl("");
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (list.arraySize == 0 && GUILayout.Button(new GUIContent("+", "Add New"), EditorStyles.miniButton))
                    {
                        list.arraySize++;
                    }
                }
            }

            EditorGUI.indentLevel--;
        }
    }

    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
        }
    }
}
