using UnityEngine;

namespace Framework.Volumes
{
    [CreateAssetMenu(fileName = "New Volume Target", menuName = "Framework/Volumes/Target", order = 2)]
    public class VolumeTarget : ScriptableObject
    {
        private Transform m_target = null;

        public Transform Target
        {
            get
            {
                return m_target;
            }
            set
            {
                if (m_target != null)
                {
                    Debug.LogWarning($"Volume target \"{name}\" aleady has a target assigned.");
                }
                m_target = value;
            }
        }

        private void OnEnable()
        {
            m_target = null;
        }
    }
}
