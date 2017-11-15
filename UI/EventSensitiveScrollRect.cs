using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.UI
{
    public class EventSensitiveScrollRect : MonoBehaviour
    {
        // how much to "overshoot" when scrolling, relative to the selected item's height
        private static float SCROLL_MARGIN = 0.3f;

        private ScrollRect m_sr;

        private GameObject m_selected;

        private void Awake()
        {
            m_sr = GetComponentInChildren<ScrollRect>(true);
        }

        private void Update()
        {
            GameObject lastSelected = m_selected;
            m_selected = EventSystem.current.currentSelectedGameObject;

            if (m_selected == null || m_selected == lastSelected || !m_selected.transform.IsChildOf(m_sr.content))
            {
                return;
            }

            RectTransform rt = null;
            foreach (Transform t in m_sr.content)
            {
                if (m_selected.transform.IsChildOf(t))
                {
                    rt = t.GetComponent<RectTransform>();
                    break;
                }
            }
            float contentHeight = m_sr.content.rect.height;
            float viewportHeight = m_sr.viewport.rect.height;

            // what bounds must be visible?
            float centerLine = rt.localPosition.y; // selected item's center
            float upperBound = centerLine + (rt.rect.height / 2f); // selected item's upper bound
            float lowerBound = centerLine - (rt.rect.height / 2f); // selected item's lower bound

            // what are the bounds of the currently visible area?
            float lowerVisible = (contentHeight - viewportHeight) * m_sr.normalizedPosition.y - contentHeight;
            float upperVisible = lowerVisible + viewportHeight;

            // is our item visible right now?
            float desiredLowerBound;
            if (upperBound > upperVisible)
            {
                // need to scroll up to upperBound
                desiredLowerBound = upperBound - viewportHeight + rt.rect.height * SCROLL_MARGIN;
            }
            else if (lowerBound < lowerVisible)
            {
                // need to scroll down to lowerBound
                desiredLowerBound = lowerBound - rt.rect.height * SCROLL_MARGIN;
            }
            else
            {
                // item already visible
                return;
            }

            // normalize and set the desired viewport
            float normalizedDesired = (desiredLowerBound + contentHeight) / (contentHeight - viewportHeight);
            m_sr.normalizedPosition = new Vector2(0, Mathf.Clamp01(normalizedDesired));
        }
    }
}