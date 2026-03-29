using System.Collections.Generic;
using UnityEngine;

public class StatValue
{
    private readonly List<StatModifier> modifiers = new List<StatModifier>();

    public float BaseValue { get; private set; }
    public float CurrentValue { get; private set; }

    public StatValue(float baseValue)
    {
        BaseValue = baseValue;
        Recalculate();
    }

    public void SetBaseValue(float baseValue)
    {
        BaseValue = baseValue;
        Recalculate();
    }

    public void AddModifier(StatModifier modifier)
    {
        modifiers.Add(modifier);
        modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
        Recalculate();
    }

    public bool RemoveModifier(string modifierId)
    {
        int removed = modifiers.RemoveAll(m => m.ModifierId == modifierId);
        if (removed > 0)
        {
            Recalculate();
            return true;
        }

        return false;
    }

    public int RemoveModifiersBySource(object source)
    {
        if (source == null)
        {
            return 0;
        }

        int removed = modifiers.RemoveAll(m => ReferenceEquals(m.Source, source));
        if (removed > 0)
        {
            Recalculate();
        }

        return removed;
    }

    private void Recalculate()
    {
        float result = BaseValue;
        float percentAdd = 0f;
        float percentMultiply = 1f;

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier modifier = modifiers[i];
            switch (modifier.Operation)
            {
                case StatModifierOperation.Flat:
                    result += modifier.Value;
                    break;
                case StatModifierOperation.PercentAdd:
                    percentAdd += modifier.Value;
                    break;
                case StatModifierOperation.PercentMultiply:
                    percentMultiply *= 1f + modifier.Value;
                    break;
            }
        }

        result *= 1f + percentAdd;
        result *= percentMultiply;
        CurrentValue = Mathf.Max(0f, result);
    }
}
