using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class BaseSceneUIController : MonoBehaviour
{
    private const string CursorOwner = "BaseModalUI";
    private const float InteractionPromptHalfSize = 26f;

    [SerializeField] private GameFlowOrchestrator gameFlowOrchestrator;
    [SerializeField] private RunCatalogAsset runCatalog;
    [SerializeField] private LevelUnlockService levelUnlockService;
    [SerializeField] private ProgressService progressService;

    private readonly List<Action> battleLevelHandlers = new List<Action>();
    private readonly List<Action> unlockLevelHandlers = new List<Action>();
    private readonly List<UIDraggableWindowManipulator> windowDragManipulators = new List<UIDraggableWindowManipulator>();
    private bool isUiBound;

    private UIDocument uiDocument;
    private VisualElement interactionPrompt;
    private Label battleStatusLabel;
    private Label unlockStatusLabel;
    private VisualElement battlePanel;
    private VisualElement upgradePanel;
    private List<Button> battleLevelButtons;
    private List<Button> unlockLevelButtons;
    private Button startLevelButton;
    private Button closeBattleButton;
    private Button closeUpgradeButton;
    private RunCatalogAsset.RunEntry selectedRun;
    private UiTextConfigAsset textConfig;

    public bool IsModalOpen => IsVisible(battlePanel) || IsVisible(upgradePanel);

    public void ConfigureRuntimeServices(
        GameFlowOrchestrator flowOrchestrator,
        RunCatalogAsset runtimeRunCatalog,
        LevelUnlockService unlockService,
        ProgressService runtimeProgressService
    )
    {
        gameFlowOrchestrator = flowOrchestrator;
        runCatalog = runtimeRunCatalog;
        levelUnlockService = unlockService;
        progressService = runtimeProgressService;
    }

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        textConfig = UiTextConfigProvider.Config;
    }

    private void OnEnable()
    {
        BindUi();
        HideAllPanels();
        SetInteractionPrompt(false, Vector3.zero);
    }

    private void OnDisable()
    {
        CursorPolicyService.ReleaseUiCursor(CursorOwner);
        if (isUiBound)
        {
            UnbindUi();
        }
    }

    public void ShowBattlePanel()
    {
        HideAllPanels();
        selectedRun = null;
        battlePanel.style.display = DisplayStyle.Flex;
        CursorPolicyService.AcquireUiCursor(CursorOwner);
        RefreshBattlePanel();
    }

    public void ShowUpgradePanel()
    {
        HideAllPanels();
        upgradePanel.style.display = DisplayStyle.Flex;
        CursorPolicyService.AcquireUiCursor(CursorOwner);
        RefreshUpgradePanel();
        SetUnlockStatus(string.Empty);
    }

    public void HideAllPanels()
    {
        battlePanel.style.display = DisplayStyle.None;
        upgradePanel.style.display = DisplayStyle.None;
        CursorPolicyService.ReleaseUiCursor(CursorOwner);
    }

    public void SetInteractionPrompt(bool visible, Vector3 worldPosition)
    {
        if (!visible)
        {
            interactionPrompt.style.display = DisplayStyle.None;
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            interactionPrompt.style.display = DisplayStyle.None;
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        float rootWidth = root.resolvedStyle.width;
        float rootHeight = root.resolvedStyle.height;
        if (rootWidth <= 0f || rootHeight <= 0f)
        {
            interactionPrompt.style.display = DisplayStyle.None;
            return;
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        if (screenPosition.z <= 0f)
        {
            interactionPrompt.style.display = DisplayStyle.None;
            return;
        }

        float panelX = screenPosition.x / Screen.width * rootWidth;
        float panelY = (Screen.height - screenPosition.y) / Screen.height * rootHeight;
        interactionPrompt.style.left = panelX - InteractionPromptHalfSize;
        interactionPrompt.style.top = panelY - InteractionPromptHalfSize;
        interactionPrompt.style.display = DisplayStyle.Flex;
    }

    private void BindUi()
    {
        if (isUiBound)
        {
            UnbindUi();
        }

        VisualElement root = uiDocument.rootVisualElement;
        interactionPrompt = root.Q<VisualElement>("interaction-hint");
        battleStatusLabel = root.Q<Label>("battle-status");
        unlockStatusLabel = root.Q<Label>("unlock-status");
        battlePanel = root.Q<VisualElement>("battle-panel");
        upgradePanel = root.Q<VisualElement>("upgrade-panel");
        battleLevelButtons = root.Query<Button>(className: "battle-level-btn").ToList();
        unlockLevelButtons = root.Query<Button>(className: "unlock-level-btn").ToList();
        startLevelButton = root.Q<Button>("start-level-btn");
        closeBattleButton = root.Q<Button>("close-battle-btn");
        closeUpgradeButton = root.Q<Button>("close-upgrade-btn");
        ValidateBoundElements();
        RegisterDraggableWindow(root, battlePanel, "battle-drag-handle");
        RegisterDraggableWindow(root, upgradePanel, "upgrade-drag-handle");

        for (int i = 0; i < battleLevelButtons.Count; i++)
        {
            int index = i;
            Action handler = () => HandleSelectLevel(index);
            battleLevelButtons[i].clicked += handler;
            battleLevelHandlers.Add(handler);
        }

        for (int i = 0; i < unlockLevelButtons.Count; i++)
        {
            int index = i;
            Action handler = () => HandleUnlockLevel(index);
            unlockLevelButtons[i].clicked += handler;
            unlockLevelHandlers.Add(handler);
        }

        startLevelButton.clicked += HandleStartLevel;
        closeBattleButton.clicked += HideAllPanels;
        closeUpgradeButton.clicked += HideAllPanels;
        isUiBound = true;
    }

    private void UnbindUi()
    {
        for (int i = 0; i < windowDragManipulators.Count; i++)
        {
            windowDragManipulators[i].Dispose();
        }
        windowDragManipulators.Clear();

        int battleUnbindCount = Mathf.Min(battleLevelButtons.Count, battleLevelHandlers.Count);
        for (int i = 0; i < battleUnbindCount; i++)
        {
            battleLevelButtons[i].clicked -= battleLevelHandlers[i];
        }
        battleLevelHandlers.Clear();

        int unlockUnbindCount = Mathf.Min(unlockLevelButtons.Count, unlockLevelHandlers.Count);
        for (int i = 0; i < unlockUnbindCount; i++)
        {
            unlockLevelButtons[i].clicked -= unlockLevelHandlers[i];
        }
        unlockLevelHandlers.Clear();

        startLevelButton.clicked -= HandleStartLevel;
        closeBattleButton.clicked -= HideAllPanels;
        closeUpgradeButton.clicked -= HideAllPanels;
        isUiBound = false;
    }

    private void HandleSelectLevel(int index)
    {
        IReadOnlyList<RunCatalogAsset.RunEntry> runs = GetRuns();
        if (index < 0 || index >= runs.Count)
        {
            return;
        }

        RunCatalogAsset.RunEntry runEntry = runs[index];
        if (!progressService.IsLevelUnlocked(runEntry.RunId))
        {
            SetBattleStatus(FormatText(textConfig.BattleStatusLockedTemplate, runEntry.DisplayName));
            return;
        }

        selectedRun = runEntry;
        progressService.SetLastSelectedLevel(runEntry.RunId);
        SetBattleStatus(FormatText(textConfig.BattleStatusSelectedTemplate, runEntry.DisplayName));
    }

    private void HandleStartLevel()
    {
        if (selectedRun == null)
        {
            SetBattleStatus(textConfig.BattleStatusSelectUnlockedHint);
            return;
        }

        gameFlowOrchestrator.EnterRun(selectedRun.RunId);
        HideAllPanels();
    }

    private void HandleUnlockLevel(int index)
    {
        List<RunCatalogAsset.RunEntry> unlockableRuns = GetUnlockableRuns();
        if (index < 0 || index >= unlockableRuns.Count)
        {
            return;
        }

        RunCatalogAsset.RunEntry runEntry = unlockableRuns[index];
        TryUnlockRun(runEntry);
    }

    private void TryUnlockRun(RunCatalogAsset.RunEntry runEntry)
    {
        if (progressService.IsLevelUnlocked(runEntry.RunId))
        {
            SetUnlockStatus(FormatText(textConfig.UnlockStatusAlreadyUnlockedTemplate, runEntry.DisplayName));
            RefreshUpgradePanel();
            return;
        }

        bool unlocked = levelUnlockService.TryUnlock(runEntry);
        SetUnlockStatus(unlocked
            ? FormatText(textConfig.UnlockStatusUnlockedTemplate, runEntry.DisplayName)
            : FormatText(textConfig.UnlockStatusFailedTemplate, runEntry.DisplayName));
        RefreshUpgradePanel();
    }

    private void RefreshBattlePanel()
    {
        IReadOnlyList<RunCatalogAsset.RunEntry> runs = GetRuns();
        for (int i = 0; i < battleLevelButtons.Count; i++)
        {
            if (i < runs.Count)
            {
                SetRunButtonState(battleLevelButtons[i], runs[i]);
            }
            else
            {
                battleLevelButtons[i].text = string.Empty;
                battleLevelButtons[i].SetEnabled(false);
            }
        }

        SetBattleStatus(textConfig.BattleStatusSelectUnlockedHint);
    }

    private void RefreshUpgradePanel()
    {
        List<RunCatalogAsset.RunEntry> unlockableRuns = GetUnlockableRuns();
        for (int i = 0; i < unlockLevelButtons.Count; i++)
        {
            if (i < unlockableRuns.Count)
            {
                SetUnlockButtonState(unlockLevelButtons[i], unlockableRuns[i]);
            }
            else
            {
                unlockLevelButtons[i].text = string.Empty;
                unlockLevelButtons[i].SetEnabled(false);
            }
        }
    }

    private List<RunCatalogAsset.RunEntry> GetUnlockableRuns()
    {
        IReadOnlyList<RunCatalogAsset.RunEntry> runs = GetRuns();
        List<RunCatalogAsset.RunEntry> unlockable = new List<RunCatalogAsset.RunEntry>();
        for (int i = 0; i < runs.Count; i++)
        {
            RunCatalogAsset.RunEntry runEntry = runs[i];
            if (runEntry.UnlockCostAmount > 0 || runEntry.PrerequisiteRunIds.Count > 0)
            {
                unlockable.Add(runEntry);
            }
        }

        return unlockable;
    }

    private IReadOnlyList<RunCatalogAsset.RunEntry> GetRuns()
    {
        return runCatalog.Runs;
    }

    private void SetRunButtonState(Button button, RunCatalogAsset.RunEntry runEntry)
    {
        bool unlocked = progressService.IsLevelUnlocked(runEntry.RunId);
        button.text = $"{runEntry.DisplayName} {(unlocked ? textConfig.LevelButtonUnlockedSuffix : textConfig.LevelButtonLockedSuffix)}";
        button.SetEnabled(unlocked);
    }

    private void SetUnlockButtonState(Button button, RunCatalogAsset.RunEntry runEntry)
    {
        bool unlocked = progressService.IsLevelUnlocked(runEntry.RunId);
        button.text = unlocked
            ? FormatText(textConfig.UnlockButtonUnlockedTemplate, runEntry.DisplayName)
            : FormatText(textConfig.UnlockButtonActionTemplate, runEntry.DisplayName);
        button.SetEnabled(!unlocked);
    }

    private void SetBattleStatus(string text)
    {
        battleStatusLabel.text = text ?? string.Empty;
    }

    private void SetUnlockStatus(string text)
    {
        unlockStatusLabel.text = text ?? string.Empty;
    }

    private static bool IsVisible(VisualElement panel)
    {
        return panel.style.display.value != DisplayStyle.None;
    }

    private static string FormatText(string template, string value)
    {
        return string.Format(template, value);
    }

    private void RegisterDraggableWindow(VisualElement root, VisualElement window, string handleName)
    {
        VisualElement handle = root.Q<VisualElement>(handleName);
        UIDraggableWindowManipulator manipulator = new UIDraggableWindowManipulator(root, window, handle);
        windowDragManipulators.Add(manipulator);
    }

    private void ValidateBoundElements()
    {
        if (interactionPrompt == null ||
            battleStatusLabel == null ||
            unlockStatusLabel == null ||
            battlePanel == null ||
            upgradePanel == null ||
            startLevelButton == null ||
            closeBattleButton == null ||
            closeUpgradeButton == null)
        {
            throw new InvalidOperationException("BaseRoot.uxml is missing required named UI elements.");
        }
    }
}
