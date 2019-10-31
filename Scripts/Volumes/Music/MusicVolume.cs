using UnityEngine;

using Framework.Audio;

namespace Framework.Volumes
{
    public class MusicVolume : Volume<MusicVolume, MusicVolumeManager>
    {
        [Header("Profile")]

        [SerializeField]
        [Tooltip("The profile for this volume.")]
        public Music m_sharedProfile;
        
        [Range(0f, 1f)]
        [Tooltip("The profile for this volume.")]
        public float volume = 1f;

#if UNITY_EDITOR
        protected override Color Color => Color.HSVToRGB(0.25f, 0.75f, 1.0f);

        private void OnDrawGizmos()
        {
            DrawGizmos();
        }
#endif
    }

    public class MusicVolumeManager : VolumeManager<MusicVolume, MusicVolumeManager>
    {
    }
}
