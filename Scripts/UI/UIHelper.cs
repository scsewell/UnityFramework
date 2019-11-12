using System.Collections.Generic;
using System.Linq;

using UnityEngine;
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

        public static T Create<T>(T prefab, Transform parent) where T : Component
        {
            T component = Object.Instantiate(prefab, parent);

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

        public static RectTransform AddSpacer(Transform parent, float height)
        {
            RectTransform rt = Create(parent, "spacer");
            LayoutElement le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            return rt;
        }

        /// <summary>
        /// Configues the navigation between a group of horizontally arranged selectables.
        /// </summary>
        /// <param name="parent">The parent of the horizontal selectable group.</param>
        /// <param name="up">The selectable to nativate to above the group.</param>
        /// <param name="down">The selectable to nativate to below the group.</param>
        /// <param name="left">The selectable to nativate to the left of the group.</param>
        /// <param name="right">The selectable to nativate to the right of the group.</param>
        /// <param name="verticalSelect">The selectable in the group to select when navigating
        /// from the above or below selectables. By default this is the first selectable.</param>
        /// <param name="allowDisabled">Configure navigation to disabled selectables in the
        /// group.</param>
        /// <returns>The horizontal selectable group.</returns>
        public static List<Selectable> SetNavigationHorizontal(
            Transform parent,
            Selectable up,
            Selectable down,
            Selectable left,
            Selectable right,
            Selectable verticalSelect = null,
            bool allowDisabled = false
        )
        {
            var selectables = GetSelectables(parent, allowDisabled);

            Navigation tempNav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = up,
                selectOnDown = down
            };

            for (int i = 0; i < selectables.Count; i++)
            {
                Selectable current = selectables[i];

                tempNav.selectOnLeft = (i == 0) ? left : selectables[i - 1];
                tempNav.selectOnRight = (i == selectables.Count - 1) ? right : selectables[i + 1];

                current.navigation = tempNav;
            }

            if (up != null)
            {
                tempNav = up.navigation;
                tempNav.selectOnDown = verticalSelect ?? selectables.First();
                up.navigation = tempNav;
            }
            if (down != null)
            {
                tempNav = down.navigation;
                tempNav.selectOnUp = verticalSelect ?? selectables.First();
                down.navigation = tempNav;
            }
            if (left != null)
            {
                tempNav = left.navigation;
                tempNav.selectOnRight = selectables.First();
                left.navigation = tempNav;
            }
            if (right != null)
            {
                tempNav = right.navigation;
                tempNav.selectOnLeft = selectables.Last();
                right.navigation = tempNav;
            }

            return selectables;
        }

        /// <summary>
        /// Configues the navigation between a group of vertically arranged selectables.
        /// </summary>
        /// <param name="parent">The parent of the vertical selectable group.</param>
        /// <param name="up">The selectable to nativate to above the group.</param>
        /// <param name="down">The selectable to nativate to below the group.</param>
        /// <param name="left">The selectable to nativate to the left of the group.</param>
        /// <param name="right">The selectable to nativate to the right of the group.</param>
        /// <param name="horizontalSelect">The selectable in the group to select when navigating
        /// from the left or right selectables. By default this is the first selectable.</param>
        /// <param name="allowDisabled">Configure navigation to disabled selectables in the
        /// group.</param>
        /// <returns>The vertical selectable group.</returns>
        public static List<Selectable> SetNavigationVertical(
            Transform parent,
            Selectable up,
            Selectable down,
            Selectable left,
            Selectable right,
            Selectable horizontalSelect = null,
            bool allowDisabled = false
        )
        {
            var selectables = GetSelectables(parent, allowDisabled);

            Navigation tempNav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = left,
                selectOnRight = right
            };

            for (int i = 0; i < selectables.Count; i++)
            {
                Selectable current = selectables[i];

                tempNav.selectOnUp = (i == 0) ? up : selectables[i - 1];
                tempNav.selectOnDown = (i == selectables.Count - 1) ? down : selectables[i + 1];

                current.navigation = tempNav;
            }

            if (up != null)
            {
                tempNav = up.navigation;
                tempNav.selectOnDown = selectables.First();
                up.navigation = tempNav;
            }
            if (down != null)
            {
                tempNav = down.navigation;
                tempNav.selectOnUp = selectables.Last();
                down.navigation = tempNav;
            }
            if (left != null)
            {
                tempNav = left.navigation;
                tempNav.selectOnRight = horizontalSelect ?? selectables.First();
                left.navigation = tempNav;
            }
            if (right != null)
            {
                tempNav = right.navigation;
                tempNav.selectOnLeft = horizontalSelect ?? selectables.First();
                right.navigation = tempNav;
            }

            return selectables;
        }
        
        private static List<Selectable> GetSelectables(Transform parent, bool allowDisabled)
        {
            List<Selectable> selectables = new List<Selectable>();

            for (int i = 0; i < parent.childCount; i++)
            {
                var selectable = parent.GetChild(i).GetComponent<Selectable>();

                if (selectable != null && (allowDisabled || (selectable.isActiveAndEnabled && selectable.interactable)))
                {
                    selectables.Add(selectable);
                }
            }

            return selectables;
        }
    }
}