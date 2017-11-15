using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.SettingManagement
{
    /// <summary>
    /// Manages a collection of setting for the application. Is able to serialize and deserialize the relevant values.
    /// </summary>
    public class Settings
    {
        private Dictionary<string, List<ISetting>> m_categoryToSettings;
        public Dictionary<string, List<ISetting>> CategoryToSettings
        {
            get { return m_categoryToSettings; }
        }

        public List<string> Categories
        {
            get { return m_categoryToSettings.Keys.ToList(); }
        }

        public Settings()
        {
            m_categoryToSettings = new Dictionary<string, List<ISetting>>();
        }
        
        public Setting<bool> Add(string category, string name, bool defaultValue, Action<bool> apply = null)
        {
            Setting<bool> setting = new Setting<bool>(name, defaultValue, (v) => v.ToString(), (s) => bool.Parse(s), apply, new DisplayOptions());
            AddSetting(category, setting);
            return setting;
        }

        public Setting<float> Add(string category, string name, float defaultValue, float minValue, float maxValue, bool intOnly, Action<float> apply = null)
        {
            Setting<float> setting = new Setting<float>(name, defaultValue, (v) => v.ToString(), (s) => float.Parse(s), apply, new DisplayOptions(minValue, maxValue, intOnly));
            AddSetting(category, setting);
            return setting;
        }

        public Setting<T> Add<T>(string category, string name, T defaultValue, string[] values, Func<T, string> serialize, Func<string, T> deserialize, Action<T> apply = null)
        {
            Setting<T> setting = new Setting<T>(name, defaultValue, serialize, deserialize, apply, new DisplayOptions(values));
            AddSetting(category, setting);
            return setting;
        }

        private void AddSetting(string category, ISetting setting)
        {
            List<ISetting> settings;

            if (m_categoryToSettings.ContainsKey(category))
            {
                settings = m_categoryToSettings[category];
            }
            else
            {
                settings = new List<ISetting>();
                m_categoryToSettings.Add(category, settings);
            }

            if (!settings.Any(s => s.Name == setting.Name))
            {
                settings.Add(setting);
            }
        }

        public ISetting GetSetting(string name)
        {
            foreach (string category in m_categoryToSettings.Keys)
            {
                foreach (ISetting setting in m_categoryToSettings[category])
                {
                    if (setting.Name == name)
                    {
                        return setting;
                    }
                }
            }
            Debug.LogError("Setting with name " + name + " does not exist!");
            return null;
        }

        public void Apply()
        {
            foreach (List<ISetting> settings in m_categoryToSettings.Values)
            {
                settings.ForEach(setting => setting.Apply());
            }
        }

        public void UseDefaults()
        {
            foreach (List<ISetting> settings in m_categoryToSettings.Values)
            {
                settings.ForEach(setting => setting.UseDefaultValue());
            }
        }

        public SerializableSettings Serialize()
        {
            return new SerializableSettings().Serialize(this);
        }

        public bool Deserialize(SerializableSettings serializableSetting)
        {
            return serializableSetting.Deserialize(this);
        }
    }
}
