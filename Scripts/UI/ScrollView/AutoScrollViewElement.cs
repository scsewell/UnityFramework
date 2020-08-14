using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.UI
{
    /// <summary>
    /// A component placed on selectable items in a scroll view to enable
    /// automatically scrolling to the element when it is selected.
    /// </summary>
    /// <seealso cref="AutoScrollView"/>
    public class AutoScrollViewElement : MonoBehaviour, ISelectHandler
    {
        private AutoScrollView m_scrollView;

        public void OnSelect(BaseEventData eventData)
        {
            if (HasReferences())
            {
                m_scrollView.CaptureEventSystem();
            }
        }

        private bool HasReferences()
        {
            if (m_scrollView == null)
            {
                m_scrollView = this.GetComponentInParentAny<AutoScrollView>();
            }
            if (m_scrollView == null)
            {
                Debug.LogError($"{nameof(AutoScrollViewElement)} \"{name}\" is not a child of a {nameof(AutoScrollView)}!");
                return false;
            }
            return true;
        }
    }
}
