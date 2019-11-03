using System;
using System.Threading.Tasks;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.AssetBundles
{
    [Serializable]
    public class AssetBundleSceneReference : AssetBundleReference
    {
        [SerializeField]
        private string m_scenePath = null;

#if UNITY_EDITOR
        /// <summary>
        /// Sets the reference from the currently assigned asset guid.
        /// </summary>
        /// <returns>False if no asset with the given GUID exists.</returns>
        public override bool UpdateBundlePath()
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(m_assetGuid);

            // Clear the references if the Guid is invalid
            if (string.IsNullOrEmpty(assetPath))
            {
                m_bundleName = null;
                m_scenePath = null;
                return false;
            }

            // Check if the asset in in an asset bundle. If in a bundle, store
            // the bundle name. Otherwise, store the scene path as usual.
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            if (importer == null)
            {
                return false;
            }

            bool isInBundle = !string.IsNullOrEmpty(importer.assetBundleName);

            if (isInBundle)
            {
                m_bundleName = importer.assetBundleName;
                m_scenePath = null;
            }
            else
            {
                m_bundleName = null;
                m_scenePath = assetPath;
            }

            return true;
        }
#endif

        /// <summary>
        /// Gets the referenced scene path.
        /// </summary>
        public async Task<string> GetAsync()
        {
            return await AssetBundleManager.LoadSceneAsync(m_bundleName);
        }
    }
}
