using UnityEngine;

[CreateAssetMenu(menuName = "Game/Effects/Periodic Damage Effect", fileName = "PeriodicDamageEffect")]
public class PeriodicDamageEffectAsset : EffectAsset
{
    [SerializeField] private float damagePerTick = 5f;
    [SerializeField] private float tickInterval = 1f;

    public override IEffectRuntime CreateRuntime(EffectContext context)
    {
        return new PeriodicDamageEffectRuntime(
            EffectId,
            context,
            Duration,
            Mathf.Max(0f, damagePerTick),
            Mathf.Max(0.01f, tickInterval)
        );
    }
}
