using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.UI
{
    /// <summary>
    /// A component used to make a <see cref="ScrollRect"/> autoscroll to the selected item.
    /// </summary>
    public class EventSensitiveScrollRect : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The amount of space between the selected item and the top/bottom of the scroll view.")]
        [Range(0f, 1f)]
        private float m_scrollMargin = 0.3f;

        private ScrollRect m_scrollRect = null;
        private GameObject m_selected = null;

        private void Awake()
        {
            m_scrollRect = GetComponentInChildren<ScrollRect>(true);
        }

        private void LateUpdate()
        {
            var lastSelected = m_selected;
            m_selected = EventSystem.current.currentSelectedGameObject;

            if (m_selected == null || m_selected == lastSelected || !m_selected.transform.IsChildOf(m_scrollRect.content))
            {
                return;
            }

            var rt = m_selected.GetComponent<RectTransform>();

            var contentHeight = m_scrollRect.content.rect.height;
            var viewportHeight = m_scrollRect.viewport.rect.height;

            var centerLine = rt.localPosition.y;
            var upperBound = centerLine + (rt.rect.height / 2f);
            var lowerBound = centerLine - (rt.rect.height / 2f);

            // what are the bounds of the currently visible area?
            var lowerVisible = (contentHeight - viewportHeight) * m_scrollRect.normalizedPosition.y - contentHeight;
            var upperVisible = lowerVisible + viewportHeight;

            // is our item visible right now?
            float desiredLowerBound;
            if (upperBound > upperVisible)
            {
                // need to scroll up to upperBound
                desiredLowerBound = upperBound - viewportHeight + (rt.rect.height * m_scrollMargin);
            }
            else if (lowerBound < lowerVisible)
            {
                // need to scroll down to lowerBound
                desiredLowerBound = lowerBound - (rt.rect.height * m_scrollMargin);
            }
            else
            {
                // item already visible
                return;
            }

            // normalize and set the desired viewport
            var normalizedDesired = Mathf.Clamp01((desiredLowerBound + contentHeight) / (contentHeight - viewportHeight));

            m_scrollRect.normalizedPosition = new Vector2(0f, normalizedDesired);
        }
    }
}