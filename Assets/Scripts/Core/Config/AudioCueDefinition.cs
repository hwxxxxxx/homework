using System;
using UnityEngine;

[Serializable]
public class AudioCueDefinition
{
    [SerializeField] private AudioCueId cueId = AudioCueId.None;
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private AudioBus bus = AudioBus.Sfx;
    [SerializeField] private bool loop;
    [SerializeField] private bool spatial;
    [SerializeField] private float volume = 1f;
    [SerializeField] private float pitchMin = 1f;
    [SerializeField] private float pitchMax = 1f;
    [SerializeField] private float cooldown = 0f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 30f;

    public AudioCueId CueId => cueId;
    public AudioBus Bus => bus;
    public bool Loop => loop;
    public bool Spatial => spatial;
    public float Volume => Mathf.Clamp01(volume);
    public float PitchMin => pitchMin;
    public float PitchMax => Mathf.Max(pitchMin, pitchMax);
    public float Cooldown => Mathf.Max(0f, cooldown);
    public float MinDistance => Mathf.Max(0.1f, minDistance);
    public float MaxDistance => Mathf.Max(MinDistance, maxDistance);

    public bool TryGetRandomClip(out AudioClip clip)
    {
        clip = null;
        if (clips == null || clips.Length == 0)
        {
            return false;
        }

        int count = clips.Length;
        if (count == 1)
        {
            clip = clips[0];
            return clip != null;
        }

        int index = UnityEngine.Random.Range(0, count);
        clip = clips[index];
        return clip != null;
    }

    public static AudioCueDefinition CreateRuntime(
        AudioCueId id,
        AudioClip clip,
        AudioBus outputBus,
        bool isLoop,
        bool isSpatial,
        float cueVolume,
        float cuePitchMin,
        float cuePitchMax,
        float cueCooldown,
        float cueMinDistance,
        float cueMaxDistance)
    {
        return new AudioCueDefinition
        {
            cueId = id,
            clips = clip != null ? new[] { clip } : Array.Empty<AudioClip>(),
            bus = outputBus,
            loop = isLoop,
            spatial = isSpatial,
            volume = cueVolume,
            pitchMin = cuePitchMin,
            pitchMax = cuePitchMax,
            cooldown = cueCooldown,
            minDistance = cueMinDistance,
            maxDistance = cueMaxDistance
        };
    }
}
