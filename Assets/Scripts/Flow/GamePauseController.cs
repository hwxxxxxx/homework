using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GamePauseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private GameStateMachineService gameStateService;

    [Header("Cursor")]
    [SerializeField] private bool hideCursorDuringGameplay = true;
    [SerializeField] private bool lockCursorDuringGameplay = true;

    [Header("Pause Rules")]
    [SerializeField] private bool pauseOnlyWhenInRun = true;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string baseSceneName = "BaseScene_Main";
    [SerializeField] private string[] scenesToUnloadOnReturn = { "BaseScene_Main", "GameScene" };

    public static bool IsPaused { get; private set; }

    private bool isEndState;
    private bool isReturningToMainMenu;
    private BaseSceneUIController baseSceneUIController;
    private bool hasAppliedCursorState;
    private bool lastShouldShowCursor;

    private UIDocument pauseDocument;
    private VisualElement pauseRoot;
    private Button returnToBaseButton;
    private Button returnToMainMenuButton;

    private void Awake()
    {
        pauseDocument = GetComponent<UIDocument>();
        VisualElement root = pauseDocument.rootVisualElement;
        pauseRoot = root.Q<VisualElement>("pause-root");
        returnToBaseButton = root.Q<Button>("return-base-btn");
        returnToMainMenuButton = root.Q<Button>("return-mainmenu-btn");
    }

    public void ConfigureMainMenuScene(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            mainMenuSceneName = sceneName;
        }
    }

    public void ConfigureBaseScene(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            baseSceneName = sceneName;
        }
    }

    public void ConfigurePauseOnlyWhenInRun(bool value)
    {
        pauseOnlyWhenInRun = value;
    }

    public void ConfigureBaseSceneUiController(BaseSceneUIController controller)
    {
        baseSceneUIController = controller;
    }

    private void OnEnable()
    {
        gameStateService.OnStateChanged += HandleGameStateChanged;
        returnToBaseButton.clicked += OnReturnToBasePressed;
        returnToMainMenuButton.clicked += OnReturnToMainMenuPressed;
        HandleGameStateChanged(gameStateService.CurrentState, gameStateService.CurrentState);
    }

    private void OnDisable()
    {
        gameStateService.OnStateChanged -= HandleGameStateChanged;
        returnToBaseButton.clicked -= OnReturnToBasePressed;
        returnToMainMenuButton.clicked -= OnReturnToMainMenuPressed;

        IsPaused = false;
        Time.timeScale = 1f;
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        RefreshCursorStateIfNeeded();

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
        SetPauseVisible(IsPaused && !isEndState);

        if (!isEndState)
        {
            Time.timeScale = IsPaused ? 0f : 1f;
        }

        ApplyCursorState();
    }

    private void ApplyCursorState()
    {
        bool shouldShowCursor = ShouldShowCursor();
        UnityEngine.Cursor.visible = shouldShowCursor;

        if (!lockCursorDuringGameplay)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            return;
        }

        UnityEngine.Cursor.lockState = shouldShowCursor ? CursorLockMode.None : CursorLockMode.Locked;
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
        if (pauseRoot != null && pauseRoot.style.display.value != DisplayStyle.None)
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

        SetPauseVisible(IsPaused && !isEndState);
        ApplyCursorState();
    }

    public void OnReturnToMainMenuPressed()
    {
        if (isReturningToMainMenu || string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            return;
        }

        StartCoroutine(ReturnToSceneRoutine(mainMenuSceneName, "Returning to main menu..."));
    }

    public void OnReturnToBasePressed()
    {
        if (isReturningToMainMenu || string.IsNullOrWhiteSpace(baseSceneName))
        {
            return;
        }

        StartCoroutine(ReturnToSceneRoutine(baseSceneName, "Returning to base..."));
    }

    public void ForceClearPauseUi()
    {
        isReturningToMainMenu = false;
        SetPaused(false);
        SetPauseVisible(false);
        Time.timeScale = 1f;
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }

    private IEnumerator ReturnToSceneRoutine(string targetSceneName, string loadingMessage)
    {
        isReturningToMainMenu = true;
        IsPaused = false;
        Time.timeScale = 1f;
        SetPauseVisible(false);

        if (string.Equals(targetSceneName, mainMenuSceneName))
        {
            EventSystem[] systems = FindObjectsOfType<EventSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                EventSystem system = systems[i];
                if (system != null && system.name == "GlobalEventSystem")
                {
                    system.gameObject.SetActive(false);
                }
            }
        }

        if (string.Equals(targetSceneName, baseSceneName) && gameStateService != null)
        {
            if (gameStateService.CurrentState == GameStateId.InRun)
            {
                gameStateService.TrySetState(GameStateId.RunResult);
            }

            if (gameStateService.CurrentState == GameStateId.RunResult)
            {
                gameStateService.TrySetState(GameStateId.LoadingBase);
            }
        }

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

        LoadingScreenService.TryLoadSceneSingle(
            targetSceneName,
            loadingMessage,
            keepVisibleAfterSceneLoad: false);

        while (LoadingScreenService.IsTransitionInProgress)
        {
            yield return null;
        }

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

    private void SetPauseVisible(bool visible)
    {
        pauseRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
