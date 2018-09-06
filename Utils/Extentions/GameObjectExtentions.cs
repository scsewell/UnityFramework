using UnityEngine;

namespace Framework
{
    public static class GameObjectExtentions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            if (c != null)
            {
                return c;
            }
            return go.AddComponent<T>();
        }

        public static T GetComponentInParentAny<T>(this GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            if (c != null)
            {
                return c;
            }
            else if (go.transform.parent != null)
            {
                return GetComponentInParentAny<T>(go.transform.parent.gameObject);
            }
            else
            {
                return null;
            }
        }
    }
}
