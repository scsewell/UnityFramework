using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.AssetBundles
{
    using Object = UnityEngine.Object;

    /// <summary>
    /// Keeps track of information about an asset bundle.
    /// </summary>
    internal class BundleData : Disposable
    {
        /// <summary>
        /// The name of the asset bundle.
        /// </summary>
        public readonly string bundleName;

        private readonly AssetBundle m_bundle;

        private readonly string m_scene;
        private bool m_sceneHasBeenUnloaded;

        private readonly HashSet<string> m_loadedAssets;
        private readonly List<WeakReference> m_references;
        
        /// <summary>
        /// Get if any assets from this bundle are currently in use. 
        /// </summary>
        public bool IsUsed
        {
            get
            {
                EnsureNotDisposed();

                if (m_bundle.isStreamedSceneAssetBundle)
                {
                    // this bundle is needed until its scene is no longer loaded
                    if (!m_sceneHasBeenUnloaded)
                    {
                        return true;
                    }
                }
                else
                {
                    // this bundle is needed if any of its assets are referenced in code
                    foreach (WeakReference reference in m_references)
                    {
                        if (reference.IsAlive)
                        {
                            return true;
                        }
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

            if (m_bundle.isStreamedSceneAssetBundle)
            {
                m_scene = m_bundle.GetAllScenePaths()[0];
                m_sceneHasBeenUnloaded = false;

                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
            else
            {
                m_loadedAssets = new HashSet<string>();
                m_references = new List<WeakReference>();
            }

            Debug.Log($"Loaded asset bundle \"{bundleName}\"");
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                if (m_bundle.isStreamedSceneAssetBundle)
                {
                    SceneManager.sceneUnloaded -= OnSceneUnloaded;
                }

                m_bundle.Unload(true);

                Debug.Log($"Unloaded asset bundle \"{bundleName}\"");
            }
        }

        /// <summary>
        /// Loads an asset from this bundle, if it contains asset data.
        /// </summary>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <param name="assetName">The name of the asset to load from the bundle.</param>
        /// <returns>The loaded asset.</returns>
        public async Task<T> LoadAssetAsync<T>(string assetName) where T : Object
        {
            EnsureNotDisposed();

            // check if this bundle contains assets
            if (m_bundle.isStreamedSceneAssetBundle)
            {
                Debug.LogWarning($"Unable to load assets from asset bundle \"{bundleName}\", it contains scene data instead!");
                return null;
            }

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
        /// Gets the scene contained in this asset bundle, if it contains scene data.
        /// </summary>
        /// <returns>The scene path to load, or null if there is no scene data.</returns>
        public string GetScene()
        {
            EnsureNotDisposed();

            // check if this bundle contains scene contents
            if (!m_bundle.isStreamedSceneAssetBundle)
            {
                Debug.LogWarning($"Unable to load scene from asset bundle \"{bundleName}\", it contains asset data instead!");
                return null;
            }

            // make sure the bundle will not be unloaded until the next time this scene has been unloaded
            m_sceneHasBeenUnloaded = false;

            return m_scene;
        }

        public override string ToString() => bundleName;

        private void OnSceneUnloaded(Scene scene)
        {
            if (scene.path == m_scene)
            {
                m_sceneHasBeenUnloaded = true;
            }
        }
    }
}
