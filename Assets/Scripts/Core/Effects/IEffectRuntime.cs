public interface IEffectRuntime
{
    string EffectId { get; }
    bool IsExpired { get; }
    float Duration { get; }
    float RemainingTime { get; }
    void OnApply();
    void OnTick(float deltaTime);
    void OnRemove();
}
