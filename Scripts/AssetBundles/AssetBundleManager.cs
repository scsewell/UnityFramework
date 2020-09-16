using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

namespace Framework.AssetBundles
{
    using Object = UnityEngine.Object;

    /// <summary>
    /// The class responsible for managing asset bundles.
    /// </summary>
    public static class AssetBundleManager
    {
        /// <summary>
        /// The absolute path of the directory where asset bundles shipped with the game are located.
        /// This directory is automatically registered to the bundle manager with priority 0.
        /// </summary>
        public static string MainBundlePath
        {
            get
            {
                if (Application.isEditor)
                {
                    // put the bundles folder besides the Assets folder in the project
                    return $"{Application.dataPath}/../Bundles/";
                }
                else
                {
                    return $"{Application.dataPath}/Bundles/";
                }
            }
        }

        /// <summary>
        /// The absolute path of the directory of override or additional asset bundles are located.
        /// This directory is automatically registered to the bundle manager with priority 100, ensuring
        /// bundles located here will replace matching bundles in the main path. This directory is
        /// also guarenteed to be writeable, so it is safe to attempt to save mods to this directory.
        /// </summary>
        public static string ModBundlePath => $"{Application.persistentDataPath}/Bundles/";


        private static readonly List<BundleDirectory> s_bundlesPaths = new List<BundleDirectory>();
        private static readonly List<BundleData> s_loadedBundles = new List<BundleData>();
        private static readonly List<BundleData> s_bundlesToRemove = new List<BundleData>();
        private static readonly Dictionary<string, BundleData> s_nameToBundle = new Dictionary<string, BundleData>();
        private static bool s_autoUnloadBundles;

        /// <summary>
        /// Will the bundle manager automatically decide when to check for unused bundles.
        /// </summary>
        public static bool AutoUnloadBundles
        {
            get => s_autoUnloadBundles;
            set
            {
                if (s_autoUnloadBundles != value)
                {
                    s_autoUnloadBundles = value;

                    if (s_autoUnloadBundles)
                    {
                        UnloadUsedBundlesAsync();
                    }
                }
            }
        }

        /// <summary>
        /// The time in seconds between checks for unused bundles.
        /// </summary>
        public static float AutoUnloadPeriod { get; set; }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            s_bundlesPaths.Clear();
            s_loadedBundles.Clear();
            s_bundlesToRemove.Clear();
            s_nameToBundle.Clear();

            s_autoUnloadBundles = true;
            AutoUnloadPeriod = 5.0f;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitEarly()
        {
            // Automatically register the default bundle paths prior to loading the game
            RegisterBundleDirectory(MainBundlePath, 0);
            RegisterBundleDirectory(ModBundlePath, 100);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitLate()
        {
            // Periodically check if there are any bundles to unload
            if (AutoUnloadBundles)
            {
                UnloadUsedBundlesAsync();
            }

            // close all asset bundles on quit
            Application.quitting += () =>
            {
                foreach (var bundle in s_loadedBundles)
                {
                    bundle.Dispose();
                }

                s_loadedBundles.Clear();
                s_nameToBundle.Clear();
            };
        }

        private static async void UnloadUsedBundlesAsync()
        {
            while (AutoUnloadBundles)
            {
                await new WaitForSeconds(AutoUnloadPeriod);
                UnloadUnusedBundles();
            }
        }

        /// <summary>
        /// Unload bundles for which all the assets are not used anymore.
        /// </summary>
        public static void UnloadUnusedBundles()
        {
            // check which bundles are not needed
            foreach (var bundle in s_loadedBundles)
            {
                if (!bundle.IsUsed)
                {
                    s_bundlesToRemove.Add(bundle);
                }
            }

            // unload the unused bundles
            foreach (var bundle in s_bundlesToRemove)
            {
                s_loadedBundles.Remove(bundle);
                s_nameToBundle.Remove(bundle.bundleName);

                bundle.Dispose();
            }

            s_bundlesToRemove.Clear();
        }

        /// <summary>
        /// Adds a path to the directories in which asset bundles may be loaded from.
        /// </summary>
        /// <param name="path">The directory to add. It will be created if it does not already exist.</param>
        /// <param name="priority">If asset bundles of the same name are found under multiple directories,
        /// the bundle from the directory with the highest priority will be loaded.</param>
        /// <returns>True if a new bundle directory was successfully added.</returns>
        public static bool RegisterBundleDirectory(string path, int priority)
        {
            // ensure the given directory exists
            DirectoryInfo directory = null;

            try
            {
                directory = new DirectoryInfo(path);

                if (!directory.Exists)
                {
                    directory.Create();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to register asset bundle directory \"{path}\"! {e}");
                return false;
            }

            // only register the path if it is not already included
            if (s_bundlesPaths.Any(x => x.path == directory.FullName))
            {
                return false;
            }

            // register the directory and sort the directories by decending priority
            var bundleDir = new BundleDirectory(directory.FullName, priority);

            s_bundlesPaths.Add(bundleDir);
            s_bundlesPaths.Sort((x, y) => -x.priority.CompareTo(y.priority));

            Debug.Log($"Registered bundle directory \"{bundleDir.path}\" with priority {bundleDir.priority}.");
            return true;
        }

        /// <summary>
        /// Loads the first asset of a given type from all asset bundles at a given path.
        /// </summary>
        /// <typeparam name="T">The type of asset to load from the bundles.</typeparam>
        /// <param name="bundleDir">The path to the directory to load the bundles from relative to the
        /// registered bundle directories.</param>
        /// <returns>A new array contaning the loaded assets, or an empty array if the bundled assets
        /// could not be found.</returns>
        public static async Task<T[]> LoadAssetsAsync<T>(string bundleDir) where T : Object
        {
            return await LoadMany(bundleDir, (bundleName) =>
            {
                return LoadAssetAsync<T>(bundleName);
            });
        }

        /// <summary>
        /// Loads a type of asset by name from all asset bundles at a given path.
        /// </summary>
        /// <typeparam name="T">The type of asset to load from the bundles.</typeparam>
        /// <param name="bundleDir">The path to the directory to load the bundles from relative to the
        /// registered bundle directories.</param>
        /// <param name="assetName">The name of the asset to load from the bundles.</param>
        /// <returns>A new array contaning the loaded assets, or an empty array if the bundled assets
        /// could not be found.</returns>
        public static async Task<T[]> LoadAssetsAsync<T>(string bundleDir, string assetName) where T : Object
        {
            return await LoadMany(bundleDir, (bundleName) =>
            {
                return LoadAssetAsync<T>(bundleName, assetName);
            });
        }

        /// <summary>
        /// Loads the first asset of the given type from an asset bundle at a given path.
        /// </summary>
        /// <typeparam name="T">The type of asset to load from the bundle.</typeparam>
        /// <param name="bundleName">The full name of the asset bundle to load the asset from.</param>
        /// <returns>The loaded asset, or null if the bundled asset could not be found.</returns>
        public static async Task<T> LoadAssetAsync<T>(string bundleName) where T : Object
        {
            // loading should only be done by the main thread
            if (!ContextUtils.EnsureMainThread())
            {
                return null;
            }

            if (!TryLoadBundle(bundleName, out var bundle))
            {
                return null;
            }

            return await bundle.LoadAssetAsync<T>();
        }

        /// <summary>
        /// Loads a type of asset by name from an asset bundle at a given path.
        /// </summary>
        /// <typeparam name="T">The type of asset to load from the bundle.</typeparam>
        /// <param name="bundleName">The full name of the asset bundle to load the asset from.</param>
        /// <param name="assetName">The name of the asset to load from the bundle.</param>
        /// <returns>The loaded asset, or null if the bundled asset could not be found.</returns>
        public static async Task<T> LoadAssetAsync<T>(string bundleName, string assetName) where T : Object
        {
            // loading should only be done by the main thread
            if (!ContextUtils.EnsureMainThread())
            {
                return null;
            }

            if (!TryLoadBundle(bundleName, out var bundle))
            {
                return null;
            }

            return await bundle.LoadAssetAsync<T>(assetName);
        }

        /// <summary>
        /// Loads a scene asset bundle at a given path.
        /// </summary>
        /// <param name="bundleName">The full name of the asset bundle to load the scene from.</param>
        /// <returns>The path of the scene which can now be loaded, or null if no scene was found.</returns>
        public static async Task<string> LoadSceneAsync(string bundleName)
        {
            // loading should only be done by the main thread
            if (!ContextUtils.EnsureMainThread())
            {
                return null;
            }

            if (!TryLoadBundle(bundleName, out var bundle))
            {
                return null;
            }

            return await bundle.GetSceneAsync();
        }

        /// <summary>
        /// Loads an asset from all asset bundles at a given path.
        /// </summary>
        /// <typeparam name="T">The type of asset to load from the bundles.</typeparam>
        /// <param name="bundleDir">The path to the directory to load the bundles from relative to the
        /// registered bundle directories.</param>
        /// <param name="getAssetLoadOp">The function determining which asset to get from a bundle.</param>
        /// <returns>A new array contaning the loaded assets, or an empty array if the bundled assets
        /// could not be found.</returns>
        private static async Task<T[]> LoadMany<T>(string bundleDir, Func<string, Task<T>> getAssetLoadOp) where T : Object
        {
            // loading should only be done by the main thread
            if (!ContextUtils.EnsureMainThread())
            {
                return new T[0];
            }

            // find the first available bundle for each bundle name
            var bundles = new HashSet<string>();

            foreach (var directory in s_bundlesPaths)
            {
                if (TryGetBundleDirectory(Path.Combine(directory.path, bundleDir), out var dirInfo))
                {
                    foreach (var file in dirInfo.EnumerateFiles())
                    {
                        // asset bundle files should have no extention
                        if (string.IsNullOrEmpty(file.Extension))
                        {
                            // we need the path relative to the bundles folder
                            bundles.Add(file.FullName.Substring(directory.path.Length));
                        }
                    }
                }
            }

            // check if any bundles were found
            if (bundles.Count <= 0)
            {
                Debug.LogWarning($"Unable to find asset bundles at path \"{bundleDir}\" relative to registered bundle directories!");
                return new T[0];
            }

            // start loading the assets
            var assetLoadOps = new List<Task<T>>();

            foreach (var bundle in bundles)
            {
                assetLoadOps.Add(getAssetLoadOp(bundle));
            }

            // wait for all loading operations to complete and return the result
            await Task.WhenAll(assetLoadOps);

            return assetLoadOps.Select(t => t.Result).ToArray();
        }

        /// <summary>
        /// Loads an asset bundle.
        /// </summary>
        /// <param name="bundleName">The full name of the asset bundle to load the asset from.</param>
        /// <param name="bundle">Returns the asset bundle, or null if no matching bundle could be loaded.</param>
        /// <returns>True if the bundle was successfully loaded..</returns>
        private static bool TryLoadBundle(string bundleName, out BundleData bundle)
        {
            var canonicalBundleName = bundleName.Replace("\\", "/");

            // check if this bundle is already loaded
            if (s_nameToBundle.TryGetValue(canonicalBundleName, out bundle))
            {
                return true;
            }

            // find the first bundle with a matching name
            FileInfo file = null;

            foreach (var directory in s_bundlesPaths)
            {
                if (TryGetBundleFile(Path.Combine(directory.path, canonicalBundleName), out file))
                {
                    break;
                }
            }

            // check if a bundle was found
            if (file == null)
            {
                Debug.LogWarning($"Unable to find asset bundle with name \"{canonicalBundleName}\"!");
                bundle = null;
                return false;
            }

            // create the bundle information
            bundle = new BundleData(canonicalBundleName, file.FullName);

            s_loadedBundles.Add(bundle);
            s_nameToBundle.Add(canonicalBundleName, bundle);

            return true;
        }

        /// <summary>
        /// Checks if a path contains a valid bundle directory.
        /// </summary>
        /// <param name="path">The absolute path to a bundle directory.</param>
        /// <returns>True if the directory path is valid.</returns>
        private static bool TryGetBundleDirectory(string path, out DirectoryInfo directory)
        {
            // check the directory path is valid and can be accesed
            try
            {
                directory = new DirectoryInfo(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Asset bundle directory \"{path}\" cannot be accessed! {e}");
                directory = null;
                return false;
            }

            return directory.Exists;
        }

        /// <summary>
        /// Checks if a path contains a valid bundle file.
        /// </summary>
        /// <param name="path">The absolute path to a bundle file.</param>
        /// <returns>True if the bundle file path is valid.</returns>
        private static bool TryGetBundleFile(string path, out FileInfo file)
        {
            // check that the file path is valid and can be accessed
            try
            {
                file = new FileInfo(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Asset bundle file \"{path}\" cannot be accessed! {e}");
                file = null;
                return false;
            }

            return file.Exists;
        }
    }
}
