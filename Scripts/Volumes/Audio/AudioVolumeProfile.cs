using UnityEngine;
using UnityEngine.Audio;

namespace Framework.Volumes
{
    [CreateAssetMenu(fileName = "New Audio Profile", menuName = "Framework/Volumes/Audio", order = 50)]
    public class AudioVolumeProfile : ScriptableObject
    {
        public AudioClip clip;
        public AudioMixerGroup mixer;
        public bool loop = true;

        [Range(0f, 2f)]
        public float pitch = 1f;

        [Range(-1f, 1f)]
        public float pan = 0f;
    }
}
