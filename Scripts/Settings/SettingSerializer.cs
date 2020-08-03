using System;
using System.Collections.Generic;

using UnityEngine;

namespace Framework.Settings
{
    /// <summary>
    /// Manages converting settings to and from json.
    /// </summary>
    internal static class SettingSerializer
    {
        [Serializable]
        private struct Settings
        {
            public List<Category> categories;
        }

        [Serializable]
        private struct Category
        {
            public string name;
            public List<Value> settings;
        }

        [Serializable]
        private struct Value
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
            var deserialized = JsonUtility.FromJson<Settings>(json);

            foreach (var category in deserialized.categories)
            {
                foreach (var setting in category.settings)
                {
                    // if there is a setting matching the serialized one apply its value
                    foreach (var s in settings)
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
            var settings = new Settings
            {
                categories = new List<Category>(categoryToSettings.Count),
            };

            foreach (var pair in categoryToSettings)
            {
                var category = new Category
                {
                    name = pair.Key.name,
                    settings = new List<Value>(pair.Value.Count),
                };

                foreach (var setting in pair.Value)
                {
                    var value = new Value
                    {
                        name = setting.name,
                        value = setting.SerializedValue,
                    };

                    category.settings.Add(value);
                }

                settings.categories.Add(category);
            }

            return JsonUtility.ToJson(settings, true);
        }
    }
}
