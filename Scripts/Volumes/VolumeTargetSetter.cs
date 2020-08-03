using UnityEngine;

namespace Framework.Volumes
{
    /// <summary>
    /// Sets a volume target to this transform.
    /// </summary>
    public class VolumeTargetSetter : MonoBehaviour
    {
        [SerializeField]
        private VolumeTarget m_target = null;

        private void OnEnable()
        {
            if (m_target != null)
            {
                m_target.Target = transform;
            }
        }
    }
}
