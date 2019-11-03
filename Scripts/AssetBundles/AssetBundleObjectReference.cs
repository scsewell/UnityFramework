using System;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;

using UnityEditor;

namespace Framework.AssetBundles
{
    public abstract class AssetBundleObjectReference
    {
        [SerializeField]
        protected string m_bundleName = null;

        [SerializeField]
        protected string m_assetName = null;

        [SerializeField]
        protected string m_assetGuid = null;

        /// <summary>
        /// Gets if the referenced asset resides in asset bundle.
        /// </summary>
        public bool IsBundled => !string.IsNullOrEmpty(m_bundleName);

#if UNITY_EDITOR
        /// <summary>
        /// Sets the reference from the currently assigned asset guid.
        /// </summary>
        /// <returns>False if no asset with the given GUID exists.</returns>
        public abstract bool UpdateBundlePath();
#endif
    }

    /// <summary>
    /// A reference which supports referecing assets in an asset bundle.
    /// If the asset is in a bundle, the bundle will be loaded on demand.
    /// </summary>
    /// <typeparam name="T">The type of object to reference.</typeparam>
    public abstract class AssetBundleObjectReference<T> : AssetBundleObjectReference where T : UnityEngine.Object
    {
        [SerializeField]
        protected T m_asset = null;

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
                m_assetName = null;
                m_asset = null;
                return false;
            }

            // Check if the asset in in an asset bundle. If in a bundle, store
            // the path to the asset. Otherwise, store the asset reference as 
            // usual.
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);

            if (importer == null)
            {
                return false;
            }

            bool isInBundle = !string.IsNullOrEmpty(importer.assetBundleName);

            if (isInBundle)
            {
                m_bundleName = importer.assetBundleName;
                m_assetName = Path.GetFileNameWithoutExtension(assetPath);
                m_asset = null;
            }
            else
            {
                m_bundleName = null;
                m_assetName = null;
                m_asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }

            return true;
        }
#endif

        /// <summary>
        /// Gets the referenced asset.
        /// </summary>
        public async Task<T> GetAsync()
        {
            // We do not cache the reference to as that will prevent the managed 
            // asset object from being garbage collected, which the bundle manager
            // needs to detect when it is safe to unload the bundle.
            return await AssetBundleManager.LoadAssetAsync<T>(m_bundleName, m_assetName);
        }
    }
}
