using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.UI
{
    /// <summary>
    /// A class contanining utility methods for UGUI elements.
    /// </summary>
    public static class UIHelper
    {
        private static FieldInfo s_EventSystem;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            s_EventSystem = null;
        }


        /// <summary>
        /// Creates a child RectTransform under an object with an identity local transform.
        /// </summary>
        /// <param name="parent">The parent transform.</param>
        /// <param name="name">The name the instantiated child object will use.</param>
        /// <returns>The instantiated RectTransform.</returns>
        public static RectTransform Create(Transform parent, string name = "New UI Element")
        {
            var rt = new GameObject(name).AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
            return rt;
        }

        /// <summary>
        /// Instantaites a prefab under a parent object with an identity local transform.
        /// </summary>
        /// <typeparam name="T">A type of component.</typeparam>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="parent">The parent transform.</param>
        /// <param name="name">The name the instantiated child object will use.</param>
        /// <returns>The new instance.</returns>
        public static T Create<T>(T prefab, Transform parent, string name = "") where T : Component
        {
            var component = Object.Instantiate(prefab, parent);

            if (!string.IsNullOrWhiteSpace(name))
            {
                component.gameObject.name = name;
            }

            if (!component.TryGetComponent<RectTransform>(out var rt))
            {
                rt = component.gameObject.AddComponent<RectTransform>();
            }

            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
            return component;
        }

        /// <summary>
        /// Creates a RectTransform with a <see cref="LayoutElement"/> that can separate
        /// adjacent elements in a layout group.
        /// </summary>
        /// <param name="parent">The parent transform.</param>
        /// <param name="size">The size of the space to add.</param>
        /// <returns>The spacer RectTransform.</returns>
        public static RectTransform AddSpacer(Transform parent, float size)
        {
            var rect = Create(parent, "spacer");
            var layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = size;
            layout.minWidth = size;
            return rect;
        }

        private static readonly HashSet<Selectable> s_visited = new HashSet<Selectable>();

        /// <summary>
        /// Follows a navigation chain to the last selectable.
        /// </summary>
        /// <remarks>
        /// If there are cycles in the navigation graph, returns the first selectable which contains
        /// a link back to a selectable earlier in the chain.
        /// </remarks>
        /// <param name="selectable">The selectable to start from.</param>
        /// <param name="exclude">The selectables which are not to be included in the search.</param>
        /// <param name="dir">The direction to navigate along.</param>
        /// <returns>The last selectable in the chian, or null if <paramref name="selectable"/> is null.</returns>
        public static Selectable FindLastSelectableInChain(Selectable selectable, IEnumerable<Selectable> exclude, MoveDirection dir)
        {
            s_visited.Clear();
            foreach (var excluded in exclude)
            {
                s_visited.Add(excluded);
            }

            while (selectable != null)
            {
                s_visited.Add(selectable);

                // try to find the next navigable in the given direction
                Selectable nextSelectable = null;

                switch (dir)
                {
                    case MoveDirection.Up: nextSelectable = selectable.navigation.selectOnUp; break;
                    case MoveDirection.Down: nextSelectable = selectable.navigation.selectOnDown; break;
                    case MoveDirection.Left: nextSelectable = selectable.navigation.selectOnLeft; break;
                    case MoveDirection.Right: nextSelectable = selectable.navigation.selectOnRight; break;
                }

                // check if we found the end of the navigation chain
                if (nextSelectable == null)
                {
                    break;
                }

                // make sure we haven't cycled back
                if (s_visited.Contains(nextSelectable))
                {
                    break;
                }

                // continue on from the next selectable in the chain
                selectable = nextSelectable;
            }

            return selectable;
        }

        /// <summary>
        /// Checks if moving from one rect transform to another does not
        /// align with a given move direction.
        /// </summary>
        /// <param name="from">The rect moved from.</param>
        /// <param name="to">The rect moved to.</param>
        /// <param name="dir">The move direction.</param>
        /// <returns>True if the directions are opposite.</returns>
        public static bool Wraps(Component from, Component to, MoveDirection dir)
        {
            var fRect = from.GetComponent<RectTransform>();
            var tRect = to.GetComponent<RectTransform>();

            var toRectCenterWorld = tRect.transform.TransformPoint(tRect.rect.center);
            var toRectCenterLocal = (Vector2)fRect.InverseTransformPoint(toRectCenterWorld);

            var delta = toRectCenterLocal - fRect.rect.center;

            switch (dir)
            {
                case MoveDirection.Up: return delta.y < 0f;
                case MoveDirection.Down: return delta.y > 0f;
                case MoveDirection.Left: return delta.x > 0f;
                case MoveDirection.Right: return delta.x < 0f;
            }

            return false;
        }

        /// <summary>
        /// Gets the event system that sent this event.
        /// </summary>
        /// <param name="eventData">The event data to get the source of.</param>
        /// <returns>The event system.</returns>
        public static EventSystem GetEventSystem(this BaseEventData eventData)
        {
            if (s_EventSystem == null)
            {
                s_EventSystem = typeof(BaseEventData).GetField("m_EventSystem", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            return s_EventSystem.GetValue(eventData) as EventSystem;
        }
    }
}