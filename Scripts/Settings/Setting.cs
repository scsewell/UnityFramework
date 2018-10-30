using System;

namespace Framework.SettingManagement
{
    public class Setting<T> : ISetting
    {
        private string m_name;
        public string Name
        {
            get { return m_name; }
        }

        private T m_defaultValue;
        public T DefaultValue
        {
            get { return m_defaultValue; }
        }

        private T m_value;
        public T Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        private DisplayOptions m_displayOptions;
        public DisplayOptions DisplayOptions
        {
            get { return m_displayOptions; }
        }

        private Action<T> m_apply;
        private Func<T, string> m_serialize;
        private Func<string, T> m_deserialize;

        /// <summary>
        /// Constructs a new settings object.
        /// </summary>
        /// <param name="name">The name used for retrieving settings and for display.</param>
        /// <param name="defaultValue">The default value used until changed.</param>
        /// <param name="serialize">The function that serializes the value.</param>
        /// <param name="deserialize">The function that deserializes the value.</param>
        /// <param name="apply">The action taken when all settings are applied.</param>
        /// <param name="displayOptions">Determines how the setting is displayed in a settings menu.</param>
        public Setting(string name, T defaultValue, Func<T, string> serialize, Func<string, T> deserialize, Action<T> apply, DisplayOptions displayOptions)
        {
            m_name = name;
            m_defaultValue = defaultValue;
            m_apply = apply;
            m_serialize = serialize;
            m_deserialize = deserialize;
            m_displayOptions = displayOptions;

            UseDefaultValue();
        }

        public void Apply()
        {
            if (m_apply != null)
            {
                m_apply(m_value);
            }
        }

        public void UseDefaultValue()
        {
            m_value = m_defaultValue;
        }

        public string Serialize()
        {
            return m_serialize(m_value);
        }

        public void Deserialize(string value)
        {
            m_value = m_deserialize(value);
        }

        public bool IsOfType(Type type)
        {
            return typeof(T).Equals(type);
        }
    }
}
