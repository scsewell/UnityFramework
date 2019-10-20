using System;
using System.Linq;

using UnityEngine;

namespace Framework.Settings
{
    [CreateAssetMenu(fileName = "New Enum Setting", menuName = "Framework/Settings/Enum", order = 4)]
    public class EnumSetting : Setting<Enum>
    {
        [SerializeField]
        [Tooltip("The type of enum used.")]
        private string m_enumType = null;

        [Serializable]
        public class Mapping
        {
            public string enumName;
            public string displayName;
        }

        [SerializeField]
        [Tooltip("The mapping between enum named values and display values.")]
        private Mapping[] m_values = null;

        private Type m_type = null;

        /// <summary>
        /// The type of the underlying enum for this setting.
        /// </summary>
        public Type Type
        {
            get
            {
#if !UNITY_EDITOR
                if (m_type == null)
#endif
                {
                    m_type = Type.GetType(m_enumType);
                }
                return m_type;
            }
        }

        private string[] m_displayValues = null;

        /// <summary>
        /// The display options for the enum values.
        /// </summary>
        public string[] DisplayValues
        {
            get
            {
#if !UNITY_EDITOR
                if (m_displayValues == null)
#endif
                {
                    m_displayValues = m_values.Select(m => m.displayName).ToArray();
                }
                return m_displayValues;
            }
        }

        public override bool Deserialize(string serialized, out Enum value)
        {
            value = default;

            if (string.IsNullOrWhiteSpace(serialized))
            {
                return false;
            }

            string enumName = null;
            for (int i = 0; i < m_values.Length; i++)
            {
                if (m_values[i].displayName == serialized)
                {
                    enumName = m_values[i].enumName;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(enumName))
            {
                return false;
            }

            object parsed = Enum.Parse(Type, enumName, true);

            if (!Enum.IsDefined(Type, parsed))
            {
                return false;
            }

            value = parsed as Enum;
            return true;
        }

        public override string Serialize(Enum value)
        {
            string enumName = value.ToString();

            for (int i = 0; i < m_values.Length; i++)
            {
                if (m_values[i].enumName == enumName)
                {
                    return m_values[i].displayName;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the correctly typed enum value.
        /// </summary>
        /// <typeparam name="T">The type of the enum. Must match the assigned type for this setting.</typeparam>
        /// <returns>The casted value.</returns>
        public T GetTypedValue<T>() where T : Enum
        {
            if (Type != typeof(T))
            {
                Debug.LogError($"Type {typeof(T).FullName} must match the setting type {Type.FullName} on setting \"{name}\"!");
                return default;
            }
            if (!Enum.IsDefined(Type, Value))
            {
                Debug.LogError($"Setting value {(T)Value} is not valid for the type {Type.FullName} on setting \"{name}\"!");
                return default;
            }
            return (T)Value;
        }
    }
}
