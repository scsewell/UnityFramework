using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

using UnityEditor;

namespace Framework.EditorTools
{
    public static class SerializedObjectUtils
    {
        /// <summary>
        /// Gets the field values backing a serialized property.
        /// </summary>
        /// <param name="property">A serialized property.</param>
        /// <returns>A new array containing all the field instances.</returns>
        public static object[] GetPropertyField(SerializedProperty property)
        {
            if (property == null)
            {
                return null;
            }

            // Make sure the serlized object is up to date before we retrieve any values from the underlying objects
            SerializedObject serializedObject = property.serializedObject;
            serializedObject.ApplyModifiedProperties();

            // converty the property path in to a more workable format
            string path = property.propertyPath.Replace(".Array.data", string.Empty);
            string[] elements = path.Split('.');

            // make sure to support multi-object editing by getting the instances for all selected objects
            List<object> instances = new List<object>();

            foreach (var o in serializedObject.targetObjects)
            {
                instances.Add(GetFieldOfProperty(o, new Queue<string>(elements)));
            }

            return instances.ToArray();
        }

        private static object GetFieldOfProperty(object instance, Queue<string> path)
        {
            if (instance == null)
            {
                return null;
            }

            Type objectType = instance.GetType();

            if (objectType == null)
            {
                return null;
            }

            // get the next field along the path
            string element = path.Dequeue();

            // make sure to get the index if a collection field
            int index = -1;
            if (element.Contains("["))
            {
                string[] split = element.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

                element = split[0];
                index = Convert.ToInt32(split[1]);
            }

            // get the backing instance for the field on this object
            FieldInfo fieldType = objectType.GetField(element, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldType == null)
            {
                return null;
            }

            object fieldInstance = fieldType.GetValue(instance);

            // Check if this is a collection field. If so, we need to get the element at the correct index.
            if (index >= 0)
            {
                switch (fieldInstance)
                {
                    case object[] array:
                        fieldInstance = array[index];
                        break;
                    case IList<object> list:
                        fieldInstance = list[index];
                        break;
                    default:
                        Debug.LogError($"Unsupported field type \"{fieldType.FieldType}\" for property with name \"{fieldType.Name}\"!");
                        break;
                }
            }

            // return this instance if at the end of the path, otherwise keep going
            if (path.Count == 0)
            {
                return fieldInstance;
            }
            else
            {
                return GetFieldOfProperty(fieldInstance, path);
            }
        }
    }
}
