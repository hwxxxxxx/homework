using System;
using UnityEngine;

public class GameStateMachineService : MonoBehaviour
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
                return to == GameStateId.MainMenu || to == GameStateId.Base;
            case GameStateId.MainMenu:
                return to == GameStateId.Base;
            case GameStateId.Base:
                return to == GameStateId.LoadingRun || to == GameStateId.MainMenu;
            case GameStateId.LoadingRun:
                return to == GameStateId.InRun || to == GameStateId.MainMenu;
            case GameStateId.InRun:
                return to == GameStateId.RunResult || to == GameStateId.LoadingBase || to == GameStateId.MainMenu;
            case GameStateId.RunResult:
                return to == GameStateId.LoadingBase || to == GameStateId.MainMenu;
            case GameStateId.LoadingBase:
                return to == GameStateId.Base || to == GameStateId.MainMenu;
            default:
                return false;
        }
    }
}
