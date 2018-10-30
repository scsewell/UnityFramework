namespace Framework.Pooling
{
    public class UnityObjectPool<T> : Pool<T> where T : UnityEngine.Object
    {
        private readonly T m_prefab = null;

        public UnityObjectPool(T prefab, int initialSize = 0) : base(initialSize)
        {
            m_prefab = prefab;
        }
        
        protected override T CreatePooledInstance()
        {
            return UnityEngine.Object.Instantiate(m_prefab);
        }
    }
}
