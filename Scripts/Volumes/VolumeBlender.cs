using UnityEngine;

namespace Framework.Volumes
{
    /// <summary>
    /// A component that evaluates volumes and applies the blended value.
    /// </summary>
    public abstract class VolumeBlender : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The layer defining which volumes to affect the blending for this source.")]
        private VolumeLayer m_layer = null;

        [SerializeField]
        [Tooltip("The target from which to get the position used for the blending. If null this transform is used.")]
        private VolumeTarget m_target = null;

        protected virtual void Awake()
        {
            if (m_layer == null)
            {
                Debug.LogWarning($"Volume blender \"{name}\" does not have a layer assigned. This volume blender will do nothing.");
            }
        }

        private void Update()
        {
            if (m_layer == null)
            {
                return;
            }

            var target = m_target != null ? m_target.Target : transform;
            UpdateBlending(target, m_layer);
        }

        /// <summary>
        /// Evaluates and applies the volume blend.
        /// </summary>
        /// <param name="target">The transform position to use.</param>
        /// <param name="layer">The volume layer controlling which volumes are evaluated.</param>
        protected abstract void UpdateBlending(Transform target, VolumeLayer layer);
    }
}
