using UnityEngine;

namespace Framework
{
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
