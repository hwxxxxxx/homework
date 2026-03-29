using System;
using System.Collections.Generic;
using UnityEngine;

public class StatBlock : MonoBehaviour, IModifiableStatProvider
{
    [Serializable]
    public struct StatEntry
    {
        public string statId;
        public float baseValue;
    }

    [SerializeField] private List<StatEntry> baseStats = new List<StatEntry>();

    private readonly Dictionary<string, StatValue> stats = new Dictionary<string, StatValue>();

    public event Action<string, float, float> OnStatChanged;

    private void Awake()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        stats.Clear();

        for (int i = 0; i < baseStats.Count; i++)
        {
            StatEntry entry = baseStats[i];
            if (string.IsNullOrWhiteSpace(entry.statId))
            {
                continue;
            }

            stats[entry.statId] = new StatValue(entry.baseValue);
        }
    }

    public bool HasStat(string statId)
    {
        return !string.IsNullOrWhiteSpace(statId) && stats.ContainsKey(statId);
    }

    public float GetStatValue(string statId)
    {
        if (!stats.TryGetValue(statId, out StatValue stat))
        {
            return 0f;
        }

        return stat.CurrentValue;
    }

    public string AddModifier(string statId, StatModifier modifier)
    {
        if (string.IsNullOrWhiteSpace(statId))
        {
            return null;
        }

        if (!stats.TryGetValue(statId, out StatValue stat))
        {
            stat = new StatValue(0f);
            stats[statId] = stat;
        }

        float oldValue = stat.CurrentValue;
        stat.AddModifier(modifier);
        RaiseStatChanged(statId, oldValue, stat.CurrentValue);
        return modifier.ModifierId;
    }

    public bool RemoveModifier(string statId, string modifierId)
    {
        if (!stats.TryGetValue(statId, out StatValue stat))
        {
            return false;
        }

        float oldValue = stat.CurrentValue;
        bool removed = stat.RemoveModifier(modifierId);
        if (removed)
        {
            RaiseStatChanged(statId, oldValue, stat.CurrentValue);
        }

        return removed;
    }

    public int RemoveModifiersBySource(object source)
    {
        int totalRemoved = 0;
        List<string> keys = new List<string>(stats.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            string statId = keys[i];
            StatValue stat = stats[statId];
            float oldValue = stat.CurrentValue;
            int removed = stat.RemoveModifiersBySource(source);
            if (removed > 0)
            {
                totalRemoved += removed;
                RaiseStatChanged(statId, oldValue, stat.CurrentValue);
            }
        }

        return totalRemoved;
    }

    public void SetBaseValue(string statId, float baseValue)
    {
        if (string.IsNullOrWhiteSpace(statId))
        {
            return;
        }

        if (!stats.TryGetValue(statId, out StatValue stat))
        {
            stat = new StatValue(baseValue);
            stats[statId] = stat;
            RaiseStatChanged(statId, 0f, stat.CurrentValue);
            return;
        }

        float oldValue = stat.CurrentValue;
        stat.SetBaseValue(baseValue);
        RaiseStatChanged(statId, oldValue, stat.CurrentValue);
    }

    private void RaiseStatChanged(string statId, float oldValue, float newValue)
    {
        if (Mathf.Approximately(oldValue, newValue))
        {
            return;
        }

        OnStatChanged?.Invoke(statId, oldValue, newValue);
        EventBus.Publish(new StatChangedEvent(gameObject, statId, oldValue, newValue));
    }
}
