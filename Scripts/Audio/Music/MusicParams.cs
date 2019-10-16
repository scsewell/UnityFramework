using UnityEngine;
using UnityEngine.Audio;

namespace Framework.Audio
{
    /// <summary>
    /// Represents a music track.
    /// </summary>
    [CreateAssetMenu(fileName = "MusicParams", menuName = "Framework/Audio/Music Params", order = 3)]
    public class MusicParams : ScriptableObject
    {
        [SerializeField]
        private AudioMixerGroup m_mixer = null;
        public AudioMixerGroup Mixer => m_mixer;

        [SerializeField]
        private AudioClip m_track = null;
        public AudioClip Track => m_track;

        [SerializeField]
        private string m_name = string.Empty;
        public string Name => m_name;

        [SerializeField]
        private string m_artist = string.Empty;
        public string Artist => m_artist;

        [SerializeField]
        private bool m_loop = true;
        public bool Loop => m_loop;

        [SerializeField]
        private int m_minutes = 0;
        [SerializeField]
        private double m_seconds = 0.0;

        /// <summary>
        /// The time at which the next loop should start.
        /// </summary>
        public double LoopDuration => (m_minutes * 60) + m_seconds;

        public override string ToString() => Name;
    }
}
