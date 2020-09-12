using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    /// <summary>
    /// A class that manages the navigation for a set of related <see cref="Selectables"/>
    /// that can be navigated between.
    /// </summary>
    public abstract class NavigationBuilder : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The selectable to nativate to above the group.")]
        protected Selectable m_up = null;

        [SerializeField]
        [Tooltip("The selectable to nativate to below the group.")]
        protected Selectable m_down = null;

        [SerializeField]
        [Tooltip("The selectable to nativate to the left of the group.")]
        protected Selectable m_left = null;

        [SerializeField]
        [Tooltip("The selectable to nativate to the right of the group.")]
        protected Selectable m_right = null;

        [SerializeField]
        [Tooltip("Allow navigation between the first and last selectable. " +
            "If this config uses the up/down/left/right selectables, we follow along their navigation chain in the relevant direction and wrap to the last avaiable element.")]
        protected bool m_wrap = true;

        [SerializeField]
        [Tooltip("Allow navigation to disabled selectables in the group.")]
        protected bool m_allowDisabled = false;


        private readonly List<Selectable> m_selectables = new List<Selectable>();

        /// <summary>
        /// The selectable to nativate to above the group.
        /// </summary>
        public Selectable Up
        {
            get => m_up;
            set => m_up = value;
        }

        /// <summary>
        /// The selectable to nativate to below the group.
        /// </summary>
        public Selectable Down
        {
            get => m_down;
            set => m_down = value;
        }

        /// <summary>
        /// The selectable to nativate to the left of the group.
        /// </summary>
        public Selectable Left
        {
            get => m_left;
            set => m_left = value;
        }

        /// <summary>
        /// The selectable to nativate to the right of the group.
        /// </summary>
        public Selectable Right
        {
            get => m_right;
            set => m_right = value;
        }

        /// <summary>
        /// Allow navigation between the first and last selectable.
        /// </summary>
        /// <remarks>
        /// If this config uses the up/down/left/right selectables, we follow along their 
        /// navigation chain in the relevant direction and wrap to the last avaiable element.
        /// </remarks>
        public bool Wrap
        {
            get => m_wrap;
            set => m_wrap = value;
        }

        /// <summary>
        /// Allow navigation to disabled selectables in the group.
        /// </summary>
        public bool AllowDisabled
        {
            get => m_allowDisabled;
            set => m_allowDisabled = value;
        }

        /// <summary>
        /// The selectables in the navigation group.
        /// </summary>
        public IReadOnlyList<Selectable> Selectables => m_selectables;


        private void Start()
        {
            UpdateNavigation();
        }

        /// <summary>
        /// Builds the navigation.
        /// </summary>
        public void UpdateNavigation()
        {
            m_selectables.Clear();

            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if (child.TryGetComponent<Selectable>(out var selectable))
                {
                    m_selectables.Add(selectable);

                    // clear existing navigation or else the algorithm will generate incorrect results
                    selectable.navigation = new Navigation
                    {
                        mode = selectable.navigation.mode,
                        selectOnUp = null,
                        selectOnDown = null,
                        selectOnLeft = null,
                        selectOnRight = null,
                    };
                }
            }

            if (m_selectables.Count > 0)
            {
                OnBuildNavigation(m_selectables);
            }
        }

        /// <summary>
        /// Builds the navigation for the group of selectables to manage.
        /// </summary>
        /// <param name="selectables">The selectables to include in the navigation chain.</param>
        protected abstract void OnBuildNavigation(List<Selectable> selectables);

        /// <summary>
        /// Gets the next selectable that is enabled and can be selected.
        /// </summary>
        /// <param name="startIndex">The index in the selectables array to start at.</param>
        /// <param name="selectable">The next selectable, or null if none was found.</param>
        /// <returns>The index of the returned selectable.</returns>
        protected int FindNextEnabledSelectable(int startIndex, out Selectable selectable)
        {
            for (var i = startIndex; i < m_selectables.Count; i++)
            {
                if (IsEnabled(m_selectables[i]))
                {
                    selectable = m_selectables[i];
                    return i;
                }
            }
            selectable = null;
            return -1;
        }

        /// <summary>
        /// Gets the previous selectable that is enabled and can be selected.
        /// </summary>
        /// <param name="startIndex">The index in the selectables array to start at.</param>
        /// <param name="selectable">The previous selectable, or null if none was found.</param>
        /// <returns>The index of the returned selectable.</returns>
        protected int FindPreviousEnabledSelectable(int startIndex, out Selectable selectable)
        {
            for (var i = startIndex; i >= 0; i--)
            {
                if (IsEnabled(m_selectables[i]))
                {
                    selectable = m_selectables[i];
                    return i;
                }
            }
            selectable = null;
            return -1;
        }

        private bool IsEnabled(Selectable selectable)
        {
            return m_allowDisabled || (selectable.isActiveAndEnabled && selectable.interactable);
        }
    }
}
