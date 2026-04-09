using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Progress/Buff Drop Table", fileName = "BuffDropTable")]
public class BuffDropTableAsset : ScriptableObject
{
    [Serializable]
    public struct BuffEntry
    {
        public EffectAsset effect;
        public int weight;
        public int minStacks;
        public int maxStacks;
    }

    [SerializeField] private int dropChancePercent = 25;
    [SerializeField] private List<BuffEntry> entries = new List<BuffEntry>();

    public bool TryRoll(out EffectAsset effect, out int stackCount)
    {
        effect = null;
        stackCount = 0;

        if (entries == null || entries.Count == 0)
        {
            return false;
        }

        if (UnityEngine.Random.Range(0, 100) >= Mathf.Clamp(dropChancePercent, 0, 100))
        {
            return false;
        }

        int totalWeight = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            BuffEntry entry = entries[i];
            if (entry.effect == null)
            {
                continue;
            }

            int weight = Mathf.Max(0, entry.weight);
            if (weight <= 0)
            {
                continue;
            }

            totalWeight += weight;
        }

        if (totalWeight <= 0)
        {
            return false;
        }

        int weightRoll = UnityEngine.Random.Range(0, totalWeight);
        int cumulativeWeight = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            BuffEntry entry = entries[i];
            if (entry.effect == null)
            {
                continue;
            }

            int weight = Mathf.Max(0, entry.weight);
            if (weight <= 0)
            {
                continue;
            }

            cumulativeWeight += weight;
            if (weightRoll >= cumulativeWeight)
            {
                continue;
            }

            int minStacks = Mathf.Max(1, entry.minStacks);
            int maxStacks = Mathf.Max(minStacks, entry.maxStacks);
            effect = entry.effect;
            stackCount = UnityEngine.Random.Range(minStacks, maxStacks + 1);
            return true;
        }

        return false;
    }
}
