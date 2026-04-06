using UnityEngine;

public readonly struct StatChangedEvent : IGameEvent
{
    public readonly GameObject Target;
    public readonly string StatId;
    public readonly float OldValue;
    public readonly float NewValue;

    public StatChangedEvent(GameObject target, string statId, float oldValue, float newValue)
    {
        Target = target;
        StatId = statId;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

public readonly struct EffectAppliedEvent : IGameEvent
{
    public readonly GameObject Target;
    public readonly string EffectId;

    public EffectAppliedEvent(GameObject target, string effectId)
    {
        Target = target;
        EffectId = effectId;
    }
}

public readonly struct EffectRemovedEvent : IGameEvent
{
    public readonly GameObject Target;
    public readonly string EffectId;

    public EffectRemovedEvent(GameObject target, string effectId)
    {
        Target = target;
        EffectId = effectId;
    }
}

public readonly struct DamageRequestedEvent : IGameEvent
{
    public readonly GameObject Source;
    public readonly GameObject Target;
    public readonly float Amount;
    public readonly bool IsPeriodic;
    public readonly string EffectId;

    public DamageRequestedEvent(GameObject source, GameObject target, float amount, bool isPeriodic, string effectId)
    {
        Source = source;
        Target = target;
        Amount = amount;
        IsPeriodic = isPeriodic;
        EffectId = effectId;
    }
}

public readonly struct BossDefeatedEvent : IGameEvent
{
    public readonly GameObject BossObject;

    public BossDefeatedEvent(GameObject bossObject)
    {
        BossObject = bossObject;
    }
}

public readonly struct LevelUnlockedEvent : IGameEvent
{
    public readonly string LevelId;

    public LevelUnlockedEvent(string levelId)
    {
        LevelId = levelId;
    }
}

public readonly struct PlayerDiedEvent : IGameEvent
{
}
