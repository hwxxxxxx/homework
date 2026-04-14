using System.Collections.Generic;
using UnityEngine;

public sealed class EffectContext
{
    public GameObject Source { get; }
    public GameObject Target { get; }
    public IModifiableStatProvider TargetStats { get; }
    public IReadOnlyList<WeaponBase> Weapons { get; }
    public IReadOnlyList<IModifiableStatProvider> WeaponStatsProviders { get; }
    public EffectController Controller { get; }

    public EffectContext(
        GameObject source,
        GameObject target,
        IModifiableStatProvider targetStats,
        IReadOnlyList<WeaponBase> weapons,
        IReadOnlyList<IModifiableStatProvider> weaponStatsProviders,
        EffectController controller)
    {
        Source = source;
        Target = target;
        TargetStats = targetStats;
        Weapons = weapons;
        WeaponStatsProviders = weaponStatsProviders;
        Controller = controller;
    }
}
