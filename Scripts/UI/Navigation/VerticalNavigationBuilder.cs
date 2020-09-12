using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Framework.UI
{
    /// <summary>
    /// A <see cref="NavigationBuilder"/> that manages a chain of vertically placed <see cref="Selectables"/>.
    /// </summary>
    public class VerticalNavigationBuilder : NavigationBuilder
    {
        [SerializeField]
        [Tooltip("The selectable in the selectable group to select when navigating to the group from the left or right when configuring vertical navigation. " +
            "By default this is the first selectable in the group.")]
        private Selectable m_horizontalSelect = null;


        /// <summary>
        /// The selectable in the selectable group to select when navigating to the group from the left or right.
        /// </summary>
        /// <remarks>
        /// By default this is the first selectable in the group.
        /// </remarks>
        public Selectable HorizontalSelect
        {
            get => m_horizontalSelect;
            set => m_horizontalSelect = value;
        }


        /// <inheritdoc/>
        protected override void OnBuildNavigation(List<Selectable> selectables)
        {
            // configure navigation between and out of selectables in the group
            var tempNav = new Navigation
            {
                selectOnLeft = m_left,
                selectOnRight = m_right,
            };

            var firstIndex = FindNextEnabledSelectable(0, out var first);
            var lastIndex = FindPreviousEnabledSelectable(selectables.Count - 1, out var last);

            for (var i = 0; i < selectables.Count; i++)
            {
                var current = selectables[i];

                if (i <= firstIndex)
                {
                    if (m_up != null)
                    {
                        tempNav.selectOnUp = m_up;
                    }
                    else if (m_wrap)
                    {
                        tempNav.selectOnUp = UIHelper.FindLastSelectableInChain(last, selectables, MoveDirection.Down);
                    }
                    else
                    {
                        tempNav.selectOnUp = null;
                    }
                }
                else
                {
                    FindPreviousEnabledSelectable(i - 1, out var previous);
                    tempNav.selectOnUp = previous;
                }

                if (i >= lastIndex)
                {
                    if (m_down != null)
                    {
                        tempNav.selectOnDown = m_down;
                    }
                    else if (m_wrap)
                    {
                        tempNav.selectOnDown = UIHelper.FindLastSelectableInChain(first, selectables, MoveDirection.Up);
                    }
                    else
                    {
                        tempNav.selectOnDown = null;
                    }
                }
                else
                {
                    FindNextEnabledSelectable(i + 1, out var next);
                    tempNav.selectOnDown = next;
                }

                current.navigation = tempNav;
            }

            // configure navigation to the group
            if (m_up != null)
            {
                tempNav = m_up.navigation;
                tempNav.selectOnDown = first;
                m_up.navigation = tempNav;
            }
            else if (m_wrap)
            {
                var wrap = UIHelper.FindLastSelectableInChain(last, selectables, MoveDirection.Down);

                tempNav = wrap.navigation;
                tempNav.selectOnDown = first;
                wrap.navigation = tempNav;
            }

            if (m_down != null)
            {
                tempNav = m_down.navigation;
                tempNav.selectOnUp = last;
                m_down.navigation = tempNav;
            }
            else if (m_wrap)
            {
                var wrap = UIHelper.FindLastSelectableInChain(first, selectables, MoveDirection.Up);

                tempNav = wrap.navigation;
                tempNav.selectOnUp = last;
                wrap.navigation = tempNav;
            }

            if (m_left != null)
            {
                tempNav = m_left.navigation;
                tempNav.selectOnRight = m_horizontalSelect != null ? m_horizontalSelect : first;
                m_left.navigation = tempNav;
            }

            if (m_right != null)
            {
                tempNav = m_right.navigation;
                tempNav.selectOnLeft = m_horizontalSelect != null ? m_horizontalSelect : first;
                m_right.navigation = tempNav;
            }
        }
    }
}
