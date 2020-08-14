using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.UI
{
    /// <summary>
    /// A component used to make a <see cref="ScrollRect"/> autoscroll to the selected item.
    /// </summary>
    public class AutoScrollView : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The parent transform of this scroll view for which any child that becomes selected and is not in the scroll view causes the scroll position to reset.")]
        private Transform m_selectionGroup = null;

        [SerializeField]
        [Tooltip("The amount of space between the selected item and the top/bottom of the scroll view.")]
        [Range(0f, 0.5f)]
        private float m_margin = 0.3f;

        [SerializeField]
        [Tooltip("The amount of smoothing applied when auto-scrolling.")]
        [Range(0f, 1f)]
        private float m_smoothing = 0f;


        private ScrollRect m_scrollRect;
        private float m_targetScrollPos;
        private bool m_snap;
        private EventSystem m_eventSystem;
        private GameObject m_lastSelected;


        private void Awake()
        {
            m_scrollRect = GetComponentInChildren<ScrollRect>(true);
        }

        private void OnEnable()
        {
            m_eventSystem = null;
            m_lastSelected = null;

            SnapToPosition(m_scrollRect.normalizedPosition.y);
        }

        private void LateUpdate()
        {
            UpdateScrollTarget();

            float scrollPos;
            if (m_snap || m_smoothing == 0f)
            {
                m_snap = false;
                scrollPos = m_targetScrollPos;
            }
            else
            {
                scrollPos = MathUtils.Damp(m_scrollRect.normalizedPosition.y, m_targetScrollPos, m_smoothing);
            }

            m_scrollRect.normalizedPosition = new Vector2(0f, scrollPos);
        }

        /// <summary>
        /// Sets the scroll position.
        /// </summary>
        /// <param name="normalizedPosition">The scroll position, where 0 is the bottom
        /// and 1 is the top.</param>
        public void SnapToPosition(float normalizedPosition)
        {
            m_targetScrollPos = normalizedPosition;
            m_snap = true;
        }

        /// <summary>
        /// Change the scroll view to use the selection of the currently executing event system.
        /// </summary>
        internal void CaptureEventSystem()
        {
            m_eventSystem = EventSystem.current;
        }

        private void UpdateScrollTarget()
        {
            // We only need to update the scroll target if the selection has changed since we
            // last updated and the selected element is either an element in the scroll view
            // or an transform under the selection group.
            if (m_eventSystem == null)
            {
                SnapToPosition(1f);
                return;
            }

            var lastSelected = m_lastSelected;
            var selected = m_eventSystem.currentSelectedGameObject;
            m_lastSelected = selected;

            if (selected == null || selected == lastSelected)
            {
                return;
            }

            var rt = selected.GetComponent<RectTransform>();

            if (rt == null || !rt.IsChildOf(m_scrollRect.content))
            {
                if (m_selectionGroup != null && rt.IsChildOf(m_selectionGroup))
                {
                    SnapToPosition(1f);
                }
                return;
            }

            // find the bounds of the currently visible area
            var contentHeight = m_scrollRect.content.rect.height;
            var viewportHeight = m_scrollRect.viewport.rect.height;
            var excessHeight = contentHeight - viewportHeight;

            var viewBottom = (excessHeight * m_targetScrollPos) - contentHeight;
            var viewTop = viewBottom + viewportHeight;

            // find the bounds of the selected item
            var itemMiddle = rt.localPosition.y;
            var itemHalfHeight = rt.rect.height / 2f;

            var itemTop = itemMiddle + itemHalfHeight;
            var itemBottom = itemMiddle - itemHalfHeight;

            // constrain how close to the edge of the viewport the selected item can be
            var margin = Mathf.Min(viewportHeight * m_margin, (viewportHeight / 2f) - itemHalfHeight);

            var adjustedTop = itemTop + margin;
            var adjustedBottom = itemBottom - margin;

            float desiredViewBottom;
            if (adjustedTop > viewTop)
            {
                desiredViewBottom = adjustedTop - viewportHeight;
            }
            else if (adjustedBottom < viewBottom)
            {
                desiredViewBottom = adjustedBottom;
            }
            else
            {
                return;
            }

            // find the normalized scroll position that shows the desired view
            m_targetScrollPos = Mathf.Clamp01((desiredViewBottom + contentHeight) / excessHeight);
        }
    }
}