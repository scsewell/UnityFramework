#if UNITY_EDITOR
#define USE_PARENT
#endif

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public class ObjectPool
    {
        private List<PooledObject> m_pooled = new List<PooledObject>();
        private List<PooledObject> m_released = new List<PooledObject>();
        private PooledObject m_prefab;
        private int m_initialSize;
#if USE_PARENT
        private Transform m_poolRoot;
#endif

        public ObjectPool(PooledObject prefab, int initialSize)
        {
            m_prefab = prefab;
            m_initialSize = initialSize;
            Initialize();
        }

#if USE_PARENT
        private void SetUpPoolRoot()
        {
            if (m_poolRoot == null)
            {
                m_poolRoot = new GameObject(m_prefab.name + " Pool").transform;
                m_poolRoot.hierarchyCapacity = m_initialSize;
            }
        }
#endif

        public void Initialize()
        {
#if USE_PARENT
            SetUpPoolRoot();
#endif
            int instanceCount = m_initialSize - m_pooled.Count - m_released.Count;
            for (int i = 0; i < instanceCount; i++)
            {
#if USE_PARENT
                Deactivate(CreateInstance(Vector3.zero, Quaternion.identity, m_poolRoot));
#else
                Deactivate(CreateInstance(Vector3.zero, Quaternion.identity, null));
#endif
            }
        }

        public PooledObject GetInstance(Vector3 position, Quaternion rotation, Transform parent)
        {
            PooledObject obj;
            if (m_pooled.Count > 0)
            {
                obj = m_pooled.First();
                m_pooled.RemoveAt(0);

                obj.transform.SetParent(parent);
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            else
            {
                obj = CreateInstance(position, rotation, parent);
            }
            obj.IsReleased = true;
            m_released.Add(obj);
            obj.gameObject.SetActive(true);
            return obj;
        }

        private PooledObject CreateInstance(Vector3 position, Quaternion rotation, Transform parent)
        {
            PooledObject obj = Object.Instantiate(m_prefab, position, rotation, parent);
            obj.SetPool(this);
            return obj;
        }

        public void Deactivate(PooledObject pooledObject)
        {
#if USE_PARENT
            SetUpPoolRoot();
            pooledObject.transform.SetParent(m_poolRoot);
#else
            pooledObject.transform.SetParent(null);
#endif
            pooledObject.gameObject.SetActive(false);
            m_pooled.Add(pooledObject);
            m_released.Remove(pooledObject);
        }

        public void ClearPool()
        {
            while (m_pooled.Count > 0)
            {
                PooledObject obj = m_pooled.First();
                m_pooled.RemoveAt(0);
                Object.Destroy(obj.gameObject);
            }
            m_released.Clear();
        }
    }
}