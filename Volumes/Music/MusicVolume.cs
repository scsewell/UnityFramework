using UnityEngine;

namespace Framework.Volumes
{
    public class MusicVolume : Volume<MusicParams, MusicVolume, MusicVolumeManager>
    {
        [Range(0f, 1f)]
        public float volume = 1f;

#if UNITY_EDITOR
        protected override Color Color => Color.HSVToRGB(0.25f, 0.75f, 1.0f);
#endif
    }

    public class MusicVolumeManager : VolumeManager<MusicParams, MusicVolume, MusicVolumeManager>
    {
    }
}
