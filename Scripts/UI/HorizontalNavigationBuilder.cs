using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    public class HorizontalNavigationBuilder : NavigationBuilder
    {
        [SerializeField]
        [Tooltip("The selectable in the selectable group to select when navigating from selectables to the top or bottom when configuring horizontal navigation. By default this is the first selectable in the group.")]
        private Selectable m_verticalSelect = null;

        protected override List<Selectable> BuildNavigation()
        {
            return UIHelper.SetNavigationHorizontal(new NavConfig()
            {
                parent = transform,
                allowDisabled = false,
                up = m_up,
                down = m_down,
                left = m_left,
                right = m_right,
                verticalSelect = m_verticalSelect,
                wrap = m_wrap,
            });
        }
    }
}
