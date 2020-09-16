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

        private readonly string m_filePath;

        private Task<AssetBundle> m_loadBundleOp = null;
        private string m_scene = null;
        private bool m_sceneHasBeenUnloaded = false;
        private List<WeakReference> m_references = null;

        /// <summary>
        /// Get if any assets from this bundle are currently in use. 
        /// </summary>
        public bool IsUsed
        {
            get
            {
                EnsureNotDisposed();

                // If the bundle is still loading, keep it alive since
                // nothing will have had the chance to use it yet.
                if (!m_loadBundleOp.IsCompleted)
                {
                    return true;
                }

                // Get the bundle. This will not block, since we know the task has finished
                var bundle = m_loadBundleOp.GetAwaiter().GetResult();

                if (bundle.isStreamedSceneAssetBundle)
                {
                    // this bundle is needed until its scene is no longer loaded
                    return !m_sceneHasBeenUnloaded;
                }
                else
                {
                    // this bundle is needed if any of its assets are referenced in code
                    foreach (var reference in m_references)
                    {
                        if (reference.IsAlive)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }

        /// <summary>
        /// Creates a new bundle data instance.
        /// </summary>
        /// <param name="bundleName">The name of the asset bundle.</param>
        /// <param name="filePath">The absolute file path of the asset bundle.</param>
        public BundleData(string bundleName, string filePath)
        {
            this.bundleName = bundleName;
            m_filePath = filePath;

            // start loading the bundle
            m_loadBundleOp = LoadBundleAsync();
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                // asset bundles should not be disposed until they have loaded to avoid blocking
                if (!m_loadBundleOp.IsCompleted)
                {
                    Debug.LogError($"Asset bundle \"{bundleName}\" was disposed before it finished loading!");
                }

                // Synchronously wait until the bundle has been loaded before unloading it to
                // ensure correct behaviour.
                var bundle = m_loadBundleOp.GetAwaiter().GetResult();

                // clean up event subscriptions
                if (bundle.isStreamedSceneAssetBundle)
                {
                    SceneManager.sceneUnloaded -= OnSceneUnloaded;
                }

                // unload the bundle and destroy all of the assets loaded from it
                bundle.Unload(true);

                Debug.Log($"Unloaded asset bundle \"{bundleName}\"");
            }
        }

        /// <summary>
        /// Loads the first asset of a given type from this bundle.
        /// </summary>
        /// <remarks>
        /// This is only valid for asset bundles containing asset data.
        /// </remarks>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <returns>The loaded asset, or null on failure.</returns>
        public async Task<T> LoadAssetAsync<T>() where T : Object
        {
            EnsureNotDisposed();

            var bundle = await m_loadBundleOp;

            // check if this bundle contains assets
            if (bundle.isStreamedSceneAssetBundle)
            {
                Debug.LogError($"Unable to load assets from asset bundle \"{bundleName}\", it contains scene data instead!");
                return null;
            }

            // Load the asset from the bundle. After testing the asset loading behaviour, 
            // repeated calls will reference the same managed object, as long as the bundle is
            // not unloaded.
            var asset = await bundle.LoadAllAssetsAsync<T>() as T;

            // Keep track that this asset is loaded. This is done using a weak reference to the 
            // asset object. We can check when the managed asset object is garbage collected, 
            // so we know when it is safe to unload the bundled asset.
            m_references.Add(new WeakReference(asset));

            return asset;
        }

        /// <summary>
        /// Loads an asset from this bundle.
        /// </summary>
        /// <remarks>
        /// This is only valid for asset bundles containing asset data.
        /// </remarks>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <param name="assetName">The name of the asset to load from the bundle.</param>
        /// <returns>The loaded asset, or null on failure.</returns>
        public async Task<T> LoadAssetAsync<T>(string assetName) where T : Object
        {
            EnsureNotDisposed();

            var bundle = await m_loadBundleOp;

            // check if this bundle contains assets
            if (bundle.isStreamedSceneAssetBundle)
            {
                Debug.LogError($"Unable to load assets from asset bundle \"{bundleName}\", it contains scene data instead!");
                return null;
            }

            // Load the asset from the bundle. After testing the asset loading behaviour, 
            // repeated calls will reference the same managed object, as long as the bundle is
            // not unloaded.
            var asset = await bundle.LoadAssetAsync<T>(assetName) as T;

            // Keep track that this asset is loaded. This is done using a weak reference to the 
            // asset object. We can check when the managed asset object is garbage collected, 
            // so we know when it is safe to unload the bundled asset.
            m_references.Add(new WeakReference(asset));

            return asset;
        }

        /// <summary>
        /// Gets the scene contained in this asset bundle.
        /// </summary>
        /// <remarks>
        /// This is only valid for asset bundles containing scene data.
        /// </remarks>
        /// <returns>The scene path to load, or null if there is no scene data.</returns>
        public async Task<string> GetSceneAsync()
        {
            EnsureNotDisposed();

            var bundle = await m_loadBundleOp;

            // check if this bundle contains scene contents
            if (!bundle.isStreamedSceneAssetBundle)
            {
                Debug.LogError($"Unable to load scene from asset bundle \"{bundleName}\", it contains asset data instead!");
                return null;
            }

            // make sure the bundle will not be unloaded until the next time this scene has been unloaded
            m_sceneHasBeenUnloaded = false;

            return m_scene;
        }

        /// <summary>
        /// Loads the asset bundle.
        /// </summary>
        private async Task<AssetBundle> LoadBundleAsync()
        {
            // load the asset bundle
            var bundle = await AssetBundle.LoadFromFileAsync(m_filePath);

            // prepare to track use of the bundle
            if (bundle.isStreamedSceneAssetBundle)
            {
                m_scene = bundle.GetAllScenePaths()[0];
                m_sceneHasBeenUnloaded = false;

                SceneManager.sceneUnloaded += OnSceneUnloaded;
            }
            else
            {
                m_references = new List<WeakReference>();
            }

            Debug.Log($"Loaded asset bundle \"{bundleName}\"");

            return bundle;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (scene.path == m_scene)
            {
                m_sceneHasBeenUnloaded = true;
            }
        }

        public override string ToString() => bundleName;
    }
}
