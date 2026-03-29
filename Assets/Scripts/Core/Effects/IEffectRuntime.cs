public interface IEffectRuntime
{
    string EffectId { get; }
    bool IsExpired { get; }
    void OnApply();
    void OnTick(float deltaTime);
    void OnRemove();
}
