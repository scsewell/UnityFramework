using UnityEngine;

namespace Framework.Volumes
{
    public abstract class VolumeBlender : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The layer defining which volumes to affect the blending for this source.")]
        protected VolumeLayer m_layer;

        [SerializeField]
        [Tooltip("The transform from which to get the position used for the volume blending calculations.")]
        protected Transform m_target;

        protected virtual void Awake()
        {
            if (m_layer == null)
            {
                Debug.LogWarning($"Volume blender \"{name}\" does not have a layer assigned. This volume blender will do nothing.");
            }

            if (m_target == null)
            {
                m_target = transform;
            }
        }
    }
}
