using System;
using System.Linq;

using UnityEngine;
using UnityEditor;

using Framework.EditorTools;

namespace Framework.AssetBundles
{
    [CustomPropertyDrawer(typeof(AssetBundleReference), true)]
    public class AssetBundleReferenceEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty assetGuid = property.FindPropertyRelative("m_assetGuid");

            // get the type of reference
            object[] references = SerializedObjectUtils.GetPropertyField(property);

            Type fieldType = references.FirstOrDefault().GetType();

            Type type;
            if (fieldType.IsAssignableFrom(typeof(AssetBundleSceneReference)))
            {
                // for scenes the type is always a scene asset
                type = typeof(SceneAsset);
            }
            else
            {
                // we need to get the type parameter giving the asset type
                type = fieldType.BaseType.GetGenericArguments().FirstOrDefault();
            }

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
                (reference as AssetBundleReference).UpdateBundlePath();
            }

            property.serializedObject.Update();

            // indicate if the reference is bundled
            if (!assetGuid.hasMultipleDifferentValues)
            {
                bool isBundled = (references.First() as AssetBundleReference).IsBundled;

                Rect indicatorRect = default;
                indicatorRect.xMin = position.xMax - 15f;
                indicatorRect.xMax = position.xMax - 5f;
                indicatorRect.yMin = position.yMin + 4f;
                indicatorRect.yMax = position.yMax - 4f;

                EditorGUI.DrawRect(indicatorRect, isBundled ? Color.green : Color.red);
            }
        }
    }
}
