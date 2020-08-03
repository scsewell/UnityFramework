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
        [Tooltip("Allow navigation between the first and last selectable. " +
            "If this config uses the up/down/left/right selectables, we follow along their navigation chain in the relevant direction and wrap to the last avaiable element.")]
        protected bool m_wrap = true;

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

        /// <summary>
        /// The selectables in the navigation group.
        /// </summary>
        public List<Selectable> Selectables { get; private set; } = null;


        protected virtual void Start()
        {
            Selectables = BuildNavigation();
        }

        /// <summary>
        /// Builds the navigation for the group of selectables to manage.
        /// </summary>
        /// <returns>The selectables in the navigation chain.</returns>
        protected abstract List<Selectable> BuildNavigation();
    }
}
