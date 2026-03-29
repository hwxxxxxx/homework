using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Serializable]
    public class WaveDefinition
    {
        public EnemyBase enemyPrefab;
        public int enemyCount = 3;
        public float spawnInterval = 0.3f;
        public int prewarmCount = 0;
    }

    [Header("Spawn Setup")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private List<WaveDefinition> waves = new List<WaveDefinition>();
    [SerializeField] private bool randomSpawnPoint = true;
    [SerializeField] private bool prewarmPoolsOnStart = true;

    private Coroutine spawningCoroutine;

    public event Action<EnemyBase> OnEnemySpawned;
    public event Action<int> OnWaveSpawnCompleted;

    public int WaveCount => waves.Count;

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
        if (waveIndex < 0 || waveIndex >= waves.Count)
        {
            Debug.LogWarning($"SpawnManager: invalid wave index {waveIndex}.");
            return;
        }

        if (spawnPoints.Count == 0)
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

    private IEnumerator SpawnWaveRoutine(int waveIndex)
    {
        WaveDefinition wave = waves[waveIndex];
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

            OnEnemySpawned?.Invoke(spawnedEnemy);

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
            int index = UnityEngine.Random.Range(0, spawnPoints.Count);
            return spawnPoints[index];
        }

        return spawnPoints[spawnIndex % spawnPoints.Count];
    }

    private void PrewarmPools()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            WaveDefinition wave = waves[i];
            if (wave.enemyPrefab == null)
            {
                continue;
            }

            int prewarmCount = wave.prewarmCount > 0 ? wave.prewarmCount : wave.enemyCount;
            PoolService.Warmup(wave.enemyPrefab.gameObject, prewarmCount);
        }
    }
}
