using UnityEngine;

public class GamePauseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private GameObject pausePanel;

    [Header("Cursor")]
    [SerializeField] private bool hideCursorDuringGameplay = true;
    [SerializeField] private bool lockCursorDuringGameplay = true;

    public static bool IsPaused { get; private set; }

    private bool isEndState;

    private void OnEnable()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.OnStateChanged += HandleFlowStateChanged;
            HandleFlowStateChanged(gameFlowManager.CurrentState);
        }
        else
        {
            ApplyCursorState();
        }
    }

    private void OnDisable()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.OnStateChanged -= HandleFlowStateChanged;
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

    private void HandleFlowStateChanged(GameFlowManager.GameFlowState state)
    {
        isEndState = state == GameFlowManager.GameFlowState.Victory || state == GameFlowManager.GameFlowState.Fail;
        if (isEndState && IsPaused)
        {
            IsPaused = false;
        }

        if (isEndState)
        {
            Time.timeScale = 0f;
        }
        else if (!IsPaused)
        {
            Time.timeScale = 1f;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(IsPaused && !isEndState);
        }

        ApplyCursorState();
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
}
