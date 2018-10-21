using UnityEngine;

namespace Framework.Volumes
{
    public class VolumeTargetSetter : MonoBehaviour
    {
        [SerializeField]
        private VolumeTarget m_target = null;

        private void Awake()
        {
            if (m_target != null)
            {
                m_target.Target = transform;
            }
        }
    }
}
