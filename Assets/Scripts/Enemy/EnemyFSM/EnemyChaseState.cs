public class EnemyChaseState : IEnemyState
{
    private readonly EnemyAIController controller;

    public EnemyChaseState(EnemyAIController controller)
    {
        this.controller = controller;
    }

    public EnemyAIController.EnemyStateId StateId => EnemyAIController.EnemyStateId.Chase;

    public void Enter()
    {
        controller.SetCurrentStateId(StateId);
        controller.SetAttackEnabled(false);
    }

    public void Tick()
    {
        if (controller.IsDead)
        {
            controller.ChangeState(EnemyAIController.EnemyStateId.Dead);
            return;
        }

        if (!controller.EnsureTarget())
        {
            controller.ChangeState(EnemyAIController.EnemyStateId.Idle);
            return;
        }

        if (controller.IsTargetInAttackRange())
        {
            controller.ChangeState(EnemyAIController.EnemyStateId.Attack);
            return;
        }

        if (!controller.IsTargetInDetectionRange())
        {
            controller.ChangeState(EnemyAIController.EnemyStateId.Idle);
            return;
        }

        controller.MoveToTarget();
    }

    public void Exit()
    {
    }
}
