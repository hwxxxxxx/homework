using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEncounterController : MonoBehaviour
{
    [Header("Encounter")]
    [SerializeField] private WaveConfigAsset waveConfig;
    [SerializeField] private Transform spawnPointsRoot;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool randomSpawnPoint = true;
    [SerializeField] private bool autoCompleteRunOnAllWaves = true;

    private readonly List<EnemyBase> aliveEnemies = new List<EnemyBase>();
    private Coroutine encounterCoroutine;
    private RuntimeShell runtimeShell;

    public void StartEncounter(RuntimeShell shell)
    {
        if (shell == null)
        {
            Debug.LogError("LevelEncounterController requires RuntimeShell to start encounter.");
            return;
        }

        StopEncounter();
        runtimeShell = shell;

        WaveConfigAsset resolvedConfig = ResolveWaveConfig();
        if (resolvedConfig == null)
        {
            return;
        }

        Transform[] resolvedSpawnPoints = ResolveSpawnPoints();
        if (resolvedSpawnPoints.Length == 0)
        {
            Debug.LogError($"LevelEncounterController in scene '{gameObject.scene.name}' has no spawn points.");
            return;
        }

        PrewarmPools(resolvedConfig);
        encounterCoroutine = StartCoroutine(RunEncounterRoutine(resolvedConfig, resolvedSpawnPoints));
    }

    public void StopEncounter()
    {
        if (encounterCoroutine != null)
        {
            StopCoroutine(encounterCoroutine);
            encounterCoroutine = null;
        }

        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            EnemyBase enemy = aliveEnemies[i];
            if (enemy != null)
            {
                enemy.OnEnemyDied -= HandleEnemyDied;
            }
        }

        aliveEnemies.Clear();
        runtimeShell = null;
    }

    private void OnDisable()
    {
        StopEncounter();
    }

    private IEnumerator RunEncounterRoutine(WaveConfigAsset config, Transform[] resolvedSpawnPoints)
    {
        for (int waveIndex = 0; waveIndex < config.WaveCount; waveIndex++)
        {
            if (!config.TryGetWave(waveIndex, out WaveConfigAsset.WaveEntry wave))
            {
                continue;
            }

            if (wave.enemyPrefab == null || wave.enemyCount <= 0)
            {
                continue;
            }

            int aliveCountInWave = 0;
            float waveStartTime = Time.time;

            for (int spawnIndex = 0; spawnIndex < wave.enemyCount; spawnIndex++)
            {
                Transform spawnPoint = SelectSpawnPoint(resolvedSpawnPoints, spawnIndex);
                EnemyBase enemy = PoolService.Spawn(
                    wave.enemyPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation,
                    null,
                    0
                );

                if (enemy != null)
                {
                    ConfigureSpawnedEnemy(enemy);
                    enemy.OnEnemyDied += HandleEnemyDied;
                    aliveEnemies.Add(enemy);
                    aliveCountInWave++;
                }

                if (wave.spawnInterval > 0f)
                {
                    yield return new WaitForSeconds(wave.spawnInterval);
                }
            }

            if (aliveCountInWave <= 0)
            {
                continue;
            }

            while (aliveCountInWave > 0)
            {
                aliveCountInWave = CountAliveEnemies();

                bool timedOut =
                    wave.completionPolicy == WaveConfigAsset.WaveCompletionPolicy.AllEnemiesDeadOrTimeout &&
                    wave.timeoutSeconds > 0f &&
                    Time.time >= waveStartTime + wave.timeoutSeconds;

                if (timedOut)
                {
                    break;
                }

                yield return null;
            }
        }

        encounterCoroutine = null;
        if (autoCompleteRunOnAllWaves)
        {
            CompleteRunWithVictory();
        }
    }

    private WaveConfigAsset ResolveWaveConfig()
    {
        if (waveConfig != null)
        {
            return waveConfig;
        }

        Debug.LogError($"LevelEncounterController in scene '{gameObject.scene.name}' has no WaveConfig assigned.");
        return null;
    }

    private Transform[] ResolveSpawnPoints()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints;
        }

        Transform root = spawnPointsRoot;
        if (root == null || root.childCount == 0)
        {
            return new Transform[0];
        }

        Transform[] result = new Transform[root.childCount];
        for (int i = 0; i < root.childCount; i++)
        {
            result[i] = root.GetChild(i);
        }

        return result;
    }

    private Transform SelectSpawnPoint(Transform[] resolvedSpawnPoints, int spawnIndex)
    {
        if (randomSpawnPoint)
        {
            int randomIndex = Random.Range(0, resolvedSpawnPoints.Length);
            return resolvedSpawnPoints[randomIndex];
        }

        return resolvedSpawnPoints[spawnIndex % resolvedSpawnPoints.Length];
    }

    private void ConfigureSpawnedEnemy(EnemyBase enemy)
    {
        Transform playerTarget = runtimeShell.PersistentRoot.PlayerStats.transform;

        EnemyAIController aiController = enemy.GetComponent<EnemyAIController>();
        if (aiController != null)
        {
            aiController.SetTarget(playerTarget);
        }

        EnemyDeathDropListenerBase[] listeners = enemy.GetComponents<EnemyDeathDropListenerBase>();
        for (int i = 0; i < listeners.Length; i++)
        {
            listeners[i].SetRunContextService(runtimeShell.RunContextService);
        }

        BuffDropOnDeath buffDrop = enemy.GetComponent<BuffDropOnDeath>();
        if (buffDrop != null)
        {
            buffDrop.SetTargetEffectController(runtimeShell.PersistentRoot.PlayerEffectController);
        }

        EnemyWorldHealthBar healthBar = enemy.GetComponent<EnemyWorldHealthBar>();
        if (healthBar != null)
        {
            healthBar.ConfigurePresentation(Camera.main);
        }
    }

    private void HandleEnemyDied(EnemyBase enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.OnEnemyDied -= HandleEnemyDied;
        aliveEnemies.Remove(enemy);
    }

    private int CountAliveEnemies()
    {
        int aliveCount = 0;
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            EnemyBase enemy = aliveEnemies[i];
            if (enemy == null)
            {
                aliveEnemies.RemoveAt(i);
                continue;
            }

            aliveCount++;
        }

        return aliveCount;
    }

    private void PrewarmPools(WaveConfigAsset config)
    {
        if (config == null)
        {
            return;
        }

        IReadOnlyList<WaveConfigAsset.WaveEntry> waves = config.GetWaves();
        for (int i = 0; i < waves.Count; i++)
        {
            WaveConfigAsset.WaveEntry wave = waves[i];
            if (wave.enemyPrefab == null)
            {
                continue;
            }

            int prewarmCount = wave.prewarmCount > 0 ? wave.prewarmCount : wave.enemyCount;
            if (prewarmCount > 0)
            {
                PoolService.Warmup(wave.enemyPrefab.gameObject, prewarmCount);
            }
        }
    }

    private void CompleteRunWithVictory()
    {
        if (runtimeShell == null || !runtimeShell.RunContextService.IsRunActive)
        {
            return;
        }

        if (runtimeShell.GameStateService.CurrentState != GameStateId.InRun)
        {
            return;
        }

        runtimeShell.RunContextService.EndRun(true);
        runtimeShell.PauseController.SetPaused(false);
        if (!runtimeShell.GameStateService.TrySetState(GameStateId.RunResult))
        {
            Debug.LogError("Failed to transition to RunResult after encounter completion.");
        }
    }
}
