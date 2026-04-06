using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class BaseSceneUIController : MonoBehaviour
{
    [SerializeField] private RunSelectorService runSelectorService;
    [SerializeField] private LevelUnlockService levelUnlockService;
    [SerializeField] private ProgressService progressService;

    [Header("Level Definitions")]
    [SerializeField] private LevelDefinitionAsset bodyLevel1Definition;
    [SerializeField] private LevelDefinitionAsset bodyLevel2Definition;
    [SerializeField] private LevelDefinitionAsset soulLevel1Definition;
    [SerializeField] private LevelDefinitionAsset memoryLevel1Definition;

    private UIDocument uiDocument;
    private Label interactionHintLabel;
    private Label battleStatusLabel;
    private Label unlockStatusLabel;
    private VisualElement battlePanel;
    private VisualElement upgradePanel;
    private Button levelBody1Button;
    private Button levelBody2Button;
    private Button levelSoul1Button;
    private Button levelMemory1Button;
    private Button startLevelButton;
    private Button closeBattleButton;
    private Button unlockBody2Button;
    private Button unlockSoul1Button;
    private Button unlockMemory1Button;
    private Button closeUpgradeButton;
    private LevelDefinitionAsset selectedLevel;

    public bool IsModalOpen => IsVisible(battlePanel) || IsVisible(upgradePanel);

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (runSelectorService == null || levelUnlockService == null || progressService == null ||
            bodyLevel1Definition == null || bodyLevel2Definition == null ||
            soulLevel1Definition == null || memoryLevel1Definition == null)
        {
            Debug.LogError("BaseSceneUIController references are not fully assigned.", this);
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        BindUi();
        HideAllPanels();
        SetInteractionHint(string.Empty);
    }

    private void OnDisable()
    {
        if (levelBody1Button != null)
        {
            levelBody1Button.clicked -= HandleSelectBody1;
        }

        if (levelBody2Button != null)
        {
            levelBody2Button.clicked -= HandleSelectBody2;
        }

        if (levelSoul1Button != null)
        {
            levelSoul1Button.clicked -= HandleSelectSoul1;
        }

        if (levelMemory1Button != null)
        {
            levelMemory1Button.clicked -= HandleSelectMemory1;
        }

        if (startLevelButton != null)
        {
            startLevelButton.clicked -= HandleStartLevel;
        }

        if (closeBattleButton != null)
        {
            closeBattleButton.clicked -= HideAllPanels;
        }

        if (unlockBody2Button != null)
        {
            unlockBody2Button.clicked -= HandleUnlockBody2;
        }

        if (unlockSoul1Button != null)
        {
            unlockSoul1Button.clicked -= HandleUnlockSoul1;
        }

        if (unlockMemory1Button != null)
        {
            unlockMemory1Button.clicked -= HandleUnlockMemory1;
        }

        if (closeUpgradeButton != null)
        {
            closeUpgradeButton.clicked -= HideAllPanels;
        }
    }

    public void ShowBattlePanel()
    {
        HideAllPanels();
        selectedLevel = null;
        if (battlePanel != null)
        {
            battlePanel.style.display = DisplayStyle.Flex;
        }

        RefreshBattlePanel();
        ApplyModalCursorState(true);
    }

    public void ShowUpgradePanel()
    {
        HideAllPanels();
        if (upgradePanel != null)
        {
            upgradePanel.style.display = DisplayStyle.Flex;
        }

        RefreshUpgradePanel();
        SetUnlockStatus(string.Empty);
        ApplyModalCursorState(true);
    }

    public void HideAllPanels()
    {
        if (battlePanel != null)
        {
            battlePanel.style.display = DisplayStyle.None;
        }

        if (upgradePanel != null)
        {
            upgradePanel.style.display = DisplayStyle.None;
        }

        ApplyModalCursorState(false);
    }

    public void SetInteractionHint(string hint)
    {
        if (interactionHintLabel == null)
        {
            return;
        }

        bool hasHint = !string.IsNullOrWhiteSpace(hint);
        interactionHintLabel.text = hasHint ? hint : string.Empty;
        interactionHintLabel.style.display = hasHint ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void BindUi()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null)
        {
            Debug.LogError("BaseSceneUIController UIDocument root is missing.", this);
            enabled = false;
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        interactionHintLabel = root.Q<Label>("interaction-hint");
        battleStatusLabel = root.Q<Label>("battle-status");
        unlockStatusLabel = root.Q<Label>("unlock-status");
        battlePanel = root.Q<VisualElement>("battle-panel");
        upgradePanel = root.Q<VisualElement>("upgrade-panel");
        levelBody1Button = root.Q<Button>("level-body1-btn");
        levelBody2Button = root.Q<Button>("level-body2-btn");
        levelSoul1Button = root.Q<Button>("level-soul1-btn");
        levelMemory1Button = root.Q<Button>("level-memory1-btn");
        startLevelButton = root.Q<Button>("start-level-btn");
        closeBattleButton = root.Q<Button>("close-battle-btn");
        unlockBody2Button = root.Q<Button>("unlock-body2-btn");
        unlockSoul1Button = root.Q<Button>("unlock-soul1-btn");
        unlockMemory1Button = root.Q<Button>("unlock-memory1-btn");
        closeUpgradeButton = root.Q<Button>("close-upgrade-btn");

        if (interactionHintLabel == null || battleStatusLabel == null || unlockStatusLabel == null ||
            battlePanel == null || upgradePanel == null || levelBody1Button == null || levelBody2Button == null ||
            levelSoul1Button == null || levelMemory1Button == null || startLevelButton == null ||
            closeBattleButton == null || unlockBody2Button == null || unlockSoul1Button == null ||
            unlockMemory1Button == null || closeUpgradeButton == null)
        {
            Debug.LogError("BaseSceneUIController missing required UI nodes in UXML.", this);
            enabled = false;
            return;
        }

        levelBody1Button.clicked -= HandleSelectBody1;
        levelBody1Button.clicked += HandleSelectBody1;
        levelBody2Button.clicked -= HandleSelectBody2;
        levelBody2Button.clicked += HandleSelectBody2;
        levelSoul1Button.clicked -= HandleSelectSoul1;
        levelSoul1Button.clicked += HandleSelectSoul1;
        levelMemory1Button.clicked -= HandleSelectMemory1;
        levelMemory1Button.clicked += HandleSelectMemory1;
        startLevelButton.clicked -= HandleStartLevel;
        startLevelButton.clicked += HandleStartLevel;
        closeBattleButton.clicked -= HideAllPanels;
        closeBattleButton.clicked += HideAllPanels;
        unlockBody2Button.clicked -= HandleUnlockBody2;
        unlockBody2Button.clicked += HandleUnlockBody2;
        unlockSoul1Button.clicked -= HandleUnlockSoul1;
        unlockSoul1Button.clicked += HandleUnlockSoul1;
        unlockMemory1Button.clicked -= HandleUnlockMemory1;
        unlockMemory1Button.clicked += HandleUnlockMemory1;
        closeUpgradeButton.clicked -= HideAllPanels;
        closeUpgradeButton.clicked += HideAllPanels;
    }

    private void HandleSelectBody1()
    {
        TrySelectLevel(bodyLevel1Definition);
    }

    private void HandleSelectBody2()
    {
        TrySelectLevel(bodyLevel2Definition);
    }

    private void HandleSelectSoul1()
    {
        TrySelectLevel(soulLevel1Definition);
    }

    private void HandleSelectMemory1()
    {
        TrySelectLevel(memoryLevel1Definition);
    }

    private void TrySelectLevel(LevelDefinitionAsset levelDefinition)
    {
        if (levelDefinition == null || runSelectorService == null)
        {
            return;
        }

        if (!runSelectorService.TrySelectLevel(levelDefinition))
        {
            SetBattleStatus($"Locked: {levelDefinition.DisplayName}");
            return;
        }

        selectedLevel = levelDefinition;
        SetBattleStatus($"Selected: {levelDefinition.DisplayName}");
    }

    private void HandleStartLevel()
    {
        if (selectedLevel == null)
        {
            SetBattleStatus("Select an unlocked level first.");
            return;
        }

        if (runSelectorService == null || !runSelectorService.TryStartSelectedRun())
        {
            return;
        }

        HideAllPanels();
    }

    private void HandleUnlockBody2()
    {
        TryUnlockLevel(bodyLevel2Definition);
    }

    private void HandleUnlockSoul1()
    {
        TryUnlockLevel(soulLevel1Definition);
    }

    private void HandleUnlockMemory1()
    {
        TryUnlockLevel(memoryLevel1Definition);
    }

    private void TryUnlockLevel(LevelDefinitionAsset levelDefinition)
    {
        if (levelDefinition == null || levelUnlockService == null || progressService == null)
        {
            return;
        }

        if (progressService.IsLevelUnlocked(levelDefinition.LevelId))
        {
            SetUnlockStatus($"Already unlocked: {levelDefinition.DisplayName}");
            RefreshUpgradePanel();
            return;
        }

        bool unlocked = levelUnlockService.TryUnlock(levelDefinition);
        SetUnlockStatus(unlocked
            ? $"Unlocked: {levelDefinition.DisplayName}"
            : $"Unlock failed: {levelDefinition.DisplayName}");
        RefreshUpgradePanel();
    }

    private void RefreshBattlePanel()
    {
        if (progressService == null)
        {
            return;
        }

        SetLevelButtonState(levelBody1Button, bodyLevel1Definition);
        SetLevelButtonState(levelBody2Button, bodyLevel2Definition);
        SetLevelButtonState(levelSoul1Button, soulLevel1Definition);
        SetLevelButtonState(levelMemory1Button, memoryLevel1Definition);
        SetBattleStatus("Select an unlocked level.");
    }

    private void RefreshUpgradePanel()
    {
        if (progressService == null)
        {
            return;
        }

        SetUnlockButtonState(unlockBody2Button, bodyLevel2Definition);
        SetUnlockButtonState(unlockSoul1Button, soulLevel1Definition);
        SetUnlockButtonState(unlockMemory1Button, memoryLevel1Definition);
    }

    private void SetLevelButtonState(Button button, LevelDefinitionAsset levelDefinition)
    {
        if (button == null || levelDefinition == null || progressService == null)
        {
            return;
        }

        bool unlocked = progressService.IsLevelUnlocked(levelDefinition.LevelId);
        button.text = $"{levelDefinition.DisplayName} {(unlocked ? "[Unlocked]" : "[Locked]")}";
        button.SetEnabled(unlocked);
    }

    private void SetUnlockButtonState(Button button, LevelDefinitionAsset levelDefinition)
    {
        if (button == null || levelDefinition == null || progressService == null)
        {
            return;
        }

        bool unlocked = progressService.IsLevelUnlocked(levelDefinition.LevelId);
        button.text = unlocked
            ? $"Unlocked: {levelDefinition.DisplayName}"
            : $"Unlock: {levelDefinition.DisplayName}";
        button.SetEnabled(!unlocked);
    }

    private void SetBattleStatus(string text)
    {
        if (battleStatusLabel != null)
        {
            battleStatusLabel.text = text ?? string.Empty;
        }
    }

    private void SetUnlockStatus(string text)
    {
        if (unlockStatusLabel != null)
        {
            unlockStatusLabel.text = text ?? string.Empty;
        }
    }

    private static bool IsVisible(VisualElement panel)
    {
        return panel != null && panel.style.display.value != DisplayStyle.None;
    }

    private static void ApplyModalCursorState(bool modalOpen)
    {
        if (modalOpen)
        {
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            return;
        }

        if (GamePauseController.IsPaused)
        {
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            return;
        }

        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
}
