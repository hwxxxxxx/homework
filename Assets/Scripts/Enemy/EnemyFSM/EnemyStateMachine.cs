using System.Collections.Generic;

public class EnemyStateMachine
{
    private readonly Dictionary<EnemyAIController.EnemyStateId, IEnemyState> states =
        new Dictionary<EnemyAIController.EnemyStateId, IEnemyState>();

    private IEnemyState currentState;

    public void RegisterState(IEnemyState state)
    {
        states[state.StateId] = state;
    }

    public void ChangeState(EnemyAIController.EnemyStateId nextStateId)
    {
        if (currentState != null && currentState.StateId == nextStateId)
        {
            return;
        }

        if (!states.TryGetValue(nextStateId, out IEnemyState nextState))
        {
            return;
        }

        currentState?.Exit();
        currentState = nextState;
        currentState.Enter();
    }

    public void Tick()
    {
        currentState?.Tick();
    }
}
