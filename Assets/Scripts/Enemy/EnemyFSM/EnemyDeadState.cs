public class EnemyDeadState : IEnemyState
{
    private readonly EnemyAIController controller;

    public EnemyDeadState(EnemyAIController controller)
    {
        this.controller = controller;
    }

    public EnemyAIController.EnemyStateId StateId => EnemyAIController.EnemyStateId.Dead;

    public void Enter()
    {
        controller.SetCurrentStateId(StateId);
        controller.DisableOnDeath();
    }

    public void Tick()
    {
    }

    public void Exit()
    {
    }
}
