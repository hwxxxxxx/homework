using UnityEngine;

public abstract class EffectRuntimeBase : IEffectRuntime
{
    private readonly float duration;
    private float elapsed;

    protected EffectContext Context { get; }
    public string EffectId { get; }
    public bool IsExpired { get; private set; }
    public float Duration => duration;
    public float RemainingTime => duration <= 0f ? 0f : Mathf.Max(0f, duration - elapsed);

    protected EffectRuntimeBase(string effectId, EffectContext context, float duration)
    {
        EffectId = effectId;
        Context = context;
        this.duration = duration;
    }

    public virtual void OnApply()
    {
    }

    public virtual void OnTick(float deltaTime)
    {
        if (IsExpired)
        {
            return;
        }

        Tick(deltaTime);

        if (duration <= 0f)
        {
            return;
        }

        elapsed += deltaTime;
        if (elapsed >= duration)
        {
            IsExpired = true;
        }
    }

    public virtual void OnRemove()
    {
    }

    protected virtual void Tick(float deltaTime)
    {
    }

    protected void Expire()
    {
        IsExpired = true;
    }
}
