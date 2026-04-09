public interface IRuntimePhaseParticipant
{
    int LifecycleOrder { get; }
    void EnterRuntimePhase(RuntimePhase phase);
}
