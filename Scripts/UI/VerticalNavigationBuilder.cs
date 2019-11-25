﻿using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    public class VerticalNavigationBuilder : NavigationBuilder
    {
        [SerializeField]
        [Tooltip("The selectable in the selectable group to select when navigating from selectables to the left or right when configuring vertical navigation. By default this is the first selectable in the group.")]
        private Selectable m_horizontalSelect = null;

        protected override List<Selectable> BuildNavigation()
        {
            return UIHelper.SetNavigationVertical(new NavConfig()
            {
                parent = transform,
                allowDisabled = false,
                up = m_up,
                down = m_down,
                left = m_left,
                right = m_right,
                horizontalSelect = m_horizontalSelect,
                wrap = m_wrap,
            });
        }
    }
}
