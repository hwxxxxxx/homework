using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioRuntimeService : MonoBehaviour
{
    private const string MasterVolumePrefKey = "Settings.Audio.Master";
    private const string MusicVolumePrefKey = "Settings.Audio.Music";
    private const string SfxVolumePrefKey = "Settings.Audio.Sfx";
    private const string UiVolumePrefKey = "Settings.Audio.Ui";
    private const string VoiceVolumePrefKey = "Settings.Audio.Voice";
    private const string AmbienceVolumePrefKey = "Settings.Audio.Ambience";

    [Header("References")]
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private Transform sfxEmitterRoot;

    [Header("SFX Pool")]
    [SerializeField] private int sfxEmitterCount = 16;

    [Header("Mixer (Optional)")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup masterGroup;
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup uiGroup;
    [SerializeField] private AudioMixerGroup voiceGroup;
    [SerializeField] private AudioMixerGroup ambienceGroup;
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SfxVolume";
    [SerializeField] private string uiVolumeParam = "UiVolume";
    [SerializeField] private string voiceVolumeParam = "VoiceVolume";
    [SerializeField] private string ambienceVolumeParam = "AmbienceVolume";

    private readonly List<AudioSource> sfxEmitters = new List<AudioSource>();
    private readonly Dictionary<AudioCueId, float> cueCooldownUntil = new Dictionary<AudioCueId, float>();
    private readonly HashSet<int> boundButtonIds = new HashSet<int>();

    private IDisposable weaponFiredSubscription;
    private IDisposable weaponReloadStartedSubscription;
    private IDisposable playerHitEnemySubscription;
    private IDisposable enemyAttackSubscription;
    private IDisposable enemyDiedSubscription;

    private int nextSfxEmitterIndex;
    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;
    private float uiVolume = 1f;
    private float voiceVolume = 1f;
    private float ambienceVolume = 1f;

    private void Awake()
    {
        if (gameStateService == null || bgmSource == null || sfxEmitterRoot == null)
        {
            throw new InvalidOperationException("AudioRuntimeService has unassigned required serialized references.");
        }

        if (sfxEmitterCount <= 0)
        {
            throw new InvalidOperationException("AudioRuntimeService requires sfxEmitterCount > 0.");
        }

        EnsureUiSource();
        BuildSfxEmitterPool();
        LoadVolumeSettings();
        ConfigureSources();
        ApplyVolumeSettings(immediateSave: false);
    }

    private void OnEnable()
    {
        gameStateService.OnStateChanged += HandleGameStateChanged;
        SceneManager.sceneLoaded += HandleSceneLoaded;

        weaponFiredSubscription = EventBus.Subscribe<WeaponFiredEvent>(HandleWeaponFired);
        weaponReloadStartedSubscription = EventBus.Subscribe<WeaponReloadStartedEvent>(HandleWeaponReloadStarted);
        playerHitEnemySubscription = EventBus.Subscribe<PlayerHitEnemyEvent>(HandlePlayerHitEnemy);
        enemyAttackSubscription = EventBus.Subscribe<EnemyAttackEvent>(HandleEnemyAttack);
        enemyDiedSubscription = EventBus.Subscribe<EnemyDiedEvent>(HandleEnemyDied);

        BindUiButtonsInLoadedScenes();
        ApplyBgmPolicy(gameStateService.CurrentState);
    }

    private void OnDisable()
    {
        gameStateService.OnStateChanged -= HandleGameStateChanged;
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        weaponFiredSubscription?.Dispose();
        weaponReloadStartedSubscription?.Dispose();
        playerHitEnemySubscription?.Dispose();
        enemyAttackSubscription?.Dispose();
        enemyDiedSubscription?.Dispose();

        weaponFiredSubscription = null;
        weaponReloadStartedSubscription = null;
        playerHitEnemySubscription = null;
        enemyAttackSubscription = null;
        enemyDiedSubscription = null;
    }

    public bool PlayCue2D(AudioCueId cueId)
    {
        if (!TryResolveCue(cueId, out AudioCueDefinition cue) || cue.Spatial)
        {
            return false;
        }

        if (!TryConsumeCooldown(cue))
        {
            return false;
        }

        if (!cue.TryGetRandomClip(out AudioClip clip))
        {
            return false;
        }

        AudioSource targetSource = cue.Bus == AudioBus.Music ? bgmSource : uiSource;
        if (targetSource == null)
        {
            return false;
        }

        targetSource.outputAudioMixerGroup = GetMixerGroup(cue.Bus);
        targetSource.pitch = UnityEngine.Random.Range(cue.PitchMin, cue.PitchMax);

        float volume = GetCueFinalVolume(cue, 1f);
        if (cue.Loop)
        {
            targetSource.loop = true;
            targetSource.clip = clip;
            targetSource.volume = volume;
            if (!targetSource.isPlaying)
            {
                targetSource.Play();
            }
        }
        else
        {
            targetSource.PlayOneShot(clip, volume);
        }

        return true;
    }

    public bool PlayCueAt(AudioCueId cueId, Vector3 position, float volumeScale = 1f)
    {
        if (!TryResolveCue(cueId, out AudioCueDefinition cue))
        {
            return false;
        }

        if (!TryConsumeCooldown(cue))
        {
            return false;
        }

        if (!cue.TryGetRandomClip(out AudioClip clip))
        {
            return false;
        }

        if (!cue.Spatial)
        {
            return PlayCue2D(cueId);
        }

        AudioSource emitter = sfxEmitters[nextSfxEmitterIndex];
        nextSfxEmitterIndex = (nextSfxEmitterIndex + 1) % sfxEmitters.Count;

        emitter.transform.position = position;
        emitter.outputAudioMixerGroup = GetMixerGroup(cue.Bus);
        emitter.loop = cue.Loop;
        emitter.spatialBlend = 1f;
        emitter.minDistance = cue.MinDistance;
        emitter.maxDistance = cue.MaxDistance;
        emitter.pitch = UnityEngine.Random.Range(cue.PitchMin, cue.PitchMax);
        emitter.volume = GetCueFinalVolume(cue, volumeScale);
        emitter.clip = clip;
        emitter.Play();
        return true;
    }

    public bool PlayCueFollowing(AudioCueId cueId, Transform followTarget, float volumeScale = 1f)
    {
        if (followTarget == null)
        {
            return false;
        }

        if (!TryResolveCue(cueId, out AudioCueDefinition cue))
        {
            return false;
        }

        if (!TryConsumeCooldown(cue))
        {
            return false;
        }

        if (!cue.TryGetRandomClip(out AudioClip clip))
        {
            return false;
        }

        if (!cue.Spatial)
        {
            return PlayCue2D(cueId);
        }

        GameObject emitterObject = new GameObject($"FollowSfx_{cueId}");
        emitterObject.transform.SetParent(followTarget, false);
        emitterObject.transform.localPosition = Vector3.zero;

        AudioSource emitter = emitterObject.AddComponent<AudioSource>();
        emitter.playOnAwake = false;
        emitter.loop = cue.Loop;
        emitter.spatialBlend = 1f;
        emitter.minDistance = cue.MinDistance;
        emitter.maxDistance = cue.MaxDistance;
        emitter.outputAudioMixerGroup = GetMixerGroup(cue.Bus);
        emitter.pitch = UnityEngine.Random.Range(cue.PitchMin, cue.PitchMax);
        emitter.volume = GetCueFinalVolume(cue, volumeScale);
        emitter.clip = clip;
        emitter.Play();

        float safePitch = Mathf.Max(0.01f, Mathf.Abs(emitter.pitch));
        float life = cue.Loop ? 0.1f : (clip.length / safePitch + 0.1f);
        Destroy(emitterObject, life);
        return true;
    }

    public bool PlayUiClickSfx()
    {
        return PlayCue2D(AudioCueId.UiClick);
    }

    public float GetEnemyVoiceInterval()
    {
        if (!TryGetAudioConfig(out AudioConfigAsset config))
        {
            return 4f;
        }

        return UnityEngine.Random.Range(config.EnemyVoiceMinInterval, config.EnemyVoiceMaxInterval);
    }

    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSfxVolume() => sfxVolume;
    public float GetUiVolume() => uiVolume;
    public float GetVoiceVolume() => voiceVolume;
    public float GetAmbienceVolume() => ambienceVolume;

    public void SetMasterVolume(float value) => SetVolume(ref masterVolume, value);
    public void SetMusicVolume(float value) => SetVolume(ref musicVolume, value);
    public void SetSfxVolume(float value) => SetVolume(ref sfxVolume, value);
    public void SetUiVolume(float value) => SetVolume(ref uiVolume, value);
    public void SetVoiceVolume(float value) => SetVolume(ref voiceVolume, value);
    public void SetAmbienceVolume(float value) => SetVolume(ref ambienceVolume, value);

    public void SaveVolumeSettings()
    {
        PlayerPrefs.Save();
    }

    public void SetBgmVolume(float value)
    {
        SetMasterVolume(value);
    }

    public float GetBgmVolume()
    {
        return GetMasterVolume();
    }

    private void HandleGameStateChanged(GameStateId _, GameStateId current)
    {
        ApplyBgmPolicy(current);
    }

    private void HandleWeaponFired(WeaponFiredEvent gameEvent)
    {
        PlayCueAt(AudioCueId.PlayerWeaponFire, gameEvent.Position);
    }

    private void HandleWeaponReloadStarted(WeaponReloadStartedEvent gameEvent)
    {
        Transform followTarget = gameEvent.SourceObject != null ? gameEvent.SourceObject.transform : null;
        if (!PlayCueFollowing(AudioCueId.PlayerWeaponReload, followTarget))
        {
            PlayCueAt(AudioCueId.PlayerWeaponReload, gameEvent.Position);
        }
    }

    private void HandlePlayerHitEnemy(PlayerHitEnemyEvent gameEvent)
    {
        PlayCueAt(AudioCueId.PlayerHitEnemy, gameEvent.Position);
    }

    private void HandleEnemyAttack(EnemyAttackEvent gameEvent)
    {
        PlayCueAt(gameEvent.IsBoss ? AudioCueId.BossEnemyAttack : AudioCueId.NormalEnemyAttack, gameEvent.Position);
    }

    private void HandleEnemyDied(EnemyDiedEvent gameEvent)
    {
        PlayCueAt(gameEvent.IsBoss ? AudioCueId.BossEnemyDeath : AudioCueId.NormalEnemyDeath, gameEvent.Position);
    }

    private void HandleSceneLoaded(Scene _, LoadSceneMode __)
    {
        boundButtonIds.Clear();
        BindUiButtonsInLoadedScenes();
    }

    private void ApplyBgmPolicy(GameStateId state)
    {
        if (state == GameStateId.Boot)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            return;
        }

        PlayCue2D(AudioCueId.BgmMainLoop);
    }

    private void EnsureUiSource()
    {
        if (uiSource != null)
        {
            return;
        }

        GameObject go = new GameObject("UiAudioSource");
        go.transform.SetParent(transform, false);
        uiSource = go.AddComponent<AudioSource>();
    }

    private void ConfigureSources()
    {
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
        bgmSource.outputAudioMixerGroup = GetMixerGroup(AudioBus.Music);

        uiSource.playOnAwake = false;
        uiSource.loop = false;
        uiSource.spatialBlend = 0f;
        uiSource.outputAudioMixerGroup = GetMixerGroup(AudioBus.Ui);
    }

    private void BuildSfxEmitterPool()
    {
        sfxEmitters.Clear();
        for (int i = 0; i < sfxEmitterCount; i++)
        {
            GameObject emitterObject = new GameObject($"SfxEmitter_{i:D2}");
            emitterObject.transform.SetParent(sfxEmitterRoot, false);
            AudioSource emitter = emitterObject.AddComponent<AudioSource>();
            emitter.playOnAwake = false;
            emitter.loop = false;
            emitter.spatialBlend = 1f;
            emitter.outputAudioMixerGroup = GetMixerGroup(AudioBus.Sfx);
            sfxEmitters.Add(emitter);
        }
    }

    private bool TryResolveCue(AudioCueId cueId, out AudioCueDefinition cue)
    {
        cue = null;
        if (cueId == AudioCueId.None || !TryGetAudioConfig(out AudioConfigAsset config))
        {
            return false;
        }

        return config.TryGetCue(cueId, out cue);
    }

    private bool TryConsumeCooldown(AudioCueDefinition cue)
    {
        if (cue.Cooldown <= 0f)
        {
            return true;
        }

        float now = Time.time;
        if (cueCooldownUntil.TryGetValue(cue.CueId, out float until) && now < until)
        {
            return false;
        }

        cueCooldownUntil[cue.CueId] = now + cue.Cooldown;
        return true;
    }

    private float GetCueFinalVolume(AudioCueDefinition cue, float volumeScale)
    {
        float busGain = audioMixer == null ? GetBusGain(cue.Bus) : 1f;
        return Mathf.Clamp01(cue.Volume * busGain * Mathf.Clamp01(volumeScale));
    }

    private float GetBusGain(AudioBus bus)
    {
        switch (bus)
        {
            case AudioBus.Music:
                return masterVolume * musicVolume;
            case AudioBus.Sfx:
                return masterVolume * sfxVolume;
            case AudioBus.Ui:
                return masterVolume * uiVolume;
            case AudioBus.Voice:
                return masterVolume * voiceVolume;
            case AudioBus.Ambience:
                return masterVolume * ambienceVolume;
            default:
                return masterVolume;
        }
    }

    private void SetVolume(ref float target, float value)
    {
        target = Mathf.Clamp01(value);
        ApplyVolumeSettings(immediateSave: false);
    }

    private void LoadVolumeSettings()
    {
        if (!TryGetAudioConfig(out AudioConfigAsset config))
        {
            masterVolume = 1f;
            musicVolume = 1f;
            sfxVolume = 1f;
            uiVolume = 1f;
            voiceVolume = 1f;
            ambienceVolume = 1f;
            return;
        }

        masterVolume = PlayerPrefs.GetFloat(MasterVolumePrefKey, config.DefaultMasterVolume);
        musicVolume = PlayerPrefs.GetFloat(MusicVolumePrefKey, config.DefaultMusicVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumePrefKey, config.DefaultSfxVolume);
        uiVolume = PlayerPrefs.GetFloat(UiVolumePrefKey, config.DefaultUiVolume);
        voiceVolume = PlayerPrefs.GetFloat(VoiceVolumePrefKey, config.DefaultVoiceVolume);
        ambienceVolume = PlayerPrefs.GetFloat(AmbienceVolumePrefKey, config.DefaultAmbienceVolume);
    }

    private void ApplyVolumeSettings(bool immediateSave)
    {
        masterVolume = Mathf.Clamp01(masterVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
        uiVolume = Mathf.Clamp01(uiVolume);
        voiceVolume = Mathf.Clamp01(voiceVolume);
        ambienceVolume = Mathf.Clamp01(ambienceVolume);

        PlayerPrefs.SetFloat(MasterVolumePrefKey, masterVolume);
        PlayerPrefs.SetFloat(MusicVolumePrefKey, musicVolume);
        PlayerPrefs.SetFloat(SfxVolumePrefKey, sfxVolume);
        PlayerPrefs.SetFloat(UiVolumePrefKey, uiVolume);
        PlayerPrefs.SetFloat(VoiceVolumePrefKey, voiceVolume);
        PlayerPrefs.SetFloat(AmbienceVolumePrefKey, ambienceVolume);

        bgmSource.volume = audioMixer == null ? masterVolume * musicVolume : 1f;
        uiSource.volume = audioMixer == null ? masterVolume * uiVolume : 1f;

        if (audioMixer != null)
        {
            SetMixerParam(masterVolumeParam, masterVolume);
            SetMixerParam(musicVolumeParam, musicVolume);
            SetMixerParam(sfxVolumeParam, sfxVolume);
            SetMixerParam(uiVolumeParam, uiVolume);
            SetMixerParam(voiceVolumeParam, voiceVolume);
            SetMixerParam(ambienceVolumeParam, ambienceVolume);
        }

        if (immediateSave)
        {
            PlayerPrefs.Save();
        }
    }

    private void SetMixerParam(string paramName, float linearValue)
    {
        if (audioMixer == null || string.IsNullOrWhiteSpace(paramName))
        {
            return;
        }

        float db = LinearToDb(linearValue);
        audioMixer.SetFloat(paramName, db);
    }

    private static float LinearToDb(float value)
    {
        if (value <= 0.0001f)
        {
            return -80f;
        }

        return Mathf.Log10(value) * 20f;
    }

    private AudioMixerGroup GetMixerGroup(AudioBus bus)
    {
        switch (bus)
        {
            case AudioBus.Music:
                return musicGroup != null ? musicGroup : masterGroup;
            case AudioBus.Sfx:
                return sfxGroup != null ? sfxGroup : masterGroup;
            case AudioBus.Ui:
                return uiGroup != null ? uiGroup : masterGroup;
            case AudioBus.Voice:
                return voiceGroup != null ? voiceGroup : masterGroup;
            case AudioBus.Ambience:
                return ambienceGroup != null ? ambienceGroup : masterGroup;
            default:
                return masterGroup;
        }
    }

    private bool TryGetAudioConfig(out AudioConfigAsset config)
    {
        try
        {
            config = AudioConfigProvider.Config;
            return config != null;
        }
        catch (InvalidOperationException)
        {
            config = null;
            return false;
        }
    }

    private void BindUiButtonsInLoadedScenes()
    {
        int sceneCount = SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.IsValid() && scene.isLoaded)
            {
                BindUiButtonsInScene(scene);
            }
        }
    }

    private void BindUiButtonsInScene(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Button[] buttons = roots[i].GetComponentsInChildren<Button>(true);
            for (int j = 0; j < buttons.Length; j++)
            {
                Button button = buttons[j];
                if (button == null)
                {
                    continue;
                }

                int id = button.gameObject.GetInstanceID();
                if (!boundButtonIds.Add(id))
                {
                    continue;
                }

                if (button.GetComponent<UiClickSfxRelay>() == null)
                {
                    button.gameObject.AddComponent<UiClickSfxRelay>();
                }
            }
        }
    }
}
