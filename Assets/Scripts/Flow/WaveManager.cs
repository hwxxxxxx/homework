using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private SpawnManager spawnManager;

    [Header("Optional Wave Rewards")]
    [SerializeField] private EffectAsset[] playerEffectsOnWaveCleared;
    [SerializeField] private EffectController playerEffectController;

    private readonly Dictionary<EnemyBase, int> enemyWaveMap = new Dictionary<EnemyBase, int>();

    private int currentWaveIndex;
    private int aliveEnemyCountInCurrentWave;
    private bool waveSpawnFinished;
    private float waveStartTime;
    private WaveConfigAsset.WaveCompletionPolicy currentCompletionPolicy;
    private float currentTimeoutSeconds;

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCleared;

    private void OnEnable()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.OnCombatStarted += HandleCombatStarted;
        }

        if (spawnManager != null)
        {
            spawnManager.OnEnemySpawned += HandleEnemySpawned;
            spawnManager.OnWaveSpawnCompleted += HandleWaveSpawnCompleted;
        }
    }

    private void OnDisable()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.OnCombatStarted -= HandleCombatStarted;
        }

        if (spawnManager != null)
        {
            spawnManager.OnEnemySpawned -= HandleEnemySpawned;
            spawnManager.OnWaveSpawnCompleted -= HandleWaveSpawnCompleted;
        }

        foreach (KeyValuePair<EnemyBase, int> pair in enemyWaveMap)
        {
            if (pair.Key != null)
            {
                pair.Key.OnEnemyDied -= HandleEnemyDied;
            }
        }

        enemyWaveMap.Clear();
    }

    private void Update()
    {
        TryAdvanceByTimeout();
    }

    private void HandleCombatStarted()
    {
        currentWaveIndex = 0;
        aliveEnemyCountInCurrentWave = 0;
        waveSpawnFinished = false;
        waveStartTime = 0f;
        enemyWaveMap.Clear();
        StartCurrentWave();
    }

    private void StartCurrentWave()
    {
        if (spawnManager == null || gameFlowManager == null)
        {
            return;
        }

        if (currentWaveIndex >= spawnManager.WaveCount)
        {
            gameFlowManager.NotifyAllWavesCleared();
            return;
        }

        if (!spawnManager.TryGetWaveEntry(currentWaveIndex, out WaveConfigAsset.WaveEntry waveEntry))
        {
            gameFlowManager.NotifyAllWavesCleared();
            return;
        }

        aliveEnemyCountInCurrentWave = 0;
        waveSpawnFinished = false;
        waveStartTime = Time.time;
        currentCompletionPolicy = waveEntry.completionPolicy;
        currentTimeoutSeconds = Mathf.Max(0f, waveEntry.timeoutSeconds);

        OnWaveStarted?.Invoke(currentWaveIndex);
        spawnManager.SpawnWave(currentWaveIndex);
    }

    private void HandleEnemySpawned(int waveIndex, EnemyBase enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (enemyWaveMap.TryGetValue(enemy, out int oldWaveIndex))
        {
            if (oldWaveIndex == currentWaveIndex)
            {
                return;
            }

            enemy.OnEnemyDied -= HandleEnemyDied;
            enemyWaveMap.Remove(enemy);
        }

        enemyWaveMap[enemy] = waveIndex;
        enemy.OnEnemyDied += HandleEnemyDied;

        if (waveIndex == currentWaveIndex)
        {
            aliveEnemyCountInCurrentWave++;
        }
    }

    private void HandleEnemyDied(EnemyBase enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.OnEnemyDied -= HandleEnemyDied;

        if (!enemyWaveMap.TryGetValue(enemy, out int waveIndex))
        {
            return;
        }

        enemyWaveMap.Remove(enemy);
        if (waveIndex != currentWaveIndex)
        {
            return;
        }

        aliveEnemyCountInCurrentWave = Mathf.Max(0, aliveEnemyCountInCurrentWave - 1);
        TryCompleteCurrentWave();
    }

    private void HandleWaveSpawnCompleted(int waveIndex)
    {
        if (waveIndex != currentWaveIndex)
        {
            return;
        }

        waveSpawnFinished = true;
        TryCompleteCurrentWave();
    }

    private void TryAdvanceByTimeout()
    {
        if (spawnManager == null || currentWaveIndex >= spawnManager.WaveCount)
        {
            return;
        }

        if (currentCompletionPolicy != WaveConfigAsset.WaveCompletionPolicy.AllEnemiesDeadOrTimeout)
        {
            return;
        }

        if (currentTimeoutSeconds <= 0f || Time.time < waveStartTime + currentTimeoutSeconds)
        {
            return;
        }

        spawnManager.CancelCurrentWaveSpawn();
        waveSpawnFinished = true;
        CompleteCurrentWave();
    }

    private void TryCompleteCurrentWave()
    {
        if (!waveSpawnFinished || aliveEnemyCountInCurrentWave > 0)
        {
            return;
        }

        CompleteCurrentWave();
    }

    private void CompleteCurrentWave()
    {
        OnWaveCleared?.Invoke(currentWaveIndex);
        ApplyEffectToPlayer(GetWaveReward(currentWaveIndex));
        currentWaveIndex++;
        StartCurrentWave();
    }

    private EffectAsset GetWaveReward(int waveIndex)
    {
        if (playerEffectsOnWaveCleared == null || waveIndex < 0 || waveIndex >= playerEffectsOnWaveCleared.Length)
        {
            return null;
        }

        return playerEffectsOnWaveCleared[waveIndex];
    }

    private void ApplyEffectToPlayer(EffectAsset effectAsset)
    {
        if (effectAsset == null)
        {
            return;
        }

        EffectController effectController = ResolvePlayerEffectController();
        if (effectController == null)
        {
            return;
        }

        effectController.ApplyEffect(effectAsset, gameObject);
    }

    private EffectController ResolvePlayerEffectController()
    {
        if (playerEffectController != null)
        {
            return playerEffectController;
        }

        if (gameFlowManager == null || gameFlowManager.PlayerStatsRef == null)
        {
            return null;
        }

        playerEffectController = gameFlowManager.PlayerStatsRef.GetComponent<EffectController>();
        return playerEffectController;
    }
}
