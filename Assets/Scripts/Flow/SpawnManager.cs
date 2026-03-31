using System;
using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Setup")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private WaveConfigAsset waveConfig;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private bool randomSpawnPoint = true;
    [SerializeField] private bool prewarmPoolsOnStart = true;

    private Coroutine spawningCoroutine;

    public event Action<int, EnemyBase> OnEnemySpawned;
    public event Action<int> OnWaveSpawnCompleted;

    public int WaveCount => waveConfig != null ? waveConfig.WaveCount : 0;

    private void Start()
    {
        if (!prewarmPoolsOnStart)
        {
            return;
        }

        PrewarmPools();
    }

    public void SpawnWave(int waveIndex)
    {
        if (!TryGetWave(waveIndex, out WaveConfigAsset.WaveEntry _))
        {
            Debug.LogWarning($"SpawnManager: invalid wave index {waveIndex}.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("SpawnManager: no spawn points configured.");
            OnWaveSpawnCompleted?.Invoke(waveIndex);
            return;
        }

        if (spawningCoroutine != null)
        {
            StopCoroutine(spawningCoroutine);
        }

        spawningCoroutine = StartCoroutine(SpawnWaveRoutine(waveIndex));
    }

    public void CancelCurrentWaveSpawn()
    {
        if (spawningCoroutine == null)
        {
            return;
        }

        StopCoroutine(spawningCoroutine);
        spawningCoroutine = null;
    }

    public bool TryGetWaveEntry(int waveIndex, out WaveConfigAsset.WaveEntry waveEntry)
    {
        return TryGetWave(waveIndex, out waveEntry);
    }

    private IEnumerator SpawnWaveRoutine(int waveIndex)
    {
        if (!TryGetWave(waveIndex, out WaveConfigAsset.WaveEntry wave))
        {
            OnWaveSpawnCompleted?.Invoke(waveIndex);
            yield break;
        }

        if (wave.enemyPrefab == null || wave.enemyCount <= 0)
        {
            OnWaveSpawnCompleted?.Invoke(waveIndex);
            yield break;
        }

        for (int i = 0; i < wave.enemyCount; i++)
        {
            Transform spawnPoint = GetSpawnPoint(i);
            EnemyBase spawnedEnemy = PoolService.Spawn(
                wave.enemyPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                null,
                0
            );
            ConfigureSpawnedEnemy(spawnedEnemy);

            OnEnemySpawned?.Invoke(waveIndex, spawnedEnemy);

            if (wave.spawnInterval > 0f)
            {
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        OnWaveSpawnCompleted?.Invoke(waveIndex);
        spawningCoroutine = null;
    }

    private Transform GetSpawnPoint(int spawnIndex)
    {
        if (randomSpawnPoint)
        {
            int index = UnityEngine.Random.Range(0, spawnPoints.Length);
            return spawnPoints[index];
        }

        return spawnPoints[spawnIndex % spawnPoints.Length];
    }

    private void PrewarmPools()
    {
        if (waveConfig == null)
        {
            return;
        }

        var waves = waveConfig.GetWaves();
        for (int i = 0; i < waves.Count; i++)
        {
            WaveConfigAsset.WaveEntry wave = waves[i];
            if (wave.enemyPrefab == null)
            {
                continue;
            }

            int prewarmCount = wave.prewarmCount > 0 ? wave.prewarmCount : wave.enemyCount;
            PoolService.Warmup(wave.enemyPrefab.gameObject, prewarmCount);
        }
    }

    private bool TryGetWave(int waveIndex, out WaveConfigAsset.WaveEntry wave)
    {
        if (waveConfig == null)
        {
            wave = default;
            return false;
        }

        return waveConfig.TryGetWave(waveIndex, out wave);
    }

    private void ConfigureSpawnedEnemy(EnemyBase enemy)
    {
        if (enemy == null || playerTarget == null)
        {
            return;
        }

        EnemyAIController ai = enemy.GetComponent<EnemyAIController>();
        if (ai != null)
        {
            ai.SetTarget(playerTarget);
        }
    }
}
