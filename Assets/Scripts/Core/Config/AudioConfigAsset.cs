using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Config/Audio Config", fileName = "AudioConfig")]
public class AudioConfigAsset : ScriptableObject
{
    [Header("Legacy Quick Bindings")]
    [SerializeField] private AudioClip bgmMainLoop;
    [SerializeField] private float bgmVolume = 0.7f;
    [SerializeField] private AudioClip uiClick;
    [SerializeField] private float uiSfxVolume = 0.8f;
    [SerializeField] private AudioClip playerWeaponFire;
    [SerializeField] private AudioClip playerHitEnemy;
    [SerializeField] private AudioClip playerWeaponReload;
    [SerializeField] private AudioClip playerRun;
    [SerializeField] private float playerRunVolume = 0.8f;
    [SerializeField] private AudioClip enemyVoice;
    [SerializeField] private float enemyVoiceVolume = 0.9f;
    [SerializeField] private float enemyVoiceMinInterval = 3f;
    [SerializeField] private float enemyVoiceMaxInterval = 8f;
    [SerializeField] private AudioClip normalEnemyAttack;
    [SerializeField] private AudioClip normalEnemyDeath;
    [SerializeField] private AudioClip bossEnemyAttack;
    [SerializeField] private AudioClip bossEnemyDeath;

    [Header("Defaults")]
    [SerializeField] private float defaultMinDistance = 1f;
    [SerializeField] private float defaultMaxDistance = 30f;
    [SerializeField] private float defaultMasterVolume = 1f;
    [SerializeField] private float defaultMusicVolume = 1f;
    [SerializeField] private float defaultSfxVolume = 1f;
    [SerializeField] private float defaultUiVolume = 1f;
    [SerializeField] private float defaultVoiceVolume = 1f;
    [SerializeField] private float defaultAmbienceVolume = 1f;

    [Header("Cue Catalog (Optional, overrides legacy mapping when same cue exists)")]
    [SerializeField] private List<AudioCueDefinition> cueDefinitions = new List<AudioCueDefinition>();

    private Dictionary<AudioCueId, AudioCueDefinition> cachedCueMap;

    public float EnemyVoiceMinInterval => Mathf.Max(0.1f, enemyVoiceMinInterval);
    public float EnemyVoiceMaxInterval => Mathf.Max(EnemyVoiceMinInterval, enemyVoiceMaxInterval);

    public float DefaultMasterVolume => Mathf.Clamp01(defaultMasterVolume);
    public float DefaultMusicVolume => Mathf.Clamp01(defaultMusicVolume);
    public float DefaultSfxVolume => Mathf.Clamp01(defaultSfxVolume);
    public float DefaultUiVolume => Mathf.Clamp01(defaultUiVolume);
    public float DefaultVoiceVolume => Mathf.Clamp01(defaultVoiceVolume);
    public float DefaultAmbienceVolume => Mathf.Clamp01(defaultAmbienceVolume);

    private void OnEnable()
    {
        cachedCueMap = null;
    }

    private void OnValidate()
    {
        cachedCueMap = null;
    }

    public bool TryGetCue(AudioCueId cueId, out AudioCueDefinition cue)
    {
        EnsureCueMap();
        return cachedCueMap.TryGetValue(cueId, out cue);
    }

    public float GetBusDefaultVolume(AudioBus bus)
    {
        switch (bus)
        {
            case AudioBus.Music:
                return DefaultMusicVolume;
            case AudioBus.Sfx:
                return DefaultSfxVolume;
            case AudioBus.Ui:
                return DefaultUiVolume;
            case AudioBus.Voice:
                return DefaultVoiceVolume;
            case AudioBus.Ambience:
                return DefaultAmbienceVolume;
            default:
                return DefaultMasterVolume;
        }
    }

    private void EnsureCueMap()
    {
        if (cachedCueMap != null)
        {
            return;
        }

        cachedCueMap = new Dictionary<AudioCueId, AudioCueDefinition>();

        if (cueDefinitions != null)
        {
            for (int i = 0; i < cueDefinitions.Count; i++)
            {
                AudioCueDefinition cue = cueDefinitions[i];
                if (cue == null || cue.CueId == AudioCueId.None)
                {
                    continue;
                }

                cachedCueMap[cue.CueId] = cue;
            }
        }

        AddLegacyCueIfMissing(AudioCueId.BgmMainLoop, bgmMainLoop, AudioBus.Music, true, false, bgmVolume);
        AddLegacyCueIfMissing(AudioCueId.UiClick, uiClick, AudioBus.Ui, false, false, uiSfxVolume);
        AddLegacyCueIfMissing(AudioCueId.PlayerWeaponFire, playerWeaponFire, AudioBus.Sfx, false, true, 1f);
        AddLegacyCueIfMissing(AudioCueId.PlayerHitEnemy, playerHitEnemy, AudioBus.Sfx, false, true, 1f);
        AddLegacyCueIfMissing(AudioCueId.PlayerWeaponReload, playerWeaponReload, AudioBus.Sfx, false, true, 1f);
        AddLegacyCueIfMissing(AudioCueId.PlayerRun, playerRun, AudioBus.Sfx, true, true, playerRunVolume);
        AddLegacyCueIfMissing(AudioCueId.EnemyVoice, enemyVoice, AudioBus.Voice, false, true, enemyVoiceVolume);
        AddLegacyCueIfMissing(AudioCueId.NormalEnemyAttack, normalEnemyAttack, AudioBus.Sfx, false, true, 1f);
        AddLegacyCueIfMissing(AudioCueId.NormalEnemyDeath, normalEnemyDeath, AudioBus.Sfx, false, true, 1f);
        AddLegacyCueIfMissing(AudioCueId.BossEnemyAttack, bossEnemyAttack, AudioBus.Sfx, false, true, 1f);
        AddLegacyCueIfMissing(AudioCueId.BossEnemyDeath, bossEnemyDeath, AudioBus.Sfx, false, true, 1f);
    }

    private void AddLegacyCueIfMissing(AudioCueId cueId, AudioClip clip, AudioBus bus, bool loop, bool spatial, float volume)
    {
        if (cachedCueMap.ContainsKey(cueId) || clip == null)
        {
            return;
        }

        cachedCueMap[cueId] = AudioCueDefinition.CreateRuntime(
            cueId,
            clip,
            bus,
            loop,
            spatial,
            volume,
            1f,
            1f,
            0f,
            defaultMinDistance,
            defaultMaxDistance);
    }
}
