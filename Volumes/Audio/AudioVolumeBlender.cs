using System.Collections.Generic;
using UnityEngine;

namespace Framework.Volumes
{
    public class AudioVolumeBlender : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The layer defining which volumes to affect the blending for this source.")]
        private VolumeLayer m_layer;

        [SerializeField]
        [Tooltip("The transform from which to get the position used for the volume blending calculations.")]
        private Transform m_target;

        [SerializeField]
        [Tooltip("Restart sounds when they become audible.")]
        private bool m_restartWhenActivated = true;
        
        private readonly Dictionary<AudioVolumeProfile, AudioSource> m_profileToSources = new Dictionary<AudioVolumeProfile, AudioSource>();
        private readonly HashSet<AudioSource> m_active = new HashSet<AudioSource>();
        private readonly List<AudioSource> m_sources = new List<AudioSource>();

        private void Update()
        {
            m_active.Clear();

            var profiles = AudioVolumeManager.Instance.GetProfiles(m_target, m_layer);
            
            for (int i = 0; i < profiles.Count; i++)
            {
                var profileBlend = profiles[i];
                AudioVolumeProfile profile = profileBlend.profile;

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

                        m_profileToSources.Add(profile, source);
                        m_sources.Add(source);
                    }

                    m_active.Add(source);
                    
                    // set the volume based on the weight
                    float volume = profile.volume * profileBlend.weight;

                    if (!source.isPlaying && volume > 0)
                    {
                        source.Play();
                    }

                    source.volume = volume;
                }
            }

            // make sure any sources not in an active volume are not playing
            foreach (AudioSource source in m_sources)
            {
                if (m_restartWhenActivated && source.isPlaying && !m_active.Contains(source))
                {
                    source.Stop();
                    source.volume = 0f;
                }
            }
        }
    }
}
