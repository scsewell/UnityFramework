using System;
using System.Collections.Generic;

namespace Framework.Pooling
{
    /// <summary>
    /// A generic thread safe object pool.
    /// </summary>
    /// <typeparam name="T">The type of object contained in this pool.</typeparam>
    public abstract class Pool<T> where T : class
    {
        private readonly Queue<T> m_pooled = null;
        private readonly HashSet<T> m_released = null;
        private readonly object m_lock = new object();

        /// <summary>
        /// The number of instances managed by this pool.
        /// </summary>
        public int Size
        {
            get
            {
                lock (m_lock)
                {
                    return m_pooled.Count + m_released.Count;
                }
            }
            set
            {
                lock (m_lock)
                {
                    int sizeDifference = Math.Max(value, 0) - (m_pooled.Count + m_released.Count);
                    if (sizeDifference > 0)
                    {
                        GrowPool(sizeDifference);
                    }
                    else
                    {
                        ShinkPool(-sizeDifference);
                    }
                }
            }
        }
        
        public Pool(int initialSize = 0)
        {
            m_pooled = new Queue<T>(initialSize);
            m_released = new HashSet<T>();
            Size = initialSize;
        }

        /// <summary>
        /// Removes all instances from the pool.
        /// </summary>
        public void ClearPool()
        {
            Size = 0;
        }

        /// <summary>
        /// Gets an unused instance from the pool, growing the pool if there are no instances available.
        /// </summary>
        public virtual T Get()
        {
            T obj;

            lock (m_lock)
            {
                if (m_pooled.Count == 0)
                {
                    GrowPool(1);
                }
                obj = m_pooled.Dequeue();
                m_released.Add(obj);
            }
            return obj;
        }

        /// <summary>
        /// Returns an instance no longer needed to the pool.
        /// </summary>
        /// <param name="obj">The instance returned to the pool.</param>
        public virtual void ReturnToPool(T obj)
        {
            lock (m_lock)
            {
                if (m_released.Remove(obj))
                {
                    m_pooled.Enqueue(obj);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"There was an attempt to return instance \"{obj}\" to a pool it did not originate from.");
                }
            }
        }
        
        /// <summary>
        /// Creates a new instance of the pooled object.
        /// </summary>
        /// <returns>A new poolable instance.</returns>
        protected abstract T CreatePooledInstance();
        
        private void GrowPool(int addCount)
        {
            for (int i = 0; i < addCount; i++)
            {
                T obj = CreatePooledInstance();
                ReturnToPool(obj);
            }
        }

        private void ShinkPool(int removeCount)
        {
            for (int i = 0; i < Math.Min(removeCount, m_pooled.Count); i++)
            {
                DestroyPooledInstance(m_pooled.Dequeue());
            }
        }

        /// <summary>
        /// Handles destroying instances cleared from the pool.
        /// </summary>
        protected virtual void DestroyPooledInstance(T obj) { }
    }
}
