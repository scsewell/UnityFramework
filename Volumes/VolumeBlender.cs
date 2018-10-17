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
    }
}
