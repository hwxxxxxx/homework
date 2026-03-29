public class PeriodicDamageEffectRuntime : EffectRuntimeBase
{
    private readonly float damagePerTick;
    private readonly float tickInterval;
    private float tickTimer;

    public PeriodicDamageEffectRuntime(
        string effectId,
        EffectContext context,
        float duration,
        float damagePerTick,
        float tickInterval
    ) : base(effectId, context, duration)
    {
        this.damagePerTick = damagePerTick;
        this.tickInterval = tickInterval;
    }

    protected override void Tick(float deltaTime)
    {
        tickTimer += deltaTime;
        if (tickTimer < tickInterval)
        {
            return;
        }

        tickTimer -= tickInterval;
        EventBus.Publish(
            new DamageRequestedEvent(
                Context.Source,
                Context.Target,
                damagePerTick,
                true,
                EffectId
            )
        );
    }
}
