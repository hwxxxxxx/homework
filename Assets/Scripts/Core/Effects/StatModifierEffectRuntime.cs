using System.Collections.Generic;

public class StatModifierEffectRuntime : EffectRuntimeBase
{
    private readonly StatModifierDescriptor[] modifierEntries;
    private readonly List<(string statId, string modifierId)> appliedModifiers =
        new List<(string statId, string modifierId)>();

    public StatModifierEffectRuntime(
        string effectId,
        EffectContext context,
        float duration,
        StatModifierDescriptor[] modifierEntries
    ) : base(effectId, context, duration)
    {
        this.modifierEntries = modifierEntries;
    }

    public override void OnApply()
    {
        if (Context.TargetStats == null || modifierEntries == null)
        {
            return;
        }

        for (int i = 0; i < modifierEntries.Length; i++)
        {
            StatModifierDescriptor entry = modifierEntries[i];
            if (string.IsNullOrWhiteSpace(entry.statId))
            {
                continue;
            }

            StatModifier modifier = new StatModifier(
                modifierId: null,
                value: entry.value,
                operation: entry.operation,
                source: this,
                order: entry.order
            );

            string modifierId = Context.TargetStats.AddModifier(entry.statId, modifier);
            if (!string.IsNullOrWhiteSpace(modifierId))
            {
                appliedModifiers.Add((entry.statId, modifierId));
            }
        }
    }

    public override void OnRemove()
    {
        if (Context.TargetStats == null)
        {
            return;
        }

        for (int i = 0; i < appliedModifiers.Count; i++)
        {
            (string statId, string modifierId) modifier = appliedModifiers[i];
            Context.TargetStats.RemoveModifier(modifier.statId, modifier.modifierId);
        }

        appliedModifiers.Clear();
    }
}
