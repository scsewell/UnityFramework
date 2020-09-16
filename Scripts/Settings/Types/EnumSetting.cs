using System;
using System.Collections.Generic;
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
        private class Mapping
        {
            public string enumName = default;
            public string displayName = default;
        }

        [SerializeField]
        [Tooltip("The mapping between enum named values and display values.")]
        private Mapping[] m_values = null;


        private Type m_type = null;
        private string[] m_displayValues = null;
        private readonly Dictionary<Delegate, Action<Enum>> m_changeHandlers = new Dictionary<Delegate, Action<Enum>>();

        /// <summary>
        /// The type of the underlying enum for this setting.
        /// </summary>
        private Type Type
        {
            get
            {
#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(m_enumType))
#else
                if (m_type == null)
#endif
                {
                    m_type = Type.GetType(m_enumType);
                }
                return m_type;
            }
        }

        /// <inheritdoc/>
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


        /// <inheritdoc/>
        internal override void Initialize()
        {
            base.Initialize();

            m_changeHandlers.Clear();
            m_displayValues = m_values.Select(m => m.displayName).ToArray();
        }

        /// <inheritdoc/>
        internal override bool Deserialize(string serialized, out Enum value)
        {
            value = default;

            if (string.IsNullOrWhiteSpace(serialized))
            {
                return false;
            }

            var enumName = default(string);
            for (var i = 0; i < m_values.Length; i++)
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

            var parsed = Enum.Parse(Type, enumName, true);

            if (!Enum.IsDefined(Type, parsed))
            {
                return false;
            }

            value = parsed as Enum;
            return true;
        }

        /// <inheritdoc/>
        internal override string Serialize(Enum value)
        {
            var enumName = value.ToString();

            for (var i = 0; i < m_values.Length; i++)
            {
                if (m_values[i].enumName == enumName)
                {
                    return m_values[i].displayName;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        internal override bool Validate()
        {
            var valid = base.Validate();

            if (Type == null)
            {
                Debug.LogError($"Setting \"{name}\" needs an enum type assigned!");
                valid = false;
            }
            else
            {
                for (var i = 0; i < m_values.Length; i++)
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

        /// <inheritdoc cref="Setting{T}.RegisterChangeHandler(Action{T})"/>
        public void RegisterChangeHandler<T>(Action<T> callback) where T : Enum
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var action = (Action<Enum>)(value =>
            {
                callback?.Invoke(GetTypedValue<T>());
            });

            m_changeHandlers[callback] = action;
            TypedValueChanged += action;
            action(Value);
        }

        /// <inheritdoc cref="Setting{T}.DeregisterChangeHandler(Action{T})"/>
        public void DeregisterChangeHandler<T>(Action<T> callback) where T : Enum
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (m_changeHandlers.TryGetValue(callback, out var action))
            {
                m_changeHandlers.Remove(callback);
                TypedValueChanged -= action;
            }
        }

#if UNITY_EDITOR
        /// <inheritdoc/>
        internal override string OnInspectorGUI(Rect pos, string serializedValue)
        {
            if (Type != null)
            {
                var values = Enum.GetValues(Type);

                Deserialize(serializedValue, out var deserialized);
                var oldSelected = Mathf.Max(0, Array.IndexOf(values, deserialized));
                var newSelected = Mathf.Max(0, UnityEditor.EditorGUI.Popup(pos, oldSelected, DisplayValues));
                var selected = (Enum)values.GetValue(newSelected);

                return Serialize(Sanitize(selected));
            }
            else
            {
                UnityEditor.EditorGUI.LabelField(pos, "Assign enum type");
                return default;
            }
        }
#endif
    }
}
