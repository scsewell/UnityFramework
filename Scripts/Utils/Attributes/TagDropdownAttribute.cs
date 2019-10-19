using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    /// <summary>
    /// An attribute tag applied to serialized string fields that allows assigning a tag.
    /// </summary>
    public class TagDropdownAttribute : PropertyAttribute
    {
        public readonly bool useDefaultTagFieldDrawer = false;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TagDropdownAttribute))]
    public class TagSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginProperty(position, label, property);

                var attrib = attribute as TagDropdownAttribute;

                if (attrib.useDefaultTagFieldDrawer)
                {
                    property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
                }
                else
                {
                    List<string> tagList = new List<string>();

                    tagList.Add("<NoTag>");
                    tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);

                    string propertyString = property.stringValue;
                    int index = -1;

                    if (propertyString == string.Empty)
                    {
                        index = 0;
                    }
                    else
                    {
                        for (int i = 1; i < tagList.Count; i++)
                        {
                            if (tagList[i] == propertyString)
                            {
                                index = i;
                                break;
                            }
                        }
                    }

                    index = EditorGUI.Popup(position, label.text, index, tagList.ToArray());

                    if (index == 0)
                    {
                        property.stringValue = string.Empty;
                    }
                    else if (index >= 1)
                    {
                        property.stringValue = tagList[index];
                    }
                    else
                    {
                        property.stringValue = string.Empty;
                    }
                }

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
#endif
}
