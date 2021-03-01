using UnityEngine;

namespace Framework
{
    /// <summary>
    /// A class containing <see cref="GameObject"/> extension methods.
    /// </summary>
    public static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            return go.TryGetComponent<T>(out var c) ? c : go.AddComponent<T>();
        }

        public static T GetComponentInParentAny<T>(this GameObject go) where T : Component
        {
            if (go.TryGetComponent<T>(out var c))
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
