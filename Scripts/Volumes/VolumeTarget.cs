using UnityEngine;

namespace Framework.Volumes
{
    /// <summary>
    /// An asset that stores the transform whose position is used as input for a <see cref="VolumeBlender"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "New Volume Target", menuName = "Framework/Volumes/Target", order = 2)]
    public class VolumeTarget : ScriptableObject
    {
        /// <summary>
        /// The transform whose position is used by <see cref="VolumeBlender"/ instances this target
        /// is assigned to.
        /// </summary>
        public Transform Target { get; set; }

        private void OnEnable()
        {
            Target = null;
        }
    }
}
