public interface IEnemyState
{
    EnemyAIController.EnemyStateId StateId { get; }

    void Enter();
    void Tick();
    void Exit();
}
