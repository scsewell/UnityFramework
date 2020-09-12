using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.UI
{
    /// <summary>
    /// A <see cref="NavigationBuilder"/> that manages a chain of horizontally placed <see cref="Selectables"/>.
    /// </summary>
    public class HorizontalNavigationBuilder : NavigationBuilder
    {
        [SerializeField]
        [Tooltip("The selectable in the selectable group to select when navigating to the group from the top or bottom when configuring horizontal navigation. " +
            "By default this is the first selectable in the group.")]
        private Selectable m_verticalSelect = null;


        /// <summary>
        /// The selectable in the selectable group to select when navigating to the group from the top or bottom.
        /// </summary>
        /// <remarks>
        /// By default this is the first selectable in the group.
        /// </remarks>
        public Selectable VerticalSelect
        {
            get => m_verticalSelect;
            set => m_verticalSelect = value;
        }


        /// <inheritdoc/>
        protected override void OnBuildNavigation(List<Selectable> selectables)
        {
            // configure navigation between and out of selectables in the group
            var tempNav = new Navigation
            {
                selectOnUp = m_up,
                selectOnDown = m_down,
            };

            var firstIndex = FindNextEnabledSelectable(0, out var first);
            var lastIndex = FindPreviousEnabledSelectable(selectables.Count - 1, out var last);

            for (var i = 0; i < selectables.Count; i++)
            {
                var current = selectables[i];

                if (i <= firstIndex)
                {
                    if (m_left != null)
                    {
                        tempNav.selectOnLeft = m_left;
                    }
                    else if (m_wrap)
                    {
                        tempNav.selectOnLeft = UIHelper.FindLastSelectableInChain(last, selectables, MoveDirection.Right);
                    }
                    else
                    {
                        tempNav.selectOnLeft = null;
                    }
                }
                else
                {
                    FindPreviousEnabledSelectable(i - 1, out var previous);
                    tempNav.selectOnLeft = previous;
                }

                if (i >= lastIndex)
                {
                    if (m_right != null)
                    {
                        tempNav.selectOnRight = m_right;
                    }
                    else if (m_wrap)
                    {
                        tempNav.selectOnRight = UIHelper.FindLastSelectableInChain(first, selectables, MoveDirection.Left);
                    }
                    else
                    {
                        tempNav.selectOnRight = null;
                    }
                }
                else
                {
                    FindNextEnabledSelectable(i + 1, out var next);
                    tempNav.selectOnRight = next;
                }

                current.navigation = tempNav;
            }

            // configure navigation to the group
            if (m_up != null)
            {
                tempNav = m_up.navigation;
                tempNav.selectOnDown = m_verticalSelect != null ? m_verticalSelect : first;
                m_up.navigation = tempNav;
            }

            if (m_down != null)
            {
                tempNav = m_down.navigation;
                tempNav.selectOnUp = m_verticalSelect != null ? m_verticalSelect : first;
                m_down.navigation = tempNav;
            }

            if (m_left != null)
            {
                tempNav = m_left.navigation;
                tempNav.selectOnRight = first;
                m_left.navigation = tempNav;
            }
            else if (m_wrap)
            {
                var wrap = UIHelper.FindLastSelectableInChain(last, selectables, MoveDirection.Right);

                tempNav = wrap.navigation;
                tempNav.selectOnRight = first;
                wrap.navigation = tempNav;
            }

            if (m_right != null)
            {
                tempNav = m_right.navigation;
                tempNav.selectOnLeft = last;
                m_right.navigation = tempNav;
            }
            else if (m_wrap)
            {
                var wrap = UIHelper.FindLastSelectableInChain(first, selectables, MoveDirection.Left);

                tempNav = wrap.navigation;
                tempNav.selectOnLeft = last;
                wrap.navigation = tempNav;
            }
        }
    }
}
