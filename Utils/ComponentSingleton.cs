using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Implements singletom behavior for Unity MonoBehaviors.
    /// </summary>
    /// <typeparam name="T">The type of the subclass.</typeparam>
    public class ComponentSingleton<T> : MonoBehaviour where T : ComponentSingleton<T>
    {
        private static object m_lock = new object();
        private static bool m_isQuitting = false;

        private static T m_instance;
        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (m_lock)
                    {
                        if (m_instance == null)
                        {
                            m_instance = FindObjectOfType<T>();
                        }

                        if (m_instance == null && !m_isQuitting)
                        {
                            m_instance = new GameObject(typeof(T).Name + " (Generated)").AddComponent<T>();
                        }
                    }
                }
                return m_instance;
            }
        }

        protected virtual void Awake()
        {
            if (m_instance != null)
            {
                Debug.LogWarning("An extra instance of singleton type " + typeof(T) + " was found! It will be destroyed.");
                DestroyImmediate(this);
            }
            else
            {
                m_instance = (T)this;
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_instance == this)
            {
                m_instance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            m_isQuitting = true;
        }
    }
}
