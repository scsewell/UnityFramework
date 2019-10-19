using UnityEngine;

namespace Framework.Audio
{
    [CreateAssetMenu(fileName = "AudioClipSet", menuName = "Framework/Audio/Audio Clip Set", order = 5)]
    public class AudioClipSet : ScriptableObject
    {
        [SerializeField]
        private AudioClip[] m_clips = null;

        public AudioClip PickClip() => m_clips.PickRandom();
    }
}
