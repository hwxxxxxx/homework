using System;
using UnityEngine;

public class RunOutcomeController : MonoBehaviour
{
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private RunContextService runContextService;
    [SerializeField] private GamePauseController pauseController;

    private IDisposable bossDefeatedSubscription;
    private IDisposable playerDiedSubscription;

    private void Awake()
    {
        if (gameStateService == null || runContextService == null || pauseController == null)
        {
            throw new InvalidOperationException("RunOutcomeController references are not fully assigned.");
        }
    }

    private void OnEnable()
    {
        bossDefeatedSubscription = EventBus.Subscribe<BossDefeatedEvent>(HandleBossDefeated);
        playerDiedSubscription = EventBus.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
    }

    private void OnDisable()
    {
        bossDefeatedSubscription?.Dispose();
        playerDiedSubscription?.Dispose();
        bossDefeatedSubscription = null;
        playerDiedSubscription = null;
    }

    private void HandleBossDefeated(BossDefeatedEvent _)
    {
        CompleteRun(true);
    }

    private void HandlePlayerDied(PlayerDiedEvent _)
    {
        CompleteRun(false);
    }

    private void CompleteRun(bool won)
    {
        if (gameStateService.CurrentState != GameStateId.InRun)
        {
            return;
        }

        if (won)
        {
            RuntimeShell runtimeShell = RuntimeShell.Instance;
            runtimeShell.ProgressService.MarkRunCompleted(runContextService.CurrentRunId, runtimeShell.RunCatalog);
        }

        runContextService.EndRun(won);
        pauseController.SetPaused(false);
        if (!gameStateService.TrySetState(GameStateId.RunResult))
        {
            throw new InvalidOperationException("Failed to transition to RunResult after run completion.");
        }
    }
}
