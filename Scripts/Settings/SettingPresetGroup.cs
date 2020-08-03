using System.Collections.Generic;

using UnityEngine;

namespace Framework.Settings
{
    /// <summary>
    /// A set of predefined values which can be applied to a grouup of settings.
    /// </summary>
    [CreateAssetMenu(fileName = "New Preset", menuName = "Framework/Settings/Preset", order = 51)]
    public class SettingPresetGroup : ScriptableObject
    {
        [SerializeField]
        private List<SettingPreset> m_presets = null;

        /// <summary>
        /// Checks if all the settings are using the values in this preset.
        /// </summary>
        public bool IsApplied()
        {
            foreach (var preset in m_presets)
            {
                if (!preset.IsApplied)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Applies this preset to all relevant settings.
        /// </summary>
        public void Apply()
        {
            Debug.Log($"Applying setting preset \"{name}\"");

            foreach (var preset in m_presets)
            {
                preset.Apply();
            }
        }

        private void OnValidate()
        {
            // Prevent one preset from having multiple values for one setting
            var settings = new HashSet<Setting>();

            for (var i = 0; i < m_presets.Count; i++)
            {
                var setting = m_presets[i].Setting;

                if (setting != null && settings.Contains(setting))
                {
                    m_presets[i] = new SettingPreset();
                }
                else
                {
                    settings.Add(setting);
                }
            }
        }
    }
}
