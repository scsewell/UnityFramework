using System;
using System.Collections.Generic;

using UnityEngine;

namespace Framework.Settings
{
    /// <summary>
    /// Manages converting settings to and from json.
    /// </summary>
    public static class SettingSerializer
    {
        [Serializable]
        private struct SerializedSettings
        {
            public List<SerializedCategory> categories;
        }

        [Serializable]
        private struct SerializedCategory
        {
            public string name;
            public List<SerializedSetting> settings;
        }

        [Serializable]
        private struct SerializedSetting
        {
            public string name;
            public string value;
        }

        /// <summary>
        /// Applies serialized values to a set of settings.
        /// </summary>
        /// <param name="json">The serialized setting values.</param>
        /// <param name="settings">The settings to apply the deserialized values to.</param>
        public static void FromJson(string json, IEnumerable<Setting> settings)
        {
            SerializedSettings deserialized = JsonUtility.FromJson<SerializedSettings>(json);

            foreach (SerializedCategory category in deserialized.categories)
            {
                foreach (SerializedSetting setting in category.settings)
                {
                    // if there is a setting matching the serialized one apply its value
                    foreach (Setting s in settings)
                    {
                        if (s.name == setting.name && s.Category.name == category.name)
                        {
                            s.SetSerializedValue(setting.value);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Serializes the given settings as json.
        /// </summary>
        /// <param name="categoryToSettings">The settings to serialize grouped by category.</param>
        /// <returns>The serialized settings.</returns>
        public static string ToJson(Dictionary<SettingCategory, List<Setting>> categoryToSettings)
        {
            // get a json representation of the settings
            SerializedSettings serializedSettings = new SerializedSettings()
            {
                categories = new List<SerializedCategory>(categoryToSettings.Count),
            };

            foreach (var pair in categoryToSettings)
            {
                SerializedCategory serializedCategory = new SerializedCategory()
                {
                    name = pair.Key.name,
                    settings = new List<SerializedSetting>(pair.Value.Count),
                };

                foreach (Setting setting in pair.Value)
                {
                    serializedCategory.settings.Add(new SerializedSetting()
                    {
                        name = setting.name,
                        value = setting.SerializedValue,
                    });
                }

                serializedSettings.categories.Add(serializedCategory);
            }

            return JsonUtility.ToJson(serializedSettings, true);
        }
    }
}
