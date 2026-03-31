using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Progress/Drop Table", fileName = "DropTable")]
public class DropTableAsset : ScriptableObject
{
    [Serializable]
    public struct DropEntry
    {
        public FragmentType fragmentType;
        public int weight;
        public int minAmount;
        public int maxAmount;
        public bool guaranteed;
    }

    [SerializeField] private List<DropEntry> entries = new List<DropEntry>();

    public IReadOnlyList<DropEntry> Entries => entries;

    public Dictionary<FragmentType, int> Roll()
    {
        var result = new Dictionary<FragmentType, int>();
        if (entries == null || entries.Count == 0)
        {
            return result;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            DropEntry entry = entries[i];
            int min = Mathf.Max(0, entry.minAmount);
            int max = Mathf.Max(min, entry.maxAmount);
            if (entry.guaranteed)
            {
                AddResult(result, entry.fragmentType, UnityEngine.Random.Range(min, max + 1));
                continue;
            }

            int weight = Mathf.Max(0, entry.weight);
            if (weight <= 0)
            {
                continue;
            }

            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < Mathf.Clamp(weight, 0, 100))
            {
                AddResult(result, entry.fragmentType, UnityEngine.Random.Range(min, max + 1));
            }
        }

        return result;
    }

    private static void AddResult(Dictionary<FragmentType, int> result, FragmentType type, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (!result.ContainsKey(type))
        {
            result[type] = 0;
        }

        result[type] += amount;
    }
}
