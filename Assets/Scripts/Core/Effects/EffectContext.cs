using UnityEngine;

public sealed class EffectContext
{
    public GameObject Source { get; }
    public GameObject Target { get; }
    public IModifiableStatProvider TargetStats { get; }
    public EffectController Controller { get; }

    public EffectContext(GameObject source, GameObject target, IModifiableStatProvider targetStats, EffectController controller)
    {
        Source = source;
        Target = target;
        TargetStats = targetStats;
        Controller = controller;
    }
}
