using UnityEngine;
using UnityEngine.Audio;

namespace Framework
{
    [CreateAssetMenu(fileName = "AudioClipSet", menuName = "Audio/Audio Clip Set", order = 5)]
    public class AudioClipSet : ScriptableObject
    {
        [SerializeField]
        private AudioClip[] m_clips;

        public AudioClip PickClip()
        {
            return m_clips.PickRandom();
        }
    }
}
