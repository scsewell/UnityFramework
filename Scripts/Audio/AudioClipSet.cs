using UnityEngine;

namespace Framework.Audio
{
    /// <summary>
    /// An asset that contains a set of audio clips from which a random clip can be picked.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioClipSet", menuName = "Framework/Audio/Audio Clip Set", order = 5)]
    public class AudioClipSet : ScriptableObject
    {
        [SerializeField]
        private AudioClip[] m_clips = null;

        /// <summary>
        /// Gets a random clip from this set.
        /// </summary>
        /// <returns>A random clip, or null if the set is empty.</returns>
        public AudioClip PickClip()
        {
            return m_clips.Length > 0 ? m_clips.PickRandom() : null;
        }
    }
}
