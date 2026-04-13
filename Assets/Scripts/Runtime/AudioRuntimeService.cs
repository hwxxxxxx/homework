using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioRuntimeService : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private Transform sfxEmitterRoot;

    [Header("SFX Pool")]
    [SerializeField] private int sfxEmitterCount = 12;

    private readonly List<AudioSource> sfxEmitters = new List<AudioSource>();
    private IDisposable weaponFiredSubscription;
    private IDisposable enemyAttackSubscription;
    private IDisposable enemyDiedSubscription;
    private int nextSfxEmitterIndex;

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

        BuildSfxEmitterPool();
        ConfigureBgmSource();
    }

    private void OnEnable()
    {
        gameStateService.OnStateChanged += HandleGameStateChanged;
        weaponFiredSubscription = EventBus.Subscribe<WeaponFiredEvent>(HandleWeaponFired);
        enemyAttackSubscription = EventBus.Subscribe<EnemyAttackEvent>(HandleEnemyAttack);
        enemyDiedSubscription = EventBus.Subscribe<EnemyDiedEvent>(HandleEnemyDied);
        ApplyBgmPolicy(gameStateService.CurrentState);
    }

    private void OnDisable()
    {
        gameStateService.OnStateChanged -= HandleGameStateChanged;
        weaponFiredSubscription?.Dispose();
        enemyAttackSubscription?.Dispose();
        enemyDiedSubscription?.Dispose();
        weaponFiredSubscription = null;
        enemyAttackSubscription = null;
        enemyDiedSubscription = null;
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
            sfxEmitters.Add(emitter);
        }
    }

    private void ConfigureBgmSource()
    {
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
    }

    private void HandleGameStateChanged(GameStateId _, GameStateId current)
    {
        ApplyBgmPolicy(current);
    }

    private void ApplyBgmPolicy(GameStateId state)
    {
        if (state == GameStateId.Boot)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            return;
        }

        AudioConfigAsset config = AudioConfigProvider.Config;
        if (bgmSource.clip != config.BgmMainLoop)
        {
            bgmSource.clip = config.BgmMainLoop;
        }

        bgmSource.volume = config.BgmVolume;
        if (!bgmSource.isPlaying && bgmSource.clip != null)
        {
            bgmSource.Play();
        }
    }

    private void HandleWeaponFired(WeaponFiredEvent gameEvent)
    {
        AudioConfigAsset config = AudioConfigProvider.Config;
        PlaySfx(config.PlayerWeaponFire, gameEvent.Position, config);
    }

    private void HandleEnemyAttack(EnemyAttackEvent gameEvent)
    {
        AudioConfigAsset config = AudioConfigProvider.Config;
        AudioClip clip = gameEvent.IsBoss ? config.BossEnemyAttack : config.NormalEnemyAttack;
        PlaySfx(clip, gameEvent.Position, config);
    }

    private void HandleEnemyDied(EnemyDiedEvent gameEvent)
    {
        AudioConfigAsset config = AudioConfigProvider.Config;
        AudioClip clip = gameEvent.IsBoss ? config.BossEnemyDeath : config.NormalEnemyDeath;
        PlaySfx(clip, gameEvent.Position, config);
    }

    private void PlaySfx(AudioClip clip, Vector3 position, AudioConfigAsset config)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource emitter = sfxEmitters[nextSfxEmitterIndex];
        nextSfxEmitterIndex = (nextSfxEmitterIndex + 1) % sfxEmitters.Count;

        emitter.transform.position = position;
        emitter.spatialBlend = config.SpatialBlend;
        emitter.minDistance = config.MinDistance;
        emitter.maxDistance = config.MaxDistance;
        emitter.volume = config.SfxVolume;
        emitter.clip = clip;
        emitter.Play();
    }

    public void SetBgmVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    public float GetBgmVolume()
    {
        return bgmSource.volume;
    }
}
