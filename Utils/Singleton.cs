using System;

namespace Framework
{
    /// <summary>
    /// Implements a generic singleton. While it is possible to instantiate multiples of the subclass,
    /// an exception will be thrown.
    /// </summary>
    /// <typeparam name="T">The type of the subclass.</typeparam>
    public class Singleton<T> where T : class, new()
    {
        private static object m_lock = new object();
        
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
                            m_instance = new T();
                        }
                    }
                }
                return m_instance;
            }
        }

        protected Singleton()
        {
            if (m_instance != null)
            {
                throw new InvalidOperationException("Attempted to create another instance of singleton " + typeof(T).Name + "!");
            }
        }
    }
}
