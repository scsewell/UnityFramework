using System;
using System.Collections.Generic;

using UnityEngine;

using Malee;

namespace Framework.Settings
{
    /// <summary>
    /// A set of values which can be applied to settings.
    /// </summary>
    [CreateAssetMenu(fileName = "New Preset Group", menuName = "Framework/Settings/Preset Group", order = 51)]
    public class SettingPresetGroup : ScriptableObject
    {
        [Serializable]
        public class SettingList : ReorderableArray<SettingPreset>
        {
        }

        [SerializeField]
        [Reorderable]
        private SettingList m_presets = null;

        /// <summary>
        /// Checks if all the settings are using the values in this preset.
        /// </summary>
        public bool IsApplied()
        {
            foreach (SettingPreset preset in m_presets)
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
            foreach (SettingPreset preset in m_presets)
            {
                preset.Apply();
            }
        }

        private void OnValidate()
        {
            // prevent one preset from having multiple values for one setting
            HashSet<Setting> settings = new HashSet<Setting>();

            for (int i = 0; i < m_presets.Length; i++)
            {
                Setting setting = m_presets[i].Setting;

                if (setting != null && settings.Contains(setting))
                {
                    m_presets.RemoveAt(i);
                    m_presets.Insert(i, new SettingPreset());
                }
                else
                {
                    settings.Add(setting);
                }
            }
        }
    }
}
