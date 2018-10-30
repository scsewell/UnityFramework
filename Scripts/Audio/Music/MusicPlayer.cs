using UnityEngine;

namespace Framework
{
    public class MusicPlayer
    {
        private readonly GameObject m_go;
        private readonly AudioSource[] m_sources;
        private MusicParams m_musicParams;
        private int m_lastMusicSource = 0;
        private double m_lastLoopTime;

        private float m_volume = 1.0f;
        public float Volume
        {
            get { return m_volume; }
            set
            {
                float volume = Mathf.Clamp01(value);
                if (m_volume != volume)
                {
                    m_volume = volume;
                    m_sources[0].volume = m_volume;
                    m_sources[1].volume = m_volume;
                }
            }
        }
        
        private bool m_pausable = true;
        public bool Pausable
        {
            get { return m_pausable; }
            set
            {
                if (m_pausable != value)
                {
                    m_pausable = value;
                    m_sources[0].ignoreListenerPause = !m_pausable;
                    m_sources[1].ignoreListenerPause = !m_pausable;
                }
            }
        }

        public bool IsPlaying => m_sources[0].isPlaying || m_sources[1].isPlaying;

        public MusicPlayer(GameObject gameObject, bool pauseable = true)
        {
            m_go = gameObject;

            m_sources = new AudioSource[2];
            for (int i = 0; i < m_sources.Length; i++)
            {
                AudioSource source = m_go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                m_sources[i] = source;
            }

            Pausable = pauseable;
        }
        
        public void Update()
        {
            if (IsPlaying)
            {
                double nextLoopTime = m_lastLoopTime + m_musicParams.LoopDuration;
                if (nextLoopTime - AudioSettings.dspTime < 1)
                {
                    PlayScheduled(nextLoopTime);
                }
            }
        }

        public void Play(MusicParams music)
        {
            Stop();

            if (music != null)
            {
                m_musicParams = music;
                PlayScheduled(AudioSettings.dspTime + 0.01);
            }
        }

        public void Pause()
        {
            m_sources[0].Pause();
            m_sources[1].Pause();
        }

        public void Stop()
        {
            m_sources[0].Stop();
            m_sources[1].Stop();
        }

        private void PlayScheduled(double time)
        {
            int source = (m_lastMusicSource + 1) % m_sources.Length;
            AudioSource music = m_sources[source];
            music.clip = m_musicParams.Track;
            music.outputAudioMixerGroup = m_musicParams.Mixer;
            music.PlayScheduled(time);
            m_lastLoopTime = time;
            m_lastMusicSource = source;
        }
    }
}
