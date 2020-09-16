using System;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Settings
{
    /// <summary>
    /// The base class for settings.
    /// </summary>
    public abstract class Setting : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The category this setting is grouped by.")]
        private SettingCategory m_category = null;

        /// <summary>
        /// The category this setting is grouped by.
        /// </summary>
        public SettingCategory Category => m_category;

        [SerializeField]
        [Tooltip("A description of what the setting does.")]
        [TextArea]
        private string m_description = null;

        /// <summary>
        /// A description of what the setting does.
        /// </summary>
        public string Description => m_description;

        [SerializeField]
        [Tooltip("How the setting should be selected in the user interface.")]
        private SettingDisplayMode m_displayMode = SettingDisplayMode.Spinner;

        /// <summary>
        /// How the setting should be selected in the user interface.
        /// </summary>
        public SettingDisplayMode DisplayMode => m_displayMode;

        [SerializeField]
        [Tooltip("The value used until settings are loaded or if deserializtion fails.")]
        protected string m_defaultValue = null;

        [SerializeField]
        [Tooltip("When a preset is added, this setting can only be modified by the user when any of settings matches the preset value.")]
        private SettingPreset[] m_onlyModifiableIf = null;

        [SerializeField]
        [Tooltip("When a preset is added, this setting is only shown in the user interface when any of settings matches the preset value.")]
        private SettingPreset[] m_onlyVisibleIf = null;


        /// <summary>
        /// The serialized setting value.
        /// </summary>
        public string SerializedValue { get; protected set; } = null;

        /// <summary>
        /// Is this setting able to be changed from the user interface.
        /// </summary>
        public bool IsModifiable { get; private set; }

        /// <summary>
        /// Is this setting visible in the user interface.
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// Are the permitted values for this setting determined at runtime.
        /// </summary>
        public virtual bool IsRuntime => false;

        /// <summary>
        /// The options to show when using a dropdown or spinner display mode.
        /// </summary>
        public abstract string[] DisplayValues { get; }

        /// <summary>
        /// An event invoked when the setting value has changed.
        /// </summary>
        public event Action ValueChanged = delegate { };

        /// <summary>
        /// An event invoked when <see cref="IsModifiable"/> has changed.
        /// </summary>
        public event Action<bool> ModifiabilityChanged;

        /// <summary>
        /// An event invoked when <see cref="IsVisible"/> has changed.
        /// </summary>
        public event Action<bool> VisibilityChanged;


        /// <summary>
        /// Called to initialize the setting.
        /// </summary>
        internal virtual void Initialize()
        {
            ValueChanged = delegate { };
            SerializedValue = m_defaultValue;
        }

        /// <summary>
        /// Called after all settings have been initialized.
        /// </summary>
        internal virtual void AfterInitialize()
        {
            foreach (var dependency in m_onlyModifiableIf)
            {
                dependency.Setting.ValueChanged += OnModifiabilityDependencyChanged;
            }
            foreach (var dependency in m_onlyVisibleIf)
            {
                dependency.Setting.ValueChanged += OnVisibilityDependencyChanged;
            }

            OnModifiabilityDependencyChanged();
            OnVisibilityDependencyChanged();
        }

        /// <summary>
        /// Sets this setting from a serialized value.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public abstract void SetSerializedValue(string newValue);

        /// <summary>
        /// Checks if this setting is valid.
        /// </summary>
        /// <returns>True if the setting is valid.</returns>
        internal virtual bool Validate()
        {
            var valid = true;

            if (m_category == null)
            {
                Debug.LogError($"Setting \"{name}\" needs to be assigned a category!");
                valid = false;
            }
            foreach (var preset in m_onlyModifiableIf)
            {
                if (preset.Setting == null)
                {
                    Debug.LogError($"Setting \"{name}\" has a null modifiability dependency!");
                    valid = false;
                }
            }
            foreach (var preset in m_onlyVisibleIf)
            {
                if (preset.Setting == null)
                {
                    Debug.LogError($"Setting \"{name}\" has a null visibility dependency!");
                    valid = false;
                }
            }

            return valid;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draws the control used to select a setting value in the inspector.
        /// </summary>
        /// <param name="pos">The control rect.</param>
        /// <param name="serializedValue">The current value of the setting.</param>
        /// <returns>The new setting value.</returns>
        internal virtual string OnInspectorGUI(Rect pos, string serializedValue)
        {
            EditorGUI.LabelField(pos, "Not configurable");
            return default;
        }
#endif

        /// <summary>
        /// Called when the setting value has changed.
        /// </summary>
        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke();
        }

        /// <summary>
        /// Determines if this setting is able to be changed from the user interface.
        /// </summary>
        /// <returns>True if the setting can be changed, false otherwise.</returns>
        protected virtual bool ComputeIsModifiable()
        {
            if (m_onlyModifiableIf.Length == 0)
            {
                return true;
            }

            for (var i = 0; i < m_onlyModifiableIf.Length; i++)
            {
                if (m_onlyModifiableIf[i].IsApplied)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if this setting is visible in the user interface.
        /// </summary>
        /// <returns>True if the setting is visible, false otherwise.</returns>
        protected virtual bool ComputeIsVisible()
        {
            if (DisplayMode == SettingDisplayMode.Hidden)
            {
                return false;
            }
            if (m_onlyVisibleIf.Length == 0)
            {
                return true;
            }

            for (var i = 0; i < m_onlyVisibleIf.Length; i++)
            {
                if (m_onlyVisibleIf[i].IsApplied)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnModifiabilityDependencyChanged()
        {
            var modifiable = ComputeIsModifiable();

            if (IsModifiable != modifiable)
            {
                IsModifiable = modifiable;
                ModifiabilityChanged?.Invoke(modifiable);
            }
        }

        private void OnVisibilityDependencyChanged()
        {
            var visible = ComputeIsVisible();

            if (IsVisible != visible)
            {
                IsVisible = visible;
                VisibilityChanged?.Invoke(visible);
            }
        }
    }

    /// <summary>
    /// The base class for settings.
    /// </summary>
    /// <typeparam name="T">The type used to store the setting.</typeparam>
    public abstract class Setting<T> : Setting
    {
        private T m_value = default;

        /// <summary>
        /// The value of this setting.
        /// </summary>
        public T Value
        {
            get => m_value;
            set
            {
                var validated = Sanitize(value);

                // only update if the value has changed
                if (!Equals(m_value, validated))
                {
                    m_value = validated;
                    SerializedValue = Serialize(validated);

                    OnValueChanged();
                }
            }
        }

        /// <summary>
        /// An event invoked when the setting value has changed.
        /// </summary>
        protected event Action<T> TypedValueChanged = delegate { };


        /// <inheritdoc/>
        internal override void Initialize()
        {
            base.Initialize();

            TypedValueChanged = delegate { };
            Deserialize(SerializedValue, out m_value);
        }

        /// <inheritdoc/>
        public override void SetSerializedValue(string newValue)
        {
            if (Deserialize(newValue, out var value))
            {
                Value = value;
            }
            else
            {
                Debug.LogError($"Failed to deserialize \"{SerializedValue}\" for setting \"{name}\"!");
            }
        }

        /// <summary>
        /// Registers a callback to be invoked when the setting changes and immetiately invokes
        /// it with the current setting value.
        /// </summary>
        /// <param name="callback">The setting change handler to register. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
        public void RegisterChangeHandler(Action<T> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            TypedValueChanged += callback;
            callback(m_value);
        }

        /// <summary>
        /// Degisters a callback so it is no longer invoked when the setting value changes.
        /// </summary>
        /// <param name="callback">The setting change handler to deregister. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is null.</exception>
        public void DeregisterChangeHandler(Action<T> callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            TypedValueChanged -= callback;
        }

        /// <summary>
        /// Processes a setting value to make sure it is valid.
        /// </summary>
        /// <param name="newValue">The value to validate.</param>
        /// <returns>The processed value/</returns>
        internal virtual T Sanitize(T newValue)
        {
            return newValue;
        }

        /// <summary>
        /// Gets a string representation of this setting's value.
        /// </summary>
        /// <param name="value">The value to deserialize.</param>
        internal abstract string Serialize(T value);

        /// <summary>
        /// Sets the value of this setting from a serialized value.
        /// </summary>
        /// <param name="serialized">The serialized value.</param>
        /// <param name="value">The deserialized value.</param>
        /// <returns>True if the deserialization was successful.</returns>
        internal abstract bool Deserialize(string serialized, out T value);

        /// <inheritdoc/>
        protected override void OnValueChanged()
        {
            base.OnValueChanged();

            TypedValueChanged?.Invoke(m_value);
        }
    }
}
