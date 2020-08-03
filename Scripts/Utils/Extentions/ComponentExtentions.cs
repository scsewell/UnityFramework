using UnityEngine;

namespace Framework
{
    /// <summary>
    /// A class containing <see cref="Component"/> extention methods.
    /// </summary>
    public static class ComponentExtentions
    {
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.GetOrAddComponent<T>();
        }

        public static T GetComponentInParentAny<T>(this Component component) where T : Component
        {
            return component.gameObject.GetComponentInParentAny<T>();
        }
    }
}
