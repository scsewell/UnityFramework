using UnityEngine;

namespace Framework.Volumes
{
    public abstract class VolumeBlender : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The layer defining which volumes to affect the blending for this source.")]
        private VolumeLayer m_layer;

        [SerializeField]
        [Tooltip("The target from which to get the position used for the blending. If null this transform is used.")]
        private VolumeTarget m_target;
        
        protected virtual void Awake()
        {
            if (m_layer == null)
            {
                Debug.LogWarning($"Volume blender \"{name}\" does not have a layer assigned. This volume blender will do nothing.");
            }
        }

        private void Update()
        {
            Transform target = m_target != null ? m_target.Target : transform;
            UpdateBlending(target, m_layer);
        }

        protected abstract void UpdateBlending(Transform target, VolumeLayer layer);
    }
}
