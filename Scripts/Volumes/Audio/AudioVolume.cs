using UnityEngine;

namespace Framework.Volumes
{
    public class AudioVolume : Volume<AudioVolume, AudioVolumeManager>
    {
        [Header("Profile")]

        [SerializeField]
        [Tooltip("The audio to play in this volume.")]
        private AudioVolumeProfile m_audio;

        [SerializeField]
        [Tooltip("The volume of the audio clip.")]
        [Range(0f, 1f)]
        private float m_volume = 1f;

        /// <summary>
        /// The audio to play in this volume.
        /// </summary>
        public AudioVolumeProfile Audio => m_audio;

        /// <summary>
        /// The volume of the audio clip.
        /// </summary>
        public float Volume => m_volume;

        /// <inheritdoc/>
        protected override Color GizmoColor => Color.HSVToRGB(0.5f, 0.75f, 1.0f);
    }

    public class AudioVolumeManager : VolumeManager<AudioVolume, AudioVolumeManager>
    {
    }
}
