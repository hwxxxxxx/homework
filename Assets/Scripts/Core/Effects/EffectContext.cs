using System.Collections.Generic;
using UnityEngine;

public sealed class EffectContext
{
    public GameObject Source { get; }
    public GameObject Target { get; }
    public IModifiableStatProvider TargetStats { get; }
    public IReadOnlyList<IModifiableStatProvider> WeaponStatsProviders { get; }
    public EffectController Controller { get; }

    public EffectContext(
        GameObject source,
        GameObject target,
        IModifiableStatProvider targetStats,
        IReadOnlyList<IModifiableStatProvider> weaponStatsProviders,
        EffectController controller)
    {
        Source = source;
        Target = target;
        TargetStats = targetStats;
        WeaponStatsProviders = weaponStatsProviders;
        Controller = controller;
    }
}
