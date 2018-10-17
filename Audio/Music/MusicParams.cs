using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "MusicParams", menuName = "Audio/Music Params", order = 3)]
public class MusicParams : ScriptableObject
{
    [SerializeField]
    private AudioMixerGroup m_mixer;
    public AudioMixerGroup Mixer => m_mixer;

    [SerializeField]
    private AudioClip m_track;
    public AudioClip Track => m_track;
    
    [SerializeField]
    private string m_name;
    public string Name => m_name;

    [SerializeField]
    private string m_artist;
    public string Artist => m_artist;

    [SerializeField]
    private int m_minutes;
    [SerializeField]
    private double m_seconds;

    public double LoopDuration => (m_minutes * 60) + m_seconds;
}