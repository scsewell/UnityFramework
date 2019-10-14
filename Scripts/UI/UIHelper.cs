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
            rt.localScale = Vector2.one;
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
            rt.localScale = Vector2.one;
            return component;
        }

        public static RectTransform AddSpacer(Transform parent, float height)
        {
            RectTransform rt = Create(parent, "spacer");
            LayoutElement le = rt.gameObject.AddComponent<LayoutElement>();
            le.minHeight = height;
            return rt;
        }

        public static Selectable[] SetNavigationVertical(Transform parent, Selectable above, Selectable below, Selectable left, Selectable right, bool allowDisabled = false)
        {
            Selectable[] selectables = parent
                .Cast<Transform>()
                .Select(t => t.GetComponentInChildren<Selectable>())
                .Where(s => s != null && (allowDisabled || s.isActiveAndEnabled))
                .ToArray();
            
            Navigation explicitNav = new Navigation();
            explicitNav.mode = Navigation.Mode.Explicit;
            explicitNav.selectOnLeft = left;
            explicitNav.selectOnRight = right;

            for (int i = 0; i < selectables.Length; i++)
            {
                Selectable current = selectables[i];

                explicitNav.selectOnUp = (i == 0) ? above : selectables[i - 1];
                explicitNav.selectOnDown = (i == selectables.Length - 1) ? below : selectables[i + 1];

                current.navigation = explicitNav;
            }

            Navigation tempNav;

            if (above != null)
            {
                tempNav = above.navigation;
                tempNav.selectOnDown = selectables.First();
                above.navigation = tempNav;
            }

            if (below != null)
            {
                tempNav = below.navigation;
                tempNav.selectOnUp = selectables.Last();
                below.navigation = tempNav;
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
                tempNav.selectOnLeft = selectables.First();
                right.navigation = tempNav;
            }

            return selectables;
        }
    }
}