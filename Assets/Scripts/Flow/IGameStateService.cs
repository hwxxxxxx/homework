using System;

public interface IGameStateService
{
    GameStateId CurrentState { get; }
    event Action<GameStateId, GameStateId> OnStateChanged;
    bool TrySetState(GameStateId nextState);
}
