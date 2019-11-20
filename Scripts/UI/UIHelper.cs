using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.UI
{
    public static class UIHelper
    {
        public static RectTransform Create(Transform parent, string name = "New UI Element")
        {
            RectTransform rt = new GameObject(name).AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
            return rt;
        }

        public static T Create<T>(T prefab, Transform parent, string name = "") where T : Component
        {
            T component = Object.Instantiate(prefab, parent);

            if (!string.IsNullOrWhiteSpace(name))
            {
                component.gameObject.name = name;
            }

            RectTransform rt = component.GetComponent<RectTransform>();
            if (rt == null)
            {
                rt = component.gameObject.AddComponent<RectTransform>();
            }
            
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
            return component;
        }

        public static RectTransform AddSpacer(Transform parent, float size)
        {
            RectTransform rect = Create(parent, "spacer");
            LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = size;
            layout.minWidth = size;
            return rect;
        }

        /// <summary>
        /// Configues the navigation between a group of horizontally arranged selectables.
        /// </summary>
        /// <param name="config">The configutation options.</param>
        /// <returns>The horizontal selectable group.</returns>
        public static List<Selectable> SetNavigationHorizontal(NavConfig config)
        {
            // get the selectable group
            List<Selectable> selectables = GetSelectables(config);

            if (selectables.Count == 0)
            {
                return selectables;
            }

            Selectable first = selectables[0];
            Selectable last = selectables[selectables.Count - 1];

            // configure navigation in the group
            Navigation tempNav = new Navigation
            {
                selectOnUp = config.up,
                selectOnDown = config.down,
            };

            for (int i = 0; i < selectables.Count; i++)
            {
                Selectable current = selectables[i];

                if (i == 0)
                {
                    if (config.left != null)
                    {
                        tempNav.selectOnLeft = config.left;
                    }
                    else if (config.wrap)
                    {
                        tempNav.selectOnLeft = FindLastSelectableInChain(last, selectables, MoveDirection.Right);
                    }
                    else
                    {
                        tempNav.selectOnLeft = null;
                    }
                }
                else
                {
                    tempNav.selectOnLeft = selectables[i - 1];
                }

                if (i == selectables.Count - 1)
                {
                    if (config.right != null)
                    {
                        tempNav.selectOnRight = config.right;
                    }
                    else if (config.wrap)
                    {
                        tempNav.selectOnRight = FindLastSelectableInChain(first, selectables, MoveDirection.Left);
                    }
                    else
                    {
                        tempNav.selectOnRight = null;
                    }
                }
                else
                {
                    tempNav.selectOnRight = selectables[i + 1];
                }

                current.navigation = tempNav;
            }

            // configure navigation to the group
            if (config.up != null)
            {
                tempNav = config.up.navigation;
                tempNav.selectOnDown = config.verticalSelect != null ? config.verticalSelect : first;
                config.up.navigation = tempNav;
            }

            if (config.down != null)
            {
                tempNav = config.down.navigation;
                tempNav.selectOnUp = config.verticalSelect != null ? config.verticalSelect : first;
                config.down.navigation = tempNav;
            }

            if (config.left != null)
            {
                tempNav = config.left.navigation;
                tempNav.selectOnRight = first;
                config.left.navigation = tempNav;
            }
            else if (config.wrap)
            {
                Selectable wrap = FindLastSelectableInChain(last, selectables, MoveDirection.Right);

                tempNav = wrap.navigation;
                tempNav.selectOnRight = first;
                wrap.navigation = tempNav;
            }

            if (config.right != null)
            {
                tempNav = config.right.navigation;
                tempNav.selectOnLeft = last;
                config.right.navigation = tempNav;
            }
            else if (config.wrap)
            {
                Selectable wrap = FindLastSelectableInChain(first, selectables, MoveDirection.Left);

                tempNav = wrap.navigation;
                tempNav.selectOnLeft = last;
                wrap.navigation = tempNav;
            }

            return selectables;
        }

        /// <summary>
        /// Configues the navigation between a group of vertically arranged selectables.
        /// </summary>
        /// <param name="config">The configutation options.</param>
        /// <returns>The vertical selectable group.</returns>
        public static List<Selectable> SetNavigationVertical(NavConfig config)
        {
            // get the selectable group
            List<Selectable> selectables = GetSelectables(config);

            if (selectables.Count == 0)
            {
                return selectables;
            }

            Selectable first = selectables[0];
            Selectable last = selectables[selectables.Count - 1];

            // configure navigation in the group
            Navigation tempNav = new Navigation
            {
                selectOnLeft = config.left,
                selectOnRight = config.right,
            };

            for (int i = 0; i < selectables.Count; i++)
            {
                Selectable current = selectables[i];

                if (i == 0)
                {
                    if (config.up != null)
                    {
                        tempNav.selectOnUp = config.up;
                    }
                    else if (config.wrap)
                    {
                        tempNav.selectOnUp = FindLastSelectableInChain(last, selectables, MoveDirection.Down);
                    }
                    else
                    {
                        tempNav.selectOnUp = null;
                    }
                }
                else
                {
                    tempNav.selectOnUp = selectables[i - 1];
                }

                if (i == selectables.Count - 1)
                {
                    if (config.down != null)
                    {
                        tempNav.selectOnDown = config.down;
                    }
                    else if (config.wrap)
                    {
                        tempNav.selectOnDown = FindLastSelectableInChain(first, selectables, MoveDirection.Up);
                    }
                    else
                    {
                        tempNav.selectOnDown = null;
                    }
                }
                else
                {
                    tempNav.selectOnDown = selectables[i + 1];
                }

                current.navigation = tempNav;
            }

            // configure navigation to the group
            if (config.up != null)
            {
                tempNav = config.up.navigation;
                tempNav.selectOnDown = first;
                config.up.navigation = tempNav;
            }
            else if (config.wrap)
            {
                Selectable wrap = FindLastSelectableInChain(last, selectables, MoveDirection.Down);

                tempNav = wrap.navigation;
                tempNav.selectOnDown = first;
                wrap.navigation = tempNav;
            }

            if (config.down != null)
            {
                tempNav = config.down.navigation;
                tempNav.selectOnUp = last;
                config.down.navigation = tempNav;
            }
            else if (config.wrap)
            {
                Selectable wrap = FindLastSelectableInChain(first, selectables, MoveDirection.Up);

                tempNav = wrap.navigation;
                tempNav.selectOnUp = last;
                wrap.navigation = tempNav;
            }

            if (config.left != null)
            {
                tempNav = config.left.navigation;
                tempNav.selectOnRight = config.horizontalSelect != null ? config.horizontalSelect : first;
                config.left.navigation = tempNav;
            }

            if (config.right != null)
            {
                tempNav = config.right.navigation;
                tempNav.selectOnLeft = config.horizontalSelect != null ? config.horizontalSelect : first;
                config.right.navigation = tempNav;
            }

            return selectables;
        }
        
        private static List<Selectable> GetSelectables(NavConfig config)
        {
            List<Selectable> selectables = new List<Selectable>();

            for (int i = 0; i < config.parent.childCount; i++)
            {
                var selectable = config.parent.GetChild(i).GetComponent<Selectable>();

                if (selectable != null && (config.allowDisabled || (selectable.isActiveAndEnabled && selectable.interactable)))
                {
                    selectables.Add(selectable);
                }
            }

            return selectables;
        }

        private static readonly HashSet<Selectable> s_visited = new HashSet<Selectable>();

        /// <summary>
        /// Follows a navigation chain to the last selectable. If there are cycles in the 
        /// navigation graph, returns the first selectable which contains a link back to a 
        /// selectable earlier in the chain.
        /// first selectable which
        /// </summary>
        /// <param name="selectable">The selectable to start from.</param>
        /// <param name="exclude">The selectables which are not to be included in the search.</param>
        /// <param name="dir">The direction to navigate along.</param>
        /// <returns>The last selectable in the chian.</returns>
        public static Selectable FindLastSelectableInChain(Selectable selectable, IEnumerable<Selectable> exclude, MoveDirection dir)
        {
            s_visited.Clear();
            foreach (Selectable excluded in exclude)
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
                    case MoveDirection.Up:      nextSelectable = selectable.navigation.selectOnUp;      break;
                    case MoveDirection.Down:    nextSelectable = selectable.navigation.selectOnDown;    break;
                    case MoveDirection.Left:    nextSelectable = selectable.navigation.selectOnLeft;    break;
                    case MoveDirection.Right:   nextSelectable = selectable.navigation.selectOnRight;   break;
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
        /// align with the given move direction.
        /// </summary>
        /// <param name="from">The rect moved from.</param>
        /// <param name="to">The rect moved to.</param>
        /// <param name="dir">The move direction.</param>
        /// <returns>True if the directions are opposite.</returns>
        public static bool Wraps(Component from, Component to, MoveDirection dir)
        {
            RectTransform fRect = from.GetComponent<RectTransform>();
            RectTransform tRect = to.GetComponent<RectTransform>();

            Vector3 toRectCenterWorld = tRect.transform.TransformPoint(tRect.rect.center);
            Vector2 toRectCenterLocal = fRect.InverseTransformPoint(toRectCenterWorld);

            Vector2 delta = toRectCenterLocal - fRect.rect.center;

            switch (dir)
            {
                case MoveDirection.Up:      return delta.y < 0f;
                case MoveDirection.Down:    return delta.y > 0f;
                case MoveDirection.Left:    return delta.x > 0f;
                case MoveDirection.Right:   return delta.x < 0f;
            }

            return false;
        }
    }

    /// <summary>
    /// A configuration used for the auto navigation builder utilites.
    /// </summary>
    public struct NavConfig
    {
        /// <summary>
        /// The parent of the selectable group.
        /// </summary>
        public Transform parent;

        /// <summary>
        /// Allow navigation to disabled selectables in the group.
        /// </summary>
        public bool allowDisabled;

        /// <summary>
        /// The selectable to nativate to above the group.
        /// </summary>
        public Selectable up;
        /// <summary>
        /// The selectable to nativate to below the group.
        /// </summary>
        public Selectable down;
        /// <summary>
        /// The selectable to nativate to the left of the group.
        /// </summary>
        public Selectable left;
        /// <summary>
        /// /The selectable to nativate to the right of the group.
        /// </summary>
        public Selectable right;

        /// <summary>
        /// The selectable in the selectable group to select when navigating from
        /// selectables to the left or right when configuring vertical navigation.
        /// By default this is the first selectable in the group.
        /// </summary>
        public Selectable horizontalSelect;
        /// <summary>
        /// The selectable in the selectable group to select when navigating from
        /// selectables to the top or bottom when configuring horizontal navigation.
        /// By default this is the first selectable in the group.
        /// </summary>
        public Selectable verticalSelect;

        /// <summary>
        /// Allow navigation between the first and last selectable. If this config
        /// uses the up/down/left/right selectables, we follow along their navigation
        /// chain in the relevant direction and wrap to the last avaiable element.
        /// </summary>
        public bool wrap;
    }
}