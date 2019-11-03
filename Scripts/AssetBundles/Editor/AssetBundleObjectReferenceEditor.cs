﻿using System;
using System.Linq;

using UnityEngine;
using UnityEditor;

using Framework.EditorTools;

namespace Framework.AssetBundles
{
    [CustomPropertyDrawer(typeof(AssetBundleObjectReference), true)]
    public class AssetBundleObjectReferenceEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty assetGuid = property.FindPropertyRelative("m_assetGuid");

            // get the type of reference
            object[] references = SerializedObjectUtils.GetPropertyField(property);
            Type type = references.FirstOrDefault().GetType().BaseType.GetGenericArguments().FirstOrDefault();

            // get the current reference value
            string oldGuid = assetGuid.stringValue;
            string oldPath = AssetDatabase.GUIDToAssetPath(oldGuid);
            UnityEngine.Object oldAsset = AssetDatabase.LoadAssetAtPath(oldPath, type);

            // draw an object field to select the asset
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            Rect referenceRect = position;
            referenceRect.xMax -= 20f;
            UnityEngine.Object newAsset = EditorGUI.ObjectField(referenceRect, oldAsset, type, false);

            if (EditorGUI.EndChangeCheck())
            {
                string newPath = AssetDatabase.GetAssetPath(newAsset);
                string newGuid = AssetDatabase.AssetPathToGUID(newPath);
                assetGuid.stringValue = newGuid;
            }

            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();

            // update the reference to make sure it is valid
            property.serializedObject.ApplyModifiedProperties();

            foreach (object reference in references)
            {
                (reference as AssetBundleObjectReference).UpdateBundlePath();
            }

            // indicate if the reference is bundled
            if (!assetGuid.hasMultipleDifferentValues)
            {
                Rect indicatorRect = default;
                indicatorRect.xMin = position.xMax - 15f;
                indicatorRect.xMax = position.xMax - 5f;
                indicatorRect.yMin = position.yMin + 4f;
                indicatorRect.yMax = position.yMax - 4f;

                bool isBundled = (references.First() as AssetBundleObjectReference).IsBundled;
                EditorGUI.DrawRect(indicatorRect, isBundled ? Color.green : Color.red);
            }

            property.serializedObject.Update();
        }
    }
}
