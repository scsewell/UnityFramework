using System.Collections.Generic;

using UnityEngine;

namespace Framework.Volumes
{
    public class AudioVolumeBlender : VolumeBlender
    {
        [SerializeField]
        [Tooltip("Restart sounds when they become audible.")]
        private bool m_restartWhenActivated = true;

        private readonly List<AudioSource> m_sources = new List<AudioSource>();
        private readonly Dictionary<AudioVolumeProfile, AudioSource> m_profileToSources = new Dictionary<AudioVolumeProfile, AudioSource>();
        private readonly HashSet<AudioSource> m_active = new HashSet<AudioSource>();

        /// <inheritdoc/>
        protected override void UpdateBlending(Transform target, VolumeLayer layer)
        {
            foreach (var source in m_sources)
            {
                source.volume = 0f;
            }

            m_active.Clear();

            var profiles = AudioVolumeManager.Instance.Evaluate(target, layer);

            for (var i = 0; i < profiles.Count; i++)
            {
                var profileBlend = profiles[i];
                var volume = profileBlend.volume;
                var profile = volume.Audio;

                if (profile.Clip != null)
                {
                    // create an audiosource for this sound
                    if (!m_profileToSources.TryGetValue(profile, out var source))
                    {
                        source = gameObject.AddComponent<AudioSource>();
                        source.clip = profile.Clip;
                        source.outputAudioMixerGroup = profile.Mixer;
                        source.playOnAwake = false;
                        source.loop = profile.Loop;
                        source.pitch = profile.Pitch;
                        source.panStereo = profile.Pan;
                        source.spatialBlend = 0f;

                        source.volume = 0f;

                        m_profileToSources.Add(profile, source);
                        m_sources.Add(source);
                    }

                    m_active.Add(source);

                    // set the volume based on the weight
                    var vol = volume.Volume * profileBlend.weight;

                    if (!source.isPlaying && vol > 0)
                    {
                        source.Play();
                    }

                    source.volume += vol;
                }
            }

            // make sure any sources not in an active volume are not playing
            foreach (var source in m_sources)
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
