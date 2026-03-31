using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Flow/Wave Config", fileName = "WaveConfig")]
public class WaveConfigAsset : ScriptableObject
{
    public enum WaveCompletionPolicy
    {
        AllEnemiesDead = 0,
        AllEnemiesDeadOrTimeout = 1
    }

    [Serializable]
    public struct WaveEntry
    {
        public EnemyBase enemyPrefab;
        public int enemyCount;
        public float spawnInterval;
        public int prewarmCount;
        public WaveCompletionPolicy completionPolicy;
        public float timeoutSeconds;
    }

    [SerializeField] private List<WaveEntry> waves = new List<WaveEntry>();

    public int WaveCount => waves != null ? waves.Count : 0;

    public bool TryGetWave(int index, out WaveEntry wave)
    {
        if (waves == null || index < 0 || index >= waves.Count)
        {
            wave = default;
            return false;
        }

        wave = waves[index];
        return true;
    }

    public IReadOnlyList<WaveEntry> GetWaves()
    {
        return waves;
    }
}
