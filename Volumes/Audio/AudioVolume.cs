using UnityEngine;

namespace Framework.Volumes
{
    public class AudioVolume : Volume<AudioVolume, AudioVolumeManager>
    {
        [SerializeField]
        [Tooltip("The profile for this volume.")]
        public AudioVolumeProfile m_sharedProfile;

        [Range(0f, 1f)]
        public float volume = 1f;
        
#if UNITY_EDITOR
        protected override Color Color => Color.HSVToRGB(0.5f, 0.75f, 1.0f);
        
        private void OnDrawGizmos()
        {
            DrawGizmos();
        }
#endif
    }

    public class AudioVolumeManager : VolumeManager<AudioVolume, AudioVolumeManager>
    {
    }
}
