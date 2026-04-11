using System.Collections.Generic;

public class StatModifierEffectRuntime : EffectRuntimeBase
{
    private readonly StatModifierDescriptor[] modifierEntries;
    private readonly List<(IModifiableStatProvider provider, string statId, string modifierId)> appliedModifiers =
        new List<(IModifiableStatProvider provider, string statId, string modifierId)>();

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
        if (modifierEntries == null)
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

            IReadOnlyList<IModifiableStatProvider> providers = ResolveProviders(entry.statId);
            if (providers == null || providers.Count == 0)
            {
                continue;
            }

            for (int p = 0; p < providers.Count; p++)
            {
                IModifiableStatProvider provider = providers[p];
                if (provider == null)
                {
                    continue;
                }

                string modifierId = provider.AddModifier(entry.statId, modifier);
                if (!string.IsNullOrWhiteSpace(modifierId))
                {
                    appliedModifiers.Add((provider, entry.statId, modifierId));
                }
            }
        }
    }

    public override void OnRemove()
    {
        for (int i = 0; i < appliedModifiers.Count; i++)
        {
            (IModifiableStatProvider provider, string statId, string modifierId) modifier = appliedModifiers[i];
            modifier.provider?.RemoveModifier(modifier.statId, modifier.modifierId);
        }

        appliedModifiers.Clear();
    }

    private IReadOnlyList<IModifiableStatProvider> ResolveProviders(string statId)
    {
        if (string.IsNullOrWhiteSpace(statId))
        {
            return null;
        }

        if (IsWeaponStat(statId) &&
            Context.WeaponStatsProviders != null &&
            Context.WeaponStatsProviders.Count > 0)
        {
            return Context.WeaponStatsProviders;
        }

        if (Context.TargetStats == null)
        {
            return null;
        }

        return new IModifiableStatProvider[] { Context.TargetStats };
    }

    private static bool IsWeaponStat(string statId)
    {
        return statId == StatIds.WeaponDamage ||
               statId == StatIds.WeaponFireRate ||
               statId == StatIds.WeaponReloadTime;
    }
}
