using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private RunContextService runContextService;
    [SerializeField] private string baseSceneName = "BaseScene_Main";
    
    private bool runStarted;

    private void Awake()
    {
        if (gameStateService == null)
        {
            gameStateService = FindObjectOfType<GameStateMachineService>(true);
        }

        if (runContextService == null)
        {
            runContextService = FindObjectOfType<RunContextService>(true);
        }
    }

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

    public bool TryReturnToBaseFromResult()
    {
        if (gameStateService == null || gameStateService.CurrentState != GameStateId.RunResult)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(baseSceneName))
        {
            return false;
        }

        if (!gameStateService.TrySetState(GameStateId.LoadingBase))
        {
            return false;
        }

        Time.timeScale = 1f;
        return LoadingScreenService.TryLoadSceneSingle(
            baseSceneName,
            "Returning to base...",
            keepVisibleAfterSceneLoad: false);
    }

    private void HandlePlayerDied()
    {
        EventBus.Publish(new PlayerDiedEvent());
        TriggerFail();
    }

    private void HandleGameStateChanged(GameStateId previous, GameStateId current)
    {
        runStarted = current == GameStateId.InRun;
    }
}
