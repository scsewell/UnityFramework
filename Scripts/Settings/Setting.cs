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
        [Tooltip("Changing this setting requires restarting the engine.")]
        private bool m_requiresRestart = false;

        [SerializeField]
        [Tooltip("How the setting should be selected in the user interface.")]
        private SettingDisplayMode m_displayMode = SettingDisplayMode.Spinner;

        [SerializeField]
        [Tooltip("The value used until settings are loaded or if deserializtion fails.")]
        protected string m_defaultValue = null;

        protected string m_serializedValue = null;

        /// <summary>
        /// Are the permitted values for this setting determined at runtime.
        /// </summary>
        public virtual bool IsRuntime => false;

        /// <summary>
        /// Initializes this setting.
        /// </summary>
        public virtual void Initialize()
        {
            m_serializedValue = m_defaultValue;
        }

        /// <summary>
        /// The serialized setting value.
        /// </summary>
        public string SerializedValue => m_serializedValue;

        /// <summary>
        /// Sets this setting from a serialized value.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public abstract void SetSerializedValue(string newValue);
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
        public event Action<T> OnValueChanged;

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
                if (!m_value.Equals(validated))
                {
                    m_value = validated;
                    m_serializedValue = Serialize(validated);

                    OnValueChanged?.Invoke(validated);
                }
            }
        }

        /// <summary>
        /// Sets this setting from a serialized value.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public override void SetSerializedValue(string newValue)
        {
            if (Deserialize(m_serializedValue, out T value))
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
