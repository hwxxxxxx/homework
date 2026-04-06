using System;
using UnityEngine;

public class GameStateMachineService : MonoBehaviour, IGameStateService
{
    [SerializeField] private GameStateId initialState = GameStateId.Boot;

    private GameStateId currentState;
    private bool isInitialized;

    public GameStateId CurrentState => isInitialized ? currentState : initialState;
    public event Action<GameStateId, GameStateId> OnStateChanged;

    private void Awake()
    {
        currentState = initialState;
        isInitialized = true;
    }

    public bool TrySetState(GameStateId nextState)
    {
        GameStateId fromState = CurrentState;
        if (!isInitialized)
        {
            currentState = fromState;
            isInitialized = true;
        }

        if (nextState == currentState)
        {
            return false;
        }

        if (!CanTransition(currentState, nextState))
        {
            return false;
        }

        GameStateId previous = currentState;
        currentState = nextState;
        OnStateChanged?.Invoke(previous, currentState);
        return true;
    }

    private static bool CanTransition(GameStateId from, GameStateId to)
    {
        switch (from)
        {
            case GameStateId.Boot:
                return to == GameStateId.Base;
            case GameStateId.Base:
                return to == GameStateId.RunSelect;
            case GameStateId.RunSelect:
                return to == GameStateId.LoadingRun || to == GameStateId.Base;
            case GameStateId.LoadingRun:
                return to == GameStateId.InRun;
            case GameStateId.InRun:
                return to == GameStateId.RunResult;
            case GameStateId.RunResult:
                return to == GameStateId.LoadingBase;
            case GameStateId.LoadingBase:
                return to == GameStateId.Base;
            default:
                return false;
        }
    }
}
