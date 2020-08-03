using System;
using System.Linq;

using Framework.EditorTools;

using UnityEditor;

using UnityEngine;

namespace Framework.AssetBundles
{
    [CustomPropertyDrawer(typeof(AssetBundleReference), true)]
    internal class AssetBundleReferenceEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var assetGuid = property.FindPropertyRelative("m_assetGuid");

            // get the type of reference
            var references = property.GetPropertyFields().Cast<AssetBundleReference>();
            var fieldType = references.FirstOrDefault().GetType();

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
            var oldGuid = assetGuid.stringValue;
            var oldPath = AssetDatabase.GUIDToAssetPath(oldGuid);
            var oldAsset = AssetDatabase.LoadAssetAtPath(oldPath, type);

            // draw an object field to select the asset
            using (var prop = new EditorGUI.PropertyScope(position, label, property))
            {
                position = EditorGUI.PrefixLabel(position, prop.content);

                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

                    var referenceRect = position;
                    referenceRect.xMax -= 20f;
                    var newAsset = EditorGUI.ObjectField(referenceRect, oldAsset, type, false);

                    if (change.changed)
                    {
                        var newPath = AssetDatabase.GetAssetPath(newAsset);
                        var newGuid = AssetDatabase.AssetPathToGUID(newPath);
                        assetGuid.stringValue = newGuid;
                    }
                }
            }

            // update the reference to make sure it is valid
            property.serializedObject.ApplyModifiedProperties();

            foreach (var reference in references)
            {
                reference.UpdateBundlePath();
            }

            property.serializedObject.Update();

            // indicate if the referenced object resides in an asset bundle
            if (!assetGuid.hasMultipleDifferentValues)
            {
                var indicatorRect = default(Rect);
                indicatorRect.xMin = position.xMax - 15f;
                indicatorRect.xMax = position.xMax - 5f;
                indicatorRect.yMin = position.yMin + 4f;
                indicatorRect.yMax = position.yMax - 4f;

                EditorGUI.DrawRect(indicatorRect, references.First().IsBundled ? Color.green : Color.red);
            }
        }
    }
}
