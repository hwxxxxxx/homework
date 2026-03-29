using System;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private SpawnManager spawnManager;

    private int currentWaveIndex;
    private int aliveEnemyCount;
    private bool waveSpawnFinished;

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
    }

    private void HandleCombatStarted()
    {
        currentWaveIndex = 0;
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

        aliveEnemyCount = 0;
        waveSpawnFinished = false;
        OnWaveStarted?.Invoke(currentWaveIndex);
        spawnManager.SpawnWave(currentWaveIndex);
    }

    private void HandleEnemySpawned(EnemyBase enemy)
    {
        if (enemy == null)
        {
            return;
        }

        aliveEnemyCount++;
        enemy.OnEnemyDied += HandleEnemyDied;
    }

    private void HandleEnemyDied(EnemyBase enemy)
    {
        if (enemy != null)
        {
            enemy.OnEnemyDied -= HandleEnemyDied;
        }

        aliveEnemyCount = Mathf.Max(0, aliveEnemyCount - 1);
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

    private void TryCompleteCurrentWave()
    {
        if (!waveSpawnFinished || aliveEnemyCount > 0)
        {
            return;
        }

        OnWaveCleared?.Invoke(currentWaveIndex);
        currentWaveIndex++;

        if (currentWaveIndex >= spawnManager.WaveCount)
        {
            gameFlowManager.NotifyAllWavesCleared();
            return;
        }

        StartCurrentWave();
    }
}
