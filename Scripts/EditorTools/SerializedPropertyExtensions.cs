using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Framework.EditorTools
{
    /// <summary>
    /// A class containing extention methods for <see cref="SerializedProperty"/>.
    /// </summary>
    public static class SerializedPropertyExtentions
    {
        /// <summary>
        /// Gets the field values backing a serialized property.
        /// </summary>
        /// <param name="property">A serialized property.</param>
        /// <returns>A new array containing the field values for all target instances.</returns>
        public static object[] GetPropertyFields(this SerializedProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            // Make sure the serialized object is up to date before we retrieve any values from the underlying objects
            var serializedObject = property.serializedObject;
            serializedObject.ApplyModifiedProperties();

            // convert the property path in to a more workable format
            var path = property.propertyPath.Replace(".Array.data", string.Empty);
            var elements = path.Split('.');

            // make sure to support multi-object editing by getting the instances for all selected objects
            return serializedObject.targetObjects
                .Select(o => GetFieldOfProperty(o, new Queue<string>(elements)))
                .ToArray();
        }

        private static object GetFieldOfProperty(object instance, Queue<string> path)
        {
            if (instance == null)
            {
                return null;
            }

            var objectType = instance.GetType();

            // get the next field along the path
            var element = path.Dequeue();

            // make sure to get the index if a collection field
            var index = -1;
            if (element.Contains("["))
            {
                var split = element.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

                element = split[0];
                index = Convert.ToInt32(split[1]);
            }

            // get the backing instance for the field on this object
            var fieldType = objectType.GetField(element, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldType == null)
            {
                return null;
            }

            var fieldInstance = fieldType.GetValue(instance);

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
            return path.Count == 0 ? fieldInstance : GetFieldOfProperty(fieldInstance, path);
        }
    }
}
