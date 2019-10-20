using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Malee;

using Framework.IO;

namespace Framework.Settings
{
    /// <summary>
    /// Manages the settings for an application.
    /// </summary>
    [CreateAssetMenu(fileName = "SettingManager", menuName = "Framework/Settings/SettingManager", order = 80)]
    public class SettingManager : ScriptableObject
    {
        /// <summary>
        /// The settings file name.
        /// </summary>
        private const string FILE_NAME = "Settings.ini";


        private static SettingManager m_instance = null;

        /// <summary>
        /// The settings manager instance.
        /// </summary>
        public static SettingManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = Resources.Load<SettingManager>(nameof(SettingManager));

                    if (m_instance == null && Application.isPlaying)
                    {
                        m_instance = CreateInstance<SettingManager>();
                        Debug.LogWarning($"No settings manager found! Create a {nameof(SettingManager)} asset with path \"Resources/{nameof(SettingManager)}\" to add settings.");
                    }
                }
                return m_instance;
            }
        }

        [SerializeField]
        [Tooltip("Will all settings in the project automatically be added to the setting list.")]
        private bool m_autoAddSettings = true;

        [Serializable]
        public class SettingList : ReorderableArray<Setting>
        {
        }

        [SerializeField]
        [Reorderable]
        private SettingList m_settings = new SettingList();

        private readonly Dictionary<SettingCategory, List<Setting>> m_categoryToSetting = new Dictionary<SettingCategory, List<Setting>>();


        [RuntimeInitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            // after awake is called initialize the settings
            Instance.Initialize();
        }

        /// <summary>
        /// Prepares the initial settings.
        /// </summary>
        private void Initialize()
        {
            // get the grouping of setting by category
            m_categoryToSetting.Clear();

            for (int i = 0; i < m_settings.Length; i++)
            {
                List<Setting> settings;
                if (!m_categoryToSetting.TryGetValue(m_settings[i].Category, out settings))
                {
                    settings = new List<Setting>();
                    m_categoryToSetting.Add(m_settings[i].Category, settings);
                }

                settings.Add(m_settings[i]);
            }

            // prepare all the settings
            foreach (Setting setting in m_settings)
            {
                if (setting != null)
                {
                    setting.Initialize();
                }
                else
                {
                    Debug.LogWarning("SettingManager has a null setting!");
                }
            }

            // try to load the saved settings
            Load();
        }

        [Serializable]
        public struct SerializedSettings
        {
            public List<SerializedCategory> categories;
        }

        [Serializable]
        public struct SerializedCategory
        {
            public string name;
            public List<SerializedSetting> settings;
        }

        [Serializable]
        public struct SerializedSetting
        {
            public string name;
            public string value;
        }

        /// <summary>
        /// Loads the saved settings.
        /// </summary>
        public void Load()
        {
            string path = Path.Combine(FileIO.GetConfigDirectory(), FILE_NAME);

            if (FileIO.ReadFileText(path, out string json))
            {
                SerializedSettings settings = JsonUtility.FromJson<SerializedSettings>(json);

                foreach (SerializedCategory category in settings.categories)
                {
                    foreach (SerializedSetting setting in category.settings)
                    {
                        // if there is a setting matching the serialized one apply its value
                        foreach (Setting s in m_settings)
                        {
                            if (s.name == setting.name && s.Category.name == category.name)
                            {
                                s.SetSerializedValue(setting.value);
                                break;
                            }
                        }
                    }
                }

                Debug.Log("Loaded settings");
            }
            else
            {
                Debug.LogWarning("Failed to load settings!");
            }
        }

        /// <summary>
        /// Saves the current settings.
        /// </summary>
        public void Save()
        {
            // get a json representation of the settings
            SerializedSettings serializedSettings = new SerializedSettings()
            {
                categories = new List<SerializedCategory>(m_categoryToSetting.Count),
            };

            foreach (var pair in m_categoryToSetting)
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
            }

            string json = JsonUtility.ToJson(serializedSettings, true);

            // write the file
            string path = Path.Combine(FileIO.GetConfigDirectory(), FILE_NAME);

            if (FileIO.WriteFile(path, json))
            {
                Debug.Log("Saved settings");
            }
            else
            {
                Debug.LogError("Failed to saving settings!");
            }
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void PlayModeInit()
        {
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private static void PlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                {
                    if (Instance != null && Instance.m_autoAddSettings)
                    {
                        Instance.AddAllSettings();
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Adds a setting to the settings included in the build.
        /// </summary>
        /// <param name="setting">The setting to add.</param>
        public void AddSetting(Setting setting)
        {
            if (m_autoAddSettings && !m_settings.Contains(setting))
            {
                m_settings.Add(setting);
            }
        }

        /// <summary>
        /// Adds all settings in the project to the settings list.
        /// </summary>
        public void AddAllSettings()
        {
            if (m_autoAddSettings)
            {
                foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(Setting).FullName}"))
                {
                    AddSetting(AssetDatabase.LoadAssetAtPath<Setting>(AssetDatabase.GUIDToAssetPath(guid)));
                }

                RemoveDuplicateSettings();
                RemoveEmptySettings();
            }

            foreach (Setting setting in m_settings)
            {
                if (setting.Category == null)
                {
                    Debug.LogError($"Setting \"{setting.name}\" needs to be assigned a category!");
                }
            }
        }

        /// <summary>
        /// Ensures each setting is only assigned once to the settings list.
        /// </summary>
        public void RemoveDuplicateSettings()
        {
            HashSet<Setting> settings = new HashSet<Setting>();
            for (int i = 0; i < m_settings.Length; i++)
            {
                if (settings.Contains(m_settings[i]))
                {
                    m_settings[i] = null;
                }
                else
                {
                    settings.Add(m_settings[i]);
                }
            }
        }

        /// <summary>
        /// Ensures all settings are assigned.
        /// </summary>
        public void RemoveEmptySettings()
        {
            for (int i = 0; i < m_settings.Length;)
            {
                if (m_settings[i] == null)
                {
                    m_settings.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
#endif
    }
}
