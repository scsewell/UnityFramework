using System.Linq;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Framework.Settings
{
    /// <summary>
    /// Manages configuring the settings prior to building.
    /// </summary>
    internal class SettingsBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; } = 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (SettingManager.Instance != null)
            {
                SettingManager.Instance.OnBuild();

                // Add the settings manager asset to the build
                var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();

                if (!preloadedAssets.Contains(SettingManager.Instance))
                {
                    preloadedAssets.Add(SettingManager.Instance);
                    PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
                }
            }
        }
    }
}
