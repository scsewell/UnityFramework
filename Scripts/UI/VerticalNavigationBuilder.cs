using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    /// <summary>
    /// A <see cref="NavigationBuilder"/> that manages a chain of vertically placed <see cref="Selectables"/>.
    /// </summary>
    public class VerticalNavigationBuilder : NavigationBuilder
    {
        [SerializeField]
        [Tooltip("The selectable in the selectable group to select when navigating from selectables to the left or right when configuring vertical navigation. " +
            "By default this is the first selectable in the group.")]
        private Selectable m_horizontalSelect = null;

        /// <inheritdoc/>
        protected override List<Selectable> OnBuildNavigation()
        {
            return UIHelper.SetNavigationVertical(new NavConfig()
            {
                parent = transform,
                up = m_up,
                down = m_down,
                left = m_left,
                right = m_right,
                defaultSelectable = m_horizontalSelect,
                wrap = m_wrap,
                allowDisabled = m_allowDisabled,
            });
        }
    }
}
