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

        public override string[] DisplayValues
        {
            get
            {
#if UNITY_EDITOR
                return m_values.Select(m => m.displayName).ToArray();
#else
                return m_displayValues;
#endif
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            m_displayValues = m_values.Select(m => m.displayName).ToArray();
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

        public override bool Validate()
        {
            bool valid = base.Validate();

            if (Type == null)
            {
                Debug.LogError($"Setting \"{name}\" needs an enum type assigned!");
                valid = false;
            }
            else
            {
                for (int i = 0; i < m_values.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(m_values[i].displayName))
                    {
                        Debug.LogError($"Setting \"{name}\" needs a mapping for enum value \"{m_values[i].enumName}\"!");
                        valid = false;
                    }
                }
            }

            return valid;
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
