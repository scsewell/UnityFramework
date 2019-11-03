using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

namespace Framework.AssetBundles
{
    using Object = UnityEngine.Object;

    /// <summary>
    /// Keeps track of information about an asset bundle.
    /// </summary>
    internal class BundleData
    {
        /// <summary>
        /// The name of the asset bundle.
        /// </summary>
        public readonly string bundleName;

        private readonly AssetBundle m_bundle;
        private readonly HashSet<string> m_loadedAssets = new HashSet<string>();
        private readonly List<WeakReference> m_references = new List<WeakReference>();
        
        /// <summary>
        /// Get if any assets from this bundle are currently in use. 
        /// </summary>
        public bool IsUsed
        {
            get
            {
                foreach (WeakReference reference in m_references)
                {
                    if (reference.IsAlive)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Creates a new bundle data instance.
        /// </summary>
        /// <param name="bundleName">The name of the asset bundle.</param>
        /// <param name="bundle">The asset bundle.</param>
        public BundleData(string bundleName, AssetBundle bundle)
        {
            this.bundleName = bundleName;
            m_bundle = bundle;

            Debug.Log($"Loaded asset bundle \"{bundleName}\"");
        }

        public async Task<T> LoadAssetAsync<T>(string assetName) where T : Object
        {
            // Load the asset from the bundle. After testing the asset loading behaviour, 
            // repeated calls will reference the same managed object, as long as the bundle is
            // not unloaded.
            T asset = await m_bundle.LoadAssetAsync<T>(assetName) as T;

            // Keep track that this asset is loaded. This is done using a weak reference to the 
            // asset object. We can check when the managed asset object is garbage collected, 
            // so we know when it is safe to unload the bundled asset.
            if (!m_loadedAssets.Contains(assetName))
            {
                m_references.Add(new WeakReference(asset));
                m_loadedAssets.Add(assetName);
            }

            return asset;
        }

        /// <summary>
        /// Unloads this asset bundle.
        /// </summary>
        public void Unload()
        {
            m_bundle.Unload(true);

            Debug.Log($"Unloaded bundle \"{bundleName}\"");
        }

        public override string ToString() => bundleName;
    }
}
