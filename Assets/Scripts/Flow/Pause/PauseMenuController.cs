using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PauseMenuController : MonoBehaviour
{
    private const string CursorOwner = "PauseMenuUI";

    [SerializeField] private GameInput gameInput;
    [SerializeField] private GamePauseController pauseController;
    [SerializeField] private GameFlowOrchestrator gameFlowOrchestrator;
    [SerializeField] private GameStateMachineService gameStateService;

    private UIDocument pauseDocument;
    private VisualElement pauseRoot;
    private Button returnBaseButton;
    private Button returnMainMenuButton;

    private void Awake()
    {
        if (gameInput == null || pauseController == null || gameFlowOrchestrator == null || gameStateService == null)
        {
            throw new InvalidOperationException("PauseMenuController references are not fully assigned.");
        }

        pauseDocument = GetComponent<UIDocument>();
        VisualElement root = pauseDocument.rootVisualElement;
        pauseRoot = root.Q<VisualElement>("pause-root");
        returnBaseButton = root.Q<Button>("return-base-btn");
        returnMainMenuButton = root.Q<Button>("return-mainmenu-btn");

        if (pauseRoot == null || returnBaseButton == null || returnMainMenuButton == null)
        {
            throw new InvalidOperationException("PauseMenuController UI binding failed.");
        }
    }

    private void OnEnable()
    {
        returnBaseButton.clicked += HandleReturnBaseClicked;
        returnMainMenuButton.clicked += HandleReturnMainMenuClicked;
        SetPauseVisible(false);
        pauseController.SetPaused(false);
    }

    private void OnDisable()
    {
        CursorPolicyService.ReleaseUiCursor(CursorOwner);
        returnBaseButton.clicked -= HandleReturnBaseClicked;
        returnMainMenuButton.clicked -= HandleReturnMainMenuClicked;
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

        bool nextPaused = !GamePauseController.IsPaused;
        pauseController.SetPaused(nextPaused);
        ConfigureButtonsByState(gameStateService.CurrentState);
        SetPauseVisible(nextPaused);
    }

    private void HandleReturnBaseClicked()
    {
        pauseController.SetPaused(false);
        SetPauseVisible(false);
        gameFlowOrchestrator.ReturnToBase();
    }

    private void HandleReturnMainMenuClicked()
    {
        pauseController.SetPaused(false);
        SetPauseVisible(false);
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
        return state == GameStateId.InRun || state == GameStateId.Base;
    }
}
