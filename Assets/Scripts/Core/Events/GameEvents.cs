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

public readonly struct WeaponFiredEvent : IGameEvent
{
    public readonly Vector3 Position;

    public WeaponFiredEvent(Vector3 position)
    {
        Position = position;
    }
}

public readonly struct WeaponReloadStartedEvent : IGameEvent
{
    public readonly GameObject SourceObject;
    public readonly Vector3 Position;

    public WeaponReloadStartedEvent(GameObject sourceObject, Vector3 position)
    {
        SourceObject = sourceObject;
        Position = position;
    }
}

public readonly struct PlayerRunStateChangedEvent : IGameEvent
{
    public readonly GameObject SourceObject;
    public readonly Vector3 Position;
    public readonly bool IsRunning;

    public PlayerRunStateChangedEvent(GameObject sourceObject, Vector3 position, bool isRunning)
    {
        SourceObject = sourceObject;
        Position = position;
        IsRunning = isRunning;
    }
}

public readonly struct PlayerHitEnemyEvent : IGameEvent
{
    public readonly GameObject EnemyObject;
    public readonly bool IsBoss;
    public readonly Vector3 Position;

    public PlayerHitEnemyEvent(GameObject enemyObject, bool isBoss, Vector3 position)
    {
        EnemyObject = enemyObject;
        IsBoss = isBoss;
        Position = position;
    }
}

public readonly struct EnemyAttackEvent : IGameEvent
{
    public readonly GameObject EnemyObject;
    public readonly bool IsBoss;
    public readonly Vector3 Position;

    public EnemyAttackEvent(GameObject enemyObject, bool isBoss, Vector3 position)
    {
        EnemyObject = enemyObject;
        IsBoss = isBoss;
        Position = position;
    }
}

public readonly struct EnemyDiedEvent : IGameEvent
{
    public readonly GameObject EnemyObject;
    public readonly bool IsBoss;
    public readonly Vector3 Position;

    public EnemyDiedEvent(GameObject enemyObject, bool isBoss, Vector3 position)
    {
        EnemyObject = enemyObject;
        IsBoss = isBoss;
        Position = position;
    }
}
