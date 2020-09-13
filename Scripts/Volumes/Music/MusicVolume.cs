using UnityEngine;

using Framework.Audio;

namespace Framework.Volumes
{
    public class MusicVolume : Volume<MusicVolume, MusicVolumeManager>
    {
        [Header("Profile")]

        [SerializeField]
        [Tooltip("The music to play in this volume.")]
        private Music m_music = null;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("The volume of the music track.")]
        private float m_volume = 1f;

        /// <summary>
        /// The music to play in this volume.
        /// </summary>
        public Music Music => m_music;

        /// <summary>
        /// The volume of the music track.
        /// </summary>
        public float Volume => m_volume;

        /// <inheritdoc/>
        protected override Color GizmoColor => Color.HSVToRGB(0.25f, 0.75f, 1.0f);
    }

    public class MusicVolumeManager : VolumeManager<MusicVolume, MusicVolumeManager>
    {
    }
}
