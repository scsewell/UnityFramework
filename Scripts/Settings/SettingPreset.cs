using System;

using UnityEngine;

namespace Framework.Settings
{
    /// <summary>
    /// A default value for a setting.
    /// </summary>
    [Serializable]
    public class SettingPreset
    {
        [SerializeField]
        [Tooltip("The setting to create a preset for.")]
        private Setting m_setting = null;
        public Setting Setting => m_setting;

        [SerializeField]
        [Tooltip("The preset value.")]
        private string m_value = null;

        /// <summary>
        /// Checks if the setting is using this preset's value.
        /// </summary>
        public bool IsApplied => m_setting.SerializedValue == m_value;

        /// <summary>
        /// Applies the preset value to the setting.
        /// </summary>
        public void Apply()
        {
            if (m_setting.IsRuntime)
            {
                Debug.LogWarning($"Present cannot be applied to setting \"{m_setting.name}\", it is configured at runtime!");
            }
            else
            {
                m_setting.SetSerializedValue(m_value);
            }
        }
    }
}
