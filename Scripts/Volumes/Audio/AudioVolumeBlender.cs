using System.Collections.Generic;
using UnityEngine;

namespace Framework.Volumes
{
    public class AudioVolumeBlender : VolumeBlender
    {
        [SerializeField]
        [Tooltip("Restart sounds when they become audible.")]
        private bool m_restartWhenActivated = true;
        
        private readonly Dictionary<AudioVolumeProfile, AudioSource> m_profileToSources = new Dictionary<AudioVolumeProfile, AudioSource>();
        private readonly HashSet<AudioSource> m_active = new HashSet<AudioSource>();
        private readonly List<AudioSource> m_sources = new List<AudioSource>();

        protected override void UpdateBlending(Transform target, VolumeLayer layer)
        {
            m_active.Clear();

            var profiles = AudioVolumeManager.Instance.Evaluate(target, layer);

            foreach (AudioSource source in m_sources)
            {
                source.volume = 0f;
            }

            for (int i = 0; i < profiles.Count; i++)
            {
                var profileBlend = profiles[i];
                var volume = profileBlend.volume;
                var profile = volume.m_sharedProfile;

                if (profile.clip != null)
                {
                    // create an audiosource for this sound
                    AudioSource source;
                    if (!m_profileToSources.TryGetValue(profile, out source))
                    {
                        source = gameObject.AddComponent<AudioSource>();
                        source.clip = profile.clip;
                        source.outputAudioMixerGroup = profile.mixer;
                        source.playOnAwake = false;
                        source.loop = profile.loop;
                        source.pitch = profile.pitch;
                        source.panStereo = profile.pan;
                        source.spatialBlend = 0f;

                        source.volume = 0f;

                        m_profileToSources.Add(profile, source);
                        m_sources.Add(source);
                    }

                    m_active.Add(source);

                    // set the volume based on the weight
                    float vol = volume.volume * profileBlend.weight;
                    if (!source.isPlaying && vol > 0)
                    {
                        source.Play();
                    }
                    source.volume += vol;
                }
            }

            // make sure any sources not in an active volume are not playing
            foreach (AudioSource source in m_sources)
            {
                if (source.isPlaying && !m_active.Contains(source))
                {
                    if (m_restartWhenActivated)
                    {
                        source.Stop();
                    }
                    else
                    {
                        source.Pause();
                    }
                    source.volume = 0f;
                }
            }
        }
    }
}
