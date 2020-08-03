using System.Collections.Generic;

using Framework.Audio;

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

        private readonly List<MusicPlayer> m_sources = new List<MusicPlayer>();
        private readonly Dictionary<Music, MusicPlayer> m_profileToSources = new Dictionary<Music, MusicPlayer>();
        private readonly HashSet<MusicPlayer> m_active = new HashSet<MusicPlayer>();

        private void OnDestroy()
        {
            foreach (var source in m_sources)
            {
                source.Dispose();
            }
        }

        /// <inheritdoc/>
        protected override void UpdateBlending(Transform target, VolumeLayer layer)
        {
            foreach (var source in m_sources)
            {
                source.Volume = 0f;
            }

            m_active.Clear();

            var volumesWeights = MusicVolumeManager.Instance.Evaluate(target, layer);

            for (var i = 0; i < volumesWeights.Count; i++)
            {
                var profileBlend = volumesWeights[i];
                var volume = profileBlend.volume;
                var profile = volume.Music;

                if (profile != null && profile.Track != null)
                {
                    if (!m_profileToSources.TryGetValue(profile, out var source))
                    {
                        source = new MusicPlayer(gameObject, m_pausable)
                        {
                            Volume = 0f
                        };

                        m_profileToSources.Add(profile, source);
                        m_sources.Add(source);
                    }

                    m_active.Add(source);

                    // set the volume based on the weight
                    var vol = volume.Volume * profileBlend.weight;

                    if (!source.IsPlaying && vol > 0)
                    {
                        source.Play(profile);
                    }

                    source.Volume += vol;
                }
            }

            // make sure any sources not in an active volume are not playing
            foreach (var source in m_sources)
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
