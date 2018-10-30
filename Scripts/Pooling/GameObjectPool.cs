using UnityEngine;

namespace Framework.Pooling
{
    public class GameObjectPool : UnityObjectPool<GameObject>
    {
        public GameObjectPool(GameObject prefab, int initialSize = 0) : base(prefab, initialSize)
        {
        }

        public override GameObject Get()
        {
            GameObject obj = base.Get();
            obj.SetActive(true);
            return obj;
        }

        public override void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            base.ReturnToPool(obj);
        }
    }
}
