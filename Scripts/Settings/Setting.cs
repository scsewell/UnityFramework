using System;

using UnityEngine;

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
        public SettingCategory Category => m_category;

        [SerializeField]
        [Tooltip("How the setting should be selected in the user interface.")]
        private SettingDisplayMode m_displayMode = SettingDisplayMode.Spinner;
        public SettingDisplayMode DisplayMode => m_displayMode;

        [SerializeField]
        [Tooltip("The value used until settings are loaded or if deserializtion fails.")]
        protected string m_defaultValue = null;

        /// <summary>
        /// The serialized setting value.
        /// </summary>
        public string SerializedValue { get; protected set; } = null;

        /// <summary>
        /// Are the permitted values for this setting determined at runtime.
        /// </summary>
        public virtual bool IsRuntime => false;

        /// <summary>
        /// The options to show when using a dropdown or spinner display mode.
        /// </summary>
        public abstract string[] DisplayValues { get; }

        /// <summary>
        /// Initializes this setting.
        /// </summary>
        public virtual void Initialize()
        {
            SerializedValue = m_defaultValue;
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
        public virtual bool Validate()
        {
            bool valid = true;

            if (m_category == null)
            {
                Debug.LogError($"Setting \"{name}\" needs to be assigned a category!");
                valid = false;
            }

            return valid;
        }
    }

    /// <summary>
    /// The base class for settings.
    /// </summary>
    /// <typeparam name="T">The type used to store the setting.</typeparam>
    public abstract class Setting<T> : Setting
    {
        /// <summary>
        /// Called when the value of this setting has changed.
        /// </summary>
        public event Action<T> OnValueChanged = delegate { };

        private T m_value = default;

        /// <summary>
        /// The value of this setting.
        /// </summary>
        public T Value
        {
            get => m_value;
            set
            {
                T validated = Sanitize(value);

                // only update if the value has changed
                if (!Equals(m_value, validated))
                {
                    m_value = validated;
                    SerializedValue = Serialize(validated);

                    OnValueChanged?.Invoke(validated);
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            // load the default value
            OnValueChanged = delegate { };
            Deserialize(SerializedValue, out m_value);
        }

        /// <summary>
        /// Sets this setting from a serialized value.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public override void SetSerializedValue(string newValue)
        {
            if (Deserialize(newValue, out T value))
            {
                Value = value;
            }
            else
            {
                Debug.LogError($"Failed to deserialize \"{SerializedValue}\" for setting \"{name}\"!");
            }
        }

        /// <summary>
        /// Processing a setting value to make sure it is valid.
        /// </summary>
        /// <param name="newValue">The value to validate.</param>
        /// <returns>The processed value/</returns>
        public virtual T Sanitize(T newValue)
        {
            return newValue;
        }

        /// <summary>
        /// Gets a string representation of this setting's value.
        /// </summary>
        /// <param name="value">The value to deserialize.</param>
        public abstract string Serialize(T value);

        /// <summary>
        /// Sets the value of this setting from a serialized value.
        /// </summary>
        /// <param name="serialized">The serialized value.</param>
        /// <param name="value">The deserialized value.</param>
        /// <returns>True if the deserialization was successful.</returns>
        public abstract bool Deserialize(string serialized, out T value);
    }
}
