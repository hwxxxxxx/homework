using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class PauseMenuController : MonoBehaviour
{
    private const string CursorOwner = "PauseMenuUI";
    private const float PauseUiSortingOrder = 1000f;

    [SerializeField] private GameInput gameInput;
    [SerializeField] private GamePauseController pauseController;
    [SerializeField] private GameFlowOrchestrator gameFlowOrchestrator;
    [SerializeField] private GameStateMachineService gameStateService;

    private UIDocument pauseDocument;
    private VisualElement pauseRoot;
    private Button returnBaseButton;
    private Button openSettingsButton;
    private Button returnMainMenuButton;
    private GameSettingsUiPresenter settingsPresenter;

    private void Awake()
    {
        if (gameInput == null || pauseController == null || gameFlowOrchestrator == null || gameStateService == null)
        {
            throw new InvalidOperationException("PauseMenuController references are not fully assigned.");
        }

        pauseDocument = GetComponent<UIDocument>();
        pauseDocument.sortingOrder = PauseUiSortingOrder;
        VisualElement root = pauseDocument.rootVisualElement;
        pauseRoot = root.Q<VisualElement>("pause-root");
        returnBaseButton = root.Q<Button>("return-base-btn");
        openSettingsButton = root.Q<Button>("pause-settings-btn");
        returnMainMenuButton = root.Q<Button>("return-mainmenu-btn");
        settingsPresenter = new GameSettingsUiPresenter(root);

        if (pauseRoot == null || returnBaseButton == null || openSettingsButton == null || returnMainMenuButton == null)
        {
            throw new InvalidOperationException("PauseMenuController UI binding failed.");
        }
    }

    private void OnEnable()
    {
        returnBaseButton.clicked += HandleReturnBaseClicked;
        openSettingsButton.clicked += HandleOpenSettingsClicked;
        returnMainMenuButton.clicked += HandleReturnMainMenuClicked;
        SetPauseVisible(false);
        pauseController.SetPaused(false);
        settingsPresenter.Hide(saveAudioSettings: false);
    }

    private void OnDisable()
    {
        CursorPolicyService.ReleaseUiCursor(CursorOwner);
        returnBaseButton.clicked -= HandleReturnBaseClicked;
        openSettingsButton.clicked -= HandleOpenSettingsClicked;
        returnMainMenuButton.clicked -= HandleReturnMainMenuClicked;
        settingsPresenter.Hide(saveAudioSettings: true);
    }

    private void OnDestroy()
    {
        settingsPresenter?.Dispose();
        settingsPresenter = null;
    }

    private void Update()
    {
        if (!IsPauseMenuState(gameStateService.CurrentState))
        {
            if (GamePauseController.IsPaused)
            {
                pauseController.SetPaused(false);
            }

            SetPauseVisible(false);
            return;
        }

        if (!gameInput.IsPausePressed())
        {
            return;
        }

        if (GamePauseController.IsPaused && settingsPresenter.IsVisible)
        {
            settingsPresenter.Hide(saveAudioSettings: true);
            return;
        }

        bool nextPaused = !GamePauseController.IsPaused;
        pauseController.SetPaused(nextPaused);
        ConfigureButtonsByState(gameStateService.CurrentState);
        SetPauseVisible(nextPaused);

        if (!nextPaused)
        {
            settingsPresenter.Hide(saveAudioSettings: true);
        }
    }

    private void HandleReturnBaseClicked()
    {
        pauseController.SetPaused(false);
        settingsPresenter.Hide(saveAudioSettings: true);
        SetPauseVisible(false);
        gameFlowOrchestrator.ReturnToBase();
    }

    private void HandleOpenSettingsClicked()
    {
        settingsPresenter.Open();
    }

    private void HandleReturnMainMenuClicked()
    {
        pauseController.SetPaused(false);
        settingsPresenter.Hide(saveAudioSettings: true);
        SetPauseVisible(false);
        if (gameStateService.CurrentState == GameStateId.Base)
        {
            gameFlowOrchestrator.EnterMainMenu();
            return;
        }

        gameFlowOrchestrator.ReturnToMainMenu();
    }

    private void SetPauseVisible(bool visible)
    {
        pauseRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        if (visible)
        {
            CursorPolicyService.AcquireUiCursor(CursorOwner);
            return;
        }

        CursorPolicyService.ReleaseUiCursor(CursorOwner);
    }

    private void ConfigureButtonsByState(GameStateId state)
    {
        if (state == GameStateId.Base)
        {
            returnBaseButton.style.display = DisplayStyle.None;
            returnMainMenuButton.style.display = DisplayStyle.Flex;
            returnMainMenuButton.SetEnabled(true);
            return;
        }

        returnBaseButton.style.display = DisplayStyle.Flex;
        returnBaseButton.SetEnabled(true);
        returnMainMenuButton.style.display = DisplayStyle.Flex;
        returnMainMenuButton.SetEnabled(true);
    }

    private static bool IsPauseMenuState(GameStateId state)
    {
        if (state == GameStateId.InRun || state == GameStateId.Base)
        {
            return true;
        }

        return IsGameplaySceneLoaded();
    }

    private static bool IsGameplaySceneLoaded()
    {
        int sceneCount = SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                continue;
            }

            string sceneName = scene.name;
            if (sceneName == "Persistent" || sceneName == "Boot" || sceneName == "MainMenu")
            {
                continue;
            }

            return true;
        }

        return false;
    }
}
