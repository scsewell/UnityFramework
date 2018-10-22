using System.Collections.Generic;
using UnityEngine;

namespace Framework.Volumes
{
    public class MusicVolumeBlender : VolumeBlender
    {
        [SerializeField]
        [Tooltip("Stop playing when the AudioListener is paused.")]
        private bool m_pausable = true;

        [SerializeField]
        [Tooltip("Restart music tracks when they become audible.")]
        private bool m_restartWhenActivated = true;

        private readonly Dictionary<MusicParams, MusicPlayer> m_profileToSources = new Dictionary<MusicParams, MusicPlayer>();
        private readonly HashSet<MusicPlayer> m_active = new HashSet<MusicPlayer>();
        private readonly List<MusicPlayer> m_sources = new List<MusicPlayer>();

        protected override void UpdateBlending(Transform target, VolumeLayer layer)
        {
            m_active.Clear();

            var profiles = MusicVolumeManager.Instance.GetProfiles(target, layer);

            for (int i = 0; i < profiles.Count; i++)
            {
                var profileBlend = profiles[i];
                var volume = profileBlend.volume;
                var profile = volume.m_sharedProfile;

                if (profile != null)
                {
                    MusicPlayer source;
                    if (!m_profileToSources.TryGetValue(profile, out source))
                    {
                        source = new MusicPlayer(gameObject, m_pausable);
                        m_profileToSources.Add(profile, source);
                        m_sources.Add(source);
                    }

                    m_active.Add(source);

                    // set the volume based on the weight
                    float vol = volume.volume * profileBlend.weight;
                    if (!source.IsPlaying && vol > 0)
                    {
                        source.Play(profile);
                    }
                    source.Volume = vol;
                }
            }

            // make sure any sources not in an active volume are not playing
            foreach (MusicPlayer source in m_sources)
            {
                if (source.IsPlaying && !m_active.Contains(source))
                {
                    if (m_restartWhenActivated)
                    {
                        source.Stop();
                    }
                    else
                    {
                        source.Pause();
                    }
                    source.Volume = 0f;
                }
                source.Update();
            }
        }
    }
}
