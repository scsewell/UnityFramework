using UnityEngine;

namespace Framework
{
    /// <summary>
    /// We use a non-generic base class for the singleton to pick up the 
    /// unity events.
    /// </summary>
    public abstract class ComponentSingletonBase : MonoBehaviour
    {
        protected static bool IsQuitting { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            IsQuitting = false;
        }

        protected virtual void OnApplicationQuit()
        {
            IsQuitting = true;
        }
    }

    /// <summary>
    /// Implements singletom behavior for Unity MonoBehaviors.
    /// </summary>
    /// <typeparam name="T">The type of the subclass.</typeparam>
    public abstract class ComponentSingleton<T> : ComponentSingletonBase where T : ComponentSingleton<T>
    {
        private static T m_instance;
        
        /// <summary>
        /// The singleton instance of this class.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<T>();
                    
                    // Only create a new instance if not quitting so the instance
                    // reference will not persist when fast play mode is enabled.
                    if (m_instance == null && !IsQuitting)
                    {
                        m_instance = new GameObject(typeof(T).Name + " (Generated)").AddComponent<T>();
                        DontDestroyOnLoad(m_instance.gameObject);
                    }
                }
                return m_instance;
            }
        }

        protected virtual void Awake()
        {
            if (m_instance != null)
            {
                Debug.LogWarning($"An extra instance of singleton type {typeof(T).FullName} was found! It will be destroyed.");
                DestroyImmediate(this);
            }
            else
            {
                m_instance = this as T;
                DontDestroyOnLoad(m_instance);
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_instance == this)
            {
                m_instance = null;
            }
        }
    }
}
