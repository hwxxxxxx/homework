using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePauseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button returnToMainMenuButton;

    [Header("Cursor")]
    [SerializeField] private bool hideCursorDuringGameplay = true;
    [SerializeField] private bool lockCursorDuringGameplay = true;

    [Header("Pause Rules")]
    [SerializeField] private bool pauseOnlyWhenInRun = true;
    [SerializeField] private bool showFallbackPauseOverlay = true;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string[] scenesToUnloadOnReturn = { "BaseScene_Main", "GameScene" };

    public static bool IsPaused { get; private set; }

    private bool isEndState;
    private bool isReturningToMainMenu;
    private BaseSceneUIController baseSceneUIController;
    private bool hasAppliedCursorState;
    private bool lastShouldShowCursor;

    public static GamePauseController EnsureBaseControllerExists()
    {
        GamePauseController existing = Object.FindObjectOfType<GamePauseController>();
        if (existing != null)
        {
            existing.ConfigurePauseOnlyWhenInRun(false);
            existing.ConfigureReturnScene("MainMenu");
            return existing;
        }

        GameObject pauseControllerObject = new GameObject("BasePauseController");
        GamePauseController pauseController = pauseControllerObject.AddComponent<GamePauseController>();
        pauseController.ConfigurePauseOnlyWhenInRun(false);
        pauseController.ConfigureReturnScene("MainMenu");
        return pauseController;
    }

    public void ConfigureDependencies(GameInput input, GameObject panel, Button returnButton)
    {
        gameInput = input;
        pausePanel = panel;
        returnToMainMenuButton = returnButton;
    }

    public void ConfigureReturnScene(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            mainMenuSceneName = sceneName;
        }
    }

    public void ConfigurePauseOnlyWhenInRun(bool value)
    {
        pauseOnlyWhenInRun = value;
    }

    private void OnEnable()
    {
        TryAutoWireReferences();

        if (gameStateService != null)
        {
            gameStateService.OnStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(gameStateService.CurrentState, gameStateService.CurrentState);
        }
        else
        {
            ApplyCursorState();
        }

        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.onClick.AddListener(OnReturnToMainMenuPressed);
        }
    }

    private void OnDisable()
    {
        if (returnToMainMenuButton != null)
        {
            returnToMainMenuButton.onClick.RemoveListener(OnReturnToMainMenuPressed);
        }

        if (gameStateService != null)
        {
            gameStateService.OnStateChanged -= HandleGameStateChanged;
        }

        IsPaused = false;
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        TryAutoWireReferences();
        RefreshCursorStateIfNeeded();

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

    private void OnGUI()
    {
        if (!showFallbackPauseOverlay || !IsPaused || isEndState || pausePanel != null)
        {
            return;
        }

        const float width = 360f;
        const float height = 180f;
        Rect boxRect = new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
        GUILayout.BeginArea(boxRect, GUI.skin.box);
        GUILayout.Space(12f);
        GUILayout.Label("Paused");
        GUILayout.Space(10f);

        if (GUILayout.Button("Resume", GUILayout.Height(36f)))
        {
            SetPaused(false);
        }

        GUILayout.Space(8f);
        if (GUILayout.Button("Back To Main Menu", GUILayout.Height(36f)))
        {
            OnReturnToMainMenuPressed();
        }

        GUILayout.EndArea();
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
        bool shouldShowCursor = ShouldShowCursor();
        Cursor.visible = shouldShowCursor;

        if (!lockCursorDuringGameplay)
        {
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        Cursor.lockState = shouldShowCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private bool ShouldShowCursor()
    {
        if (!hideCursorDuringGameplay)
        {
            return true;
        }

        if (IsPaused || isEndState)
        {
            return true;
        }

        return IsInteractiveUiOpen();
    }

    private bool IsInteractiveUiOpen()
    {
        if (pausePanel != null && pausePanel.activeInHierarchy)
        {
            return true;
        }

        return baseSceneUIController != null && baseSceneUIController.IsModalOpen;
    }

    private void RefreshCursorStateIfNeeded()
    {
        bool shouldShowCursor = ShouldShowCursor();
        if (hasAppliedCursorState && shouldShowCursor == lastShouldShowCursor)
        {
            return;
        }

        ApplyCursorState();
        lastShouldShowCursor = shouldShowCursor;
        hasAppliedCursorState = true;
    }

    private void HandleGameStateChanged(GameStateId previous, GameStateId current)
    {
        bool inRun = current == GameStateId.InRun;
        isEndState = pauseOnlyWhenInRun && !inRun;

        if (isEndState && IsPaused)
        {
            IsPaused = false;
        }

        if (!isEndState)
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

    public void OnReturnToMainMenuPressed()
    {
        if (isReturningToMainMenu || string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            return;
        }

        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        isReturningToMainMenu = true;
        IsPaused = false;
        Time.timeScale = 1f;

        Scene activeScene = SceneManager.GetActiveScene();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (!loadedScene.isLoaded)
            {
                continue;
            }

            if (!ShouldUnloadOnReturn(loadedScene.name) || loadedScene == activeScene)
            {
                continue;
            }

            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(loadedScene);
            if (unloadOperation != null)
            {
                while (!unloadOperation.isDone)
                {
                    yield return null;
                }
            }
        }

        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
        yield return null;
        yield return Resources.UnloadUnusedAssets();

        isReturningToMainMenu = false;
    }

    private bool ShouldUnloadOnReturn(string sceneName)
    {
        if (scenesToUnloadOnReturn == null || scenesToUnloadOnReturn.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < scenesToUnloadOnReturn.Length; i++)
        {
            if (string.Equals(scenesToUnloadOnReturn[i], sceneName))
            {
                return true;
            }
        }

        return false;
    }

    private void TryAutoWireReferences()
    {
        if (gameInput == null)
        {
            gameInput = FindObjectOfType<GameInput>();
        }

        if (pausePanel == null)
        {
            GameObject foundPausePanel = GameObject.Find("PausePanel");
            if (foundPausePanel != null)
            {
                pausePanel = foundPausePanel;
            }
        }

        if (returnToMainMenuButton == null && pausePanel != null)
        {
            returnToMainMenuButton = pausePanel.GetComponentInChildren<Button>(true);
        }

        if (baseSceneUIController == null)
        {
            baseSceneUIController = FindObjectOfType<BaseSceneUIController>();
        }
    }

}
