﻿using System;

using Framework.AssetBundles;

using UnityEngine;

namespace Framework.Audio
{
    [Serializable]
    public class AssetBundleMusicReference : AssetBundleObjectReference<Music>
    {
    }

    /// <summary>
    /// An asset that stores a reference to a music track and manages metadata.
    /// </summary>
    [CreateAssetMenu(fileName = "New Music", menuName = "Framework/Audio/Music", order = 3)]
    public class Music : ScriptableObject
    {
        [Serializable]
        public class AssetBundleAudioClipReference : AssetBundleObjectReference<AudioClip>
        {
        }

        [SerializeField]
        [Tooltip("The music track.")]
        private AssetBundleAudioClipReference m_track = null;

        /// <summary>
        /// The music track.
        /// </summary>
        public AssetBundleAudioClipReference Track => m_track;

        [SerializeField]
        [Tooltip("The display name of the track.")]
        private string m_name = string.Empty;

        /// <summary>
        /// The display name of the track.
        /// </summary>
        public string Name => m_name;

        [SerializeField]
        [Tooltip("The display name of the artist.")]
        private string m_artist = string.Empty;

        /// <summary>
        /// The display name of the artist.
        /// </summary>
        public string Artist => m_artist;

        [SerializeField]
        [Tooltip("The credits for the music.")]
        [TextArea(4, 10)]
        private string m_attribution = string.Empty;

        /// <summary>
        /// The credits for the music.
        /// </summary>
        public string Attribution => m_attribution;

        /// <summary>
        /// The behaviours of a music track when it is finished playing.
        /// </summary>
        public enum LoopMode
        {
            /// <summary>
            /// The track does not loop.
            /// </summary>
            None,
            /// <summary>
            /// The track loops at the end of the track.
            /// </summary>
            AtEnd,
            /// <summary>
            /// The track loops at a specific time prior to the end of the track.
            /// </summary>
            AtTime,
        }

        [SerializeField]
        [Tooltip("How the music track should loop.")]
        private LoopMode m_loop = LoopMode.None;

        /// <summary>
        /// How the music track should loop.
        /// </summary>
        public LoopMode Loop => m_loop;

        /// <summary>
        /// Gets if the music track support looping.
        /// </summary>
        public bool CanLoop => m_loop != LoopMode.None;

        [SerializeField]
        private int m_minutes = 0;
        [SerializeField]
        private double m_seconds = 0.0;

        /// <summary>
        /// The time in seconds at which the audio loops.
        /// </summary>
        /// <param name="music">The music to get the looping time for.</param>
        /// <param name="track">The loaded audio track.</param>
        /// <returns>The loop time, or 0 if <paramref name="music"/> or <paramref name="track"/>
        /// is null.</returns>
        public static double GetLoopTime(Music music, AudioClip track)
        {
            if (music != null && track != null)
            {
                switch (music.m_loop)
                {
                    case LoopMode.AtEnd:
                        return (double)track.samples / track.frequency;
                    case LoopMode.AtTime:
                        return (music.m_minutes * 60) + music.m_seconds;
                    default:
                        throw new ArgumentException("Invalid loop mode set!", nameof(music));
                }
            }
            return 0.0;
        }

        public override string ToString() => Name;
    }
}
