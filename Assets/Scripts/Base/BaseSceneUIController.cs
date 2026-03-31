using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class BaseSceneUIController : MonoBehaviour
{
    [SerializeField] private RunSelectorService runSelectorService;
    [SerializeField] private LevelDefinitionAsset firstLevelDefinition;
    [SerializeField] private string firstLevelSceneName = "GameScene";

    private UIDocument uiDocument;
    private Label interactionHintLabel;
    private VisualElement battlePanel;
    private VisualElement upgradePanel;
    private Button level1Button;
    private Button startLevelButton;
    private Button closeBattleButton;
    private Button closeUpgradeButton;
    private bool battleSelectionMade;

    public bool IsModalOpen => IsVisible(battlePanel) || IsVisible(upgradePanel);

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (runSelectorService == null || firstLevelDefinition == null)
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
        if (level1Button != null)
        {
            level1Button.clicked -= HandleSelectLevel1;
        }

        if (startLevelButton != null)
        {
            startLevelButton.clicked -= HandleStartLevel;
        }

        if (closeBattleButton != null)
        {
            closeBattleButton.clicked -= HideAllPanels;
        }

        if (closeUpgradeButton != null)
        {
            closeUpgradeButton.clicked -= HideAllPanels;
        }
    }

    public void ShowBattlePanel()
    {
        HideAllPanels();
        battleSelectionMade = false;
        if (battlePanel != null)
        {
            battlePanel.style.display = DisplayStyle.Flex;
        }
    }

    public void ShowUpgradePanel()
    {
        HideAllPanels();
        if (upgradePanel != null)
        {
            upgradePanel.style.display = DisplayStyle.Flex;
        }
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
        battlePanel = root.Q<VisualElement>("battle-panel");
        upgradePanel = root.Q<VisualElement>("upgrade-panel");
        level1Button = root.Q<Button>("level1-btn");
        startLevelButton = root.Q<Button>("start-level-btn");
        closeBattleButton = root.Q<Button>("close-battle-btn");
        closeUpgradeButton = root.Q<Button>("close-upgrade-btn");

        if (interactionHintLabel == null || battlePanel == null || upgradePanel == null ||
            level1Button == null || startLevelButton == null || closeBattleButton == null || closeUpgradeButton == null)
        {
            Debug.LogError("BaseSceneUIController missing required UI nodes in UXML.", this);
            enabled = false;
            return;
        }

        level1Button.clicked -= HandleSelectLevel1;
        level1Button.clicked += HandleSelectLevel1;
        startLevelButton.clicked -= HandleStartLevel;
        startLevelButton.clicked += HandleStartLevel;
        closeBattleButton.clicked -= HideAllPanels;
        closeBattleButton.clicked += HideAllPanels;
        closeUpgradeButton.clicked -= HideAllPanels;
        closeUpgradeButton.clicked += HideAllPanels;
    }

    private void HandleSelectLevel1()
    {
        if (runSelectorService == null || firstLevelDefinition == null)
        {
            return;
        }

        battleSelectionMade = runSelectorService.TrySelectLevel(firstLevelDefinition);
    }

    private void HandleStartLevel()
    {
        if (!battleSelectionMade)
        {
            HandleSelectLevel1();
        }

        if (!battleSelectionMade || runSelectorService == null || !runSelectorService.TryStartSelectedRun())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(firstLevelSceneName))
        {
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelSceneName);
    }

    private static bool IsVisible(VisualElement panel)
    {
        return panel != null && panel.style.display.value != DisplayStyle.None;
    }
}
