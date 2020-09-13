using UnityEngine;
using UnityEngine.Audio;

namespace Framework.Volumes
{
    /// <summary>
    /// An asset that configures how an audio clip will play in an <see cref="AudioVolume"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "New Audio Profile", menuName = "Framework/Volumes/Audio", order = 50)]
    public class AudioVolumeProfile : ScriptableObject
    {
        [SerializeField]
        [Tooltip("The audio clip to play.")]
        private AudioClip m_clip = null;

        [SerializeField]
        [Tooltip("The mixer channel to play the audio clip on.")]
        private AudioMixerGroup m_mixer = null;

        [SerializeField]
        [Tooltip("Should the clip loop when it plays.")]
        private bool m_loop = true;

        [SerializeField]
        [Tooltip("The pitch multiplier of the audio clip.")]
        [Range(0f, 2f)]
        private float m_pitch = 1f;

        [SerializeField]
        [Tooltip("The panning of the audio clip.")]
        [Range(-1f, 1f)]
        private float m_pan = 0f;

        /// <summary>
        /// The audio clip to play.
        /// </summary>
        public AudioClip Clip => m_clip;

        /// <summary>
        /// The mixer channel to play the audio clip on.
        /// </summary>
        public AudioMixerGroup Mixer => m_mixer;

        /// <summary>
        /// Should the clip loop when it plays.
        /// </summary>
        public bool Loop => m_loop;

        /// <summary>
        /// The pitch multiplier of the audio clip.
        /// </summary>
        public float Pitch => m_pitch;

        /// <summary>
        /// The panning of the audio clip.
        /// </summary>
        public float Pan => m_pan;
    }
}
