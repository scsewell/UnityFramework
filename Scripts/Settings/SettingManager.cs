using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    [CreateAssetMenu(fileName = "SettingManager", menuName = "Framework/Settings/Setting Manager", order = 80)]
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            Instance.Initialize();
        }


        [SerializeField]
        [LeftToggle]
        [Tooltip("Automatically add all settings in the project to the setting list.")]
        private bool m_autoAddSettings = true;

        [SerializeField]
        private List<Setting> m_settings = null;

        private List<SettingCategory> m_categories = null;
        private Dictionary<SettingCategory, List<Setting>> m_categoryToSettings = null;

        /// <summary>
        /// The categories the settings are grouped by.
        /// </summary>
        public IReadOnlyList<SettingCategory> Catergories => m_categories;


        /// <summary>
        /// Prepares the initial settings.
        /// </summary>
        private void Initialize()
        {
            // build the grouping of setting by category
            m_categoryToSettings = new Dictionary<SettingCategory, List<Setting>>();

            foreach (var setting in m_settings)
            {
                var category = setting.Category;

                if (!m_categoryToSettings.TryGetValue(category, out var settings))
                {
                    settings = new List<Setting>();
                    m_categoryToSettings.Add(category, settings);
                }

                settings.Add(setting);
            }

            m_categories = m_categoryToSettings.Keys.OrderBy(c => c.SortOrder).ToList();

            // prepare all the settings
            foreach (var setting in m_settings)
            {
                setting.Initialize();
            }
            foreach (var setting in m_settings)
            {
                setting.AfterInitialize();
            }

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
            if (category != null && m_categoryToSettings.TryGetValue(category, out var settings))
            {
                return settings;
            }
            return null;
        }

        /// <summary>
        /// Loads the saved settings.
        /// </summary>
        public void Load()
        {
            var path = Path.Combine(FileIO.ConfigDirectory, FILE_NAME);

            if (FileIO.ReadFileText(path, out var json))
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
            var path = Path.Combine(FileIO.ConfigDirectory, FILE_NAME);

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
                case PlayModeStateChange.ExitingEditMode:
                {
                    if (Instance != null)
                    {
                        Instance.OnBuild();
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Ensures the settings list is up to date and validates the settings.
        /// </summary>
        internal void OnBuild()
        {
            if (m_autoAddSettings)
            {
                AddAllSettings();
                ValidateSettings();
            }
        }

        /// <summary>
        /// Adds all settings in the project to the settings list.
        /// </summary>
        internal void AddAllSettings()
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(Setting).FullName}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var setting = AssetDatabase.LoadAssetAtPath<Setting>(path);

                if (!m_settings.Contains(setting))
                {
                    m_settings.Add(setting);
                }
            }

            RemoveDuplicateSettings();
            RemoveEmptySettings();
        }

        /// <summary>
        /// Ensures each setting is only assigned once to the settings list.
        /// </summary>
        internal void RemoveDuplicateSettings()
        {
            var settings = new HashSet<Setting>();

            for (var i = 0; i < m_settings.Count; i++)
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

        private void RemoveEmptySettings()
        {
            for (var i = 0; i < m_settings.Count;)
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

        private bool ValidateSettings()
        {
            var valid = true;

            foreach (var setting in m_settings)
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
