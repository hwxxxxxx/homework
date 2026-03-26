public class EnemyAttackState : IEnemyState
{
    private readonly EnemyAIController controller;

    public EnemyAttackState(EnemyAIController controller)
    {
        this.controller = controller;
    }

    public EnemyAIController.EnemyStateId StateId => EnemyAIController.EnemyStateId.Attack;

    public void Enter()
    {
        controller.SetCurrentStateId(StateId);
        controller.StopMoving();
        controller.SetAttackEnabled(true);
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

        if (!controller.IsTargetInAttackRange())
        {
            if (controller.IsTargetInDetectionRange())
            {
                controller.ChangeState(EnemyAIController.EnemyStateId.Chase);
            }
            else
            {
                controller.ChangeState(EnemyAIController.EnemyStateId.Idle);
            }

            return;
        }

        controller.FaceTarget();
    }

    public void Exit()
    {
        controller.SetAttackEnabled(false);
    }
}
