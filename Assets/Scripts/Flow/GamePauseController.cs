using UnityEngine;

public class GamePauseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private GameObject pausePanel;

    [Header("Cursor")]
    [SerializeField] private bool hideCursorDuringGameplay = true;
    [SerializeField] private bool lockCursorDuringGameplay = true;

    public static bool IsPaused { get; private set; }

    private bool isEndState;

    private void OnEnable()
    {
        if (gameStateService != null)
        {
            gameStateService.OnStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(gameStateService.CurrentState, gameStateService.CurrentState);
        }
        else
        {
            ApplyCursorState();
        }
    }

    private void OnDisable()
    {
        if (gameStateService != null)
        {
            gameStateService.OnStateChanged -= HandleGameStateChanged;
        }

        if (IsPaused)
        {
            SetPaused(false);
        }
    }

    private void Update()
    {
        if (gameInput == null)
        {
            return;
        }

        if (!gameInput.IsPausePressed())
        {
            return;
        }

        if (isEndState)
        {
            return;
        }

        SetPaused(!IsPaused);
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(IsPaused && !isEndState);
        }

        if (!isEndState)
        {
            Time.timeScale = IsPaused ? 0f : 1f;
        }

        ApplyCursorState();
    }

    private void ApplyCursorState()
    {
        bool shouldShowCursor = IsPaused || isEndState || !hideCursorDuringGameplay;
        Cursor.visible = shouldShowCursor;

        if (!lockCursorDuringGameplay)
        {
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        Cursor.lockState = shouldShowCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void HandleGameStateChanged(GameStateId previous, GameStateId current)
    {
        bool inRun = current == GameStateId.InRun;
        isEndState = !inRun;

        if (isEndState && IsPaused)
        {
            IsPaused = false;
        }

        if (inRun)
        {
            if (!IsPaused)
            {
                Time.timeScale = 1f;
            }
        }
        else
        {
            Time.timeScale = 0f;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(IsPaused && !isEndState);
        }

        ApplyCursorState();
    }
}
