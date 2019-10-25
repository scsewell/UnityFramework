using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Framework.Settings
{
    /// <summary>
    /// Manages configuring the settings prior to building.
    /// </summary>
    public class SettingsBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; } = 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (SettingManager.Instance != null)
            {
                SettingManager.Instance.AddAllSettings();
                SettingManager.Instance.ValidateSettings();

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
