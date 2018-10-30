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

        public static Selectable[] SetNavigationVertical(Transform parent, Navigation startNav, Navigation middleNav, Navigation endNav)
        {
            Selectable[] selectables = parent.Cast<Transform>().Select(t => t.GetComponentInChildren<Selectable>()).Where(s => s != null).ToArray();

            for (int i = 0; i < selectables.Length; i++)
            {
                Selectable current = selectables[i];

                if (i == 0)
                {
                    startNav.selectOnDown = selectables[i + 1];
                    current.navigation = startNav;
                }
                else if (i == selectables.Length - 1)
                {
                    endNav.selectOnUp = selectables[i - 1];
                    current.navigation = endNav;
                }
                else
                {
                    Navigation nav = middleNav;
                    nav.selectOnUp = selectables[i - 1];
                    nav.selectOnDown = selectables[i + 1];
                    current.navigation = nav;
                }
            }
            
            return selectables;
        }
    }
}