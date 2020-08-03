using UnityEngine;
using UnityEngine.Audio;

namespace Framework.Audio
{
    /// <summary>
    /// A class that manages playing <see cref="Music"/> tracks.
    /// </summary>
    public class MusicPlayer : Disposable
    {
        private readonly GameObject m_go = null;
        private readonly AudioSource[] m_sources = new AudioSource[2];

        private AudioMixerGroup m_mixer = null;
        private Music m_currentTrack = null;
        private int m_lastMusicSource = 0;
        private double m_lastLoopTime = 0.0;

        private float m_volume = 1.0f;

        /// <summary>
        /// The volume of the music tracks.
        /// </summary>
        public float Volume
        {
            get => m_volume;
            set
            {
                EnsureNotDisposed();
                var volume = Mathf.Clamp01(value);
                if (m_volume != volume)
                {
                    m_volume = volume;
                    m_sources[0].volume = m_volume;
                    m_sources[1].volume = m_volume;
                }
            }
        }

        private bool m_pausable = true;

        /// <summary>
        /// Does the music pause while the audio listener is paused.
        /// </summary>
        public bool Pausable
        {
            get => m_pausable;
            set
            {
                EnsureNotDisposed();
                if (m_pausable != value)
                {
                    m_pausable = value;
                    m_sources[0].ignoreListenerPause = !m_pausable;
                    m_sources[1].ignoreListenerPause = !m_pausable;
                }
            }
        }

        /// <summary>
        /// Gets if any music track is currently playing.
        /// </summary>
        public bool IsPlaying => m_sources[0].isPlaying || m_sources[1].isPlaying;

        /// <summary>
        /// Creates a new music player.
        /// </summary>
        /// <param name="gameObject">The object to add the audio components onto.</param>
        /// <param name="pauseable">Will the music pause while the audio listener is paused.</param>
        public MusicPlayer(GameObject gameObject, bool pauseable = true)
        {
            m_go = gameObject;

            for (var i = 0; i < m_sources.Length; i++)
            {
                var source = m_go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                m_sources[i] = source;
            }

            Pausable = pauseable;
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                for (var i = 0; i < m_sources.Length; i++)
                {
                    Object.Destroy(m_sources[i]);
                }
            }
        }

        /// <summary>
        /// Updates the music player.
        /// </summary>
        public void Update()
        {
            EnsureNotDisposed();

            if (IsPlaying && m_currentTrack != null && m_currentTrack.CanLoop)
            {
                var nextLoopTime = m_lastLoopTime + m_currentTrack.LoopTime;

                // if near the end of the current loop start the next one
                if (nextLoopTime - AudioSettings.dspTime < 0.1)
                {
                    PlayScheduled(nextLoopTime);
                }
            }
        }

        /// <summary>
        /// Plays a music track.
        /// </summary>
        /// <param name="music">The music to play.</param>
        /// <param name="mixer">The mixer channel to play the music on.</param>
        public void Play(Music music, AudioMixerGroup mixer = null)
        {
            Stop();

            if (music != null)
            {
                m_mixer = mixer;
                m_currentTrack = music;
                PlayScheduled(AudioSettings.dspTime + 0.01);
            }
        }

        /// <summary>
        /// Stops playing music.
        /// </summary>
        public void Stop()
        {
            EnsureNotDisposed();

            m_sources[0].Stop();
            m_sources[1].Stop();

            m_currentTrack = null;
            m_sources[0].clip = null;
            m_sources[1].clip = null;

            m_lastMusicSource = 0;
            m_lastLoopTime = 0.0;
        }

        /// <summary>
        /// Resumes the music if paused.
        /// </summary>
        public void Resume()
        {
            EnsureNotDisposed();

            m_sources[0].UnPause();
            m_sources[1].UnPause();
        }

        /// <summary>
        /// Pauses the music.
        /// </summary>
        public void Pause()
        {
            EnsureNotDisposed();

            m_sources[0].Pause();
            m_sources[1].Pause();
        }

        private void PlayScheduled(double time)
        {
            var source = (m_lastMusicSource + 1) % m_sources.Length;
            var music = m_sources[source];

            music.clip = m_currentTrack.Track;
            music.outputAudioMixerGroup = m_mixer;
            music.PlayScheduled(time);

            m_lastLoopTime = time;
            m_lastMusicSource = source;
        }
    }
}
