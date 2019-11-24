using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Framework.IO;

namespace Framework.Settings
{
    /// <summary>
    /// Manages the settings for the application.
    /// </summary>
    [CreateAssetMenu(fileName = "SettingManager", menuName = "Framework/Settings/SettingManager", order = 80)]
    public class SettingManager : ScriptableObject
    {
        /// <summary>
        /// The settings file name.
        /// </summary>
        private const string FILE_NAME = "Settings.ini";


        private static SettingManager m_instance;

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            m_instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            Instance.Initialize();
        }


        [SerializeField]
        [Tooltip("Will all settings in the project automatically be added to the setting list.")]
        private bool m_autoAddSettings = true;

        [Serializable]
        public class SettingList : ReorderableArray<Setting>
        {
        }

        [SerializeField]
        [Reorderable(singleLine = true)]
        private SettingList m_settings = new SettingList();

        private bool m_initialized = false;
        private List<SettingCategory> m_categories = null;
        private Dictionary<SettingCategory, List<Setting>> m_categoryToSettings = null;

        /// <summary>
        /// The categories the settings are grouped by.
        /// </summary>
        public IReadOnlyList<SettingCategory> Catergories => m_categories as IReadOnlyList<SettingCategory>;


#if UNITY_EDITOR
        private void OnEnable()
        {
            m_initialized = false;
        }
#endif

        /// <summary>
        /// Prepares the initial settings.
        /// </summary>
        private void Initialize()
        {
            if (m_initialized)
            {
                return;
            }

            // get the grouping of setting by category
            m_categories = new List<SettingCategory>();
            m_categoryToSettings = new Dictionary<SettingCategory, List<Setting>>();

            for (int i = 0; i < m_settings.Length; i++)
            {
                SettingCategory category = m_settings[i].Category;

                List<Setting> settings;
                if (!m_categoryToSettings.TryGetValue(category, out settings))
                {
                    m_categories.Add(category);

                    settings = new List<Setting>();
                    m_categoryToSettings.Add(category, settings);
                }

                settings.Add(m_settings[i]);
            }

            // prepare all the settings
            foreach (Setting setting in m_settings)
            {
                setting.Initialize();
            }

            m_initialized = true;

            // try to load the saved settings
            Load();
        }

        /// <summary>
        /// Gets the settings that belong to a category.
        /// </summary>
        /// <param name="category">The category to get the settings for.</param>
        /// <returns>The settings list for the category.</returns>
        public IReadOnlyList<Setting> GetSettings(SettingCategory category)
        {
            if (category != null && m_categoryToSettings.TryGetValue(category, out List<Setting> settings))
            {
                return settings as IReadOnlyList<Setting>;
            }
            return null;
        }

        /// <summary>
        /// Loads the saved settings.
        /// </summary>
        public void Load()
        {
            string path = Path.Combine(FileIO.GetConfigDirectory(), FILE_NAME);

            if (FileIO.ReadFileText(path, out string json))
            {
                SettingSerializer.FromJson(json, m_settings);

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
            string path = Path.Combine(FileIO.GetConfigDirectory(), FILE_NAME);

            if (FileIO.WriteFile(path, SettingSerializer.ToJson(m_categoryToSettings)))
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
                case PlayModeStateChange.EnteredPlayMode:
                {
                    if (Instance != null && Instance.m_autoAddSettings)
                    {
                        Instance.AddAllSettings();
                        Instance.ValidateSettings();
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Adds a setting to the settings included in the build.
        /// </summary>
        /// <param name="setting">The setting to add.</param>
        private void AddSetting(Setting setting)
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
        
        /// <summary>
        /// Checks if all settings are valid.
        /// </summary>
        /// <returns>True if the settings are valid.</returns>
        public bool ValidateSettings()
        {
            bool valid = true;
            
            foreach (Setting setting in m_settings)
            {
                if (!setting.Validate())
                {
                    valid = false;
                }
            }
            
            return valid;
        }
#endif
    }
}
