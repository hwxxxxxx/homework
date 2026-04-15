using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class BaseSceneUIController : MonoBehaviour
{
    private const string CursorOwner = "BaseModalUI";
    private const float InteractionPromptHalfSize = 26f;
    private static readonly Vector2 InteractionPromptDefaultAnchor = new Vector2(0.58f, 0.42f);

    [SerializeField] private GameFlowOrchestrator gameFlowOrchestrator;
    [SerializeField] private RunCatalogAsset runCatalog;
    [SerializeField] private ProgressService progressService;
    [Header("Interaction Prompt Layout")]
    [SerializeField] private Vector2 interactionPromptAnchor = InteractionPromptDefaultAnchor;
    [SerializeField] private Vector2 interactionPromptPixelOffset = Vector2.zero;

    private readonly List<Action> battleLevelHandlers = new List<Action>();
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
        ProgressService runtimeProgressService
    )
    {
        gameFlowOrchestrator = flowOrchestrator;
        runCatalog = runtimeRunCatalog;
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
        SetUnlockStatus("关卡会在完成前置关卡后自动解锁。");
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

        VisualElement root = uiDocument.rootVisualElement;
        float rootWidth = root.resolvedStyle.width;
        float rootHeight = root.resolvedStyle.height;
        if (rootWidth <= 0f || rootHeight <= 0f)
        {
            interactionPrompt.style.display = DisplayStyle.None;
            return;
        }

        float anchorX = Mathf.Clamp01(interactionPromptAnchor.x);
        float anchorY = Mathf.Clamp01(interactionPromptAnchor.y);
        float panelX = rootWidth * anchorX + interactionPromptPixelOffset.x;
        float panelY = rootHeight * anchorY + interactionPromptPixelOffset.y;
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
        if (unlocked)
        {
            button.text = $"{runEntry.DisplayName} {textConfig.LevelButtonUnlockedSuffix}";
            button.SetEnabled(false);
            return;
        }

        string requirement = BuildRequirementText(runEntry);
        button.text = $"{runEntry.DisplayName} ({requirement})";
        button.SetEnabled(false);
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

    private string BuildRequirementText(RunCatalogAsset.RunEntry runEntry)
    {
        if (runEntry.PrerequisiteRunIds == null || runEntry.PrerequisiteRunIds.Count == 0)
        {
            return "自动解锁";
        }

        List<string> names = new List<string>();
        for (int i = 0; i < runEntry.PrerequisiteRunIds.Count; i++)
        {
            string prerequisiteRunId = runEntry.PrerequisiteRunIds[i];
            string displayName = ResolveRunDisplayName(prerequisiteRunId);
            bool completed = progressService.IsRunCompleted(prerequisiteRunId);
            names.Add(completed ? $"{displayName}：已通关" : $"{displayName}：未通关");
        }

        return string.Join(", ", names);
    }

    private string ResolveRunDisplayName(string runId)
    {
        IReadOnlyList<RunCatalogAsset.RunEntry> runs = GetRuns();
        for (int i = 0; i < runs.Count; i++)
        {
            if (runs[i].RunId == runId)
            {
                return runs[i].DisplayName;
            }
        }

        return runId;
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
