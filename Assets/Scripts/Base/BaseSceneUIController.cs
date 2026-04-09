using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class BaseSceneUIController : MonoBehaviour
{
    private const string CursorOwner = "BaseModalUI";

    [SerializeField] private GameFlowOrchestrator gameFlowOrchestrator;
    [SerializeField] private RunCatalogAsset runCatalog;
    [SerializeField] private LevelUnlockService levelUnlockService;
    [SerializeField] private ProgressService progressService;

    private readonly List<Action> battleLevelHandlers = new List<Action>();
    private readonly List<Action> unlockLevelHandlers = new List<Action>();

    private UIDocument uiDocument;
    private Label interactionHintLabel;
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
        SetInteractionHint(string.Empty);
    }

    private void OnDisable()
    {
        CursorPolicyService.ReleaseUiCursor(CursorOwner);
        UnbindUi();
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

    public void SetInteractionHint(string hint)
    {
        bool hasHint = !string.IsNullOrWhiteSpace(hint);
        interactionHintLabel.text = hasHint ? hint : string.Empty;
        interactionHintLabel.style.display = hasHint ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void BindUi()
    {
        VisualElement root = uiDocument.rootVisualElement;
        interactionHintLabel = root.Q<Label>("interaction-hint");
        battleStatusLabel = root.Q<Label>("battle-status");
        unlockStatusLabel = root.Q<Label>("unlock-status");
        battlePanel = root.Q<VisualElement>("battle-panel");
        upgradePanel = root.Q<VisualElement>("upgrade-panel");
        battleLevelButtons = root.Query<Button>(className: "battle-level-btn").ToList();
        unlockLevelButtons = root.Query<Button>(className: "unlock-level-btn").ToList();
        startLevelButton = root.Q<Button>("start-level-btn");
        closeBattleButton = root.Q<Button>("close-battle-btn");
        closeUpgradeButton = root.Q<Button>("close-upgrade-btn");

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
    }

    private void UnbindUi()
    {
        for (int i = 0; i < battleLevelButtons.Count; i++)
        {
            battleLevelButtons[i].clicked -= battleLevelHandlers[i];
        }
        battleLevelHandlers.Clear();

        for (int i = 0; i < unlockLevelButtons.Count; i++)
        {
            unlockLevelButtons[i].clicked -= unlockLevelHandlers[i];
        }
        unlockLevelHandlers.Clear();

        startLevelButton.clicked -= HandleStartLevel;
        closeBattleButton.clicked -= HideAllPanels;
        closeUpgradeButton.clicked -= HideAllPanels;
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
}
