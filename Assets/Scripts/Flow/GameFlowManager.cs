using System;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private RunContextService runContextService;
    
    private bool runStarted;

    public event Action OnCombatStarted;

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnDied += HandlePlayerDied;
        }

        if (gameStateService != null)
        {
            gameStateService.OnStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(gameStateService.CurrentState, gameStateService.CurrentState);
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnDied -= HandlePlayerDied;
        }

        if (gameStateService != null)
        {
            gameStateService.OnStateChanged -= HandleGameStateChanged;
        }

        runStarted = false;
    }

    public void NotifyAllWavesCleared()
    {
        if (!runStarted || gameStateService == null || gameStateService.CurrentState != GameStateId.InRun)
        {
            return;
        }

        if (runContextService != null && runContextService.IsRunActive)
        {
            runContextService.CompleteRun(true);
        }

        if (gameStateService != null)
        {
            gameStateService.TrySetState(GameStateId.RunResult);
        }
    }

    public void TriggerFail()
    {
        if (!runStarted || gameStateService == null || gameStateService.CurrentState != GameStateId.InRun)
        {
            return;
        }

        if (runContextService != null && runContextService.IsRunActive)
        {
            runContextService.CompleteRun(false);
        }

        if (gameStateService != null)
        {
            gameStateService.TrySetState(GameStateId.RunResult);
        }
    }

    private void HandlePlayerDied()
    {
        TriggerFail();
    }

    private void HandleGameStateChanged(GameStateId previous, GameStateId current)
    {
        if (current == GameStateId.InRun)
        {
            if (!runStarted)
            {
                runStarted = true;
                OnCombatStarted?.Invoke();
            }
            return;
        }

        runStarted = false;
    }
}
