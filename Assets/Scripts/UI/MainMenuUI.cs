using System;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuUI : MonoBehaviour
{
    private const string CursorOwner = "MainMenuUI";

    [SerializeField] private GameFlowOrchestrator flowOrchestrator;
    [SerializeField] private UIDocument menuDocument;

    private VisualElement mainMenuRoot;
    private VisualElement saveSelectRoot;
    private Button startButton;
    private Button saveStartButton;
    private Button saveBackButton;
    private Button slot1Button;
    private Button slot2Button;
    private Button slot3Button;
    private Button settingsButton;
    private Button quitButton;
    private GameSettingsUiPresenter settingsPresenter;
    private int selectedSlotId;

    public void ConfigureRuntimeServices(GameFlowOrchestrator runtimeFlowOrchestrator)
    {
        flowOrchestrator = runtimeFlowOrchestrator;
    }

    public void OnStartGame()
    {
        HandleStartGameClicked();
    }

    public void OnQuitGame()
    {
        HandleQuitClicked();
    }

    private void Awake()
    {
        if (menuDocument == null)
        {
            menuDocument = GetComponent<UIDocument>();
        }

        if (menuDocument == null)
        {
            throw new InvalidOperationException("MainMenuUI requires UIDocument.");
        }

        VisualElement root = menuDocument.rootVisualElement;
        mainMenuRoot = root.Q<VisualElement>("mainmenu-root");
        saveSelectRoot = root.Q<VisualElement>("save-select-root");
        startButton = root.Q<Button>("mainmenu-start-btn");
        saveStartButton = root.Q<Button>("save-start-btn");
        saveBackButton = root.Q<Button>("save-back-btn");
        slot1Button = root.Q<Button>("save-slot-1-btn");
        slot2Button = root.Q<Button>("save-slot-2-btn");
        slot3Button = root.Q<Button>("save-slot-3-btn");
        settingsButton = root.Q<Button>("mainmenu-settings-btn");
        quitButton = root.Q<Button>("mainmenu-quit-btn");

        if (mainMenuRoot == null ||
            saveSelectRoot == null ||
            startButton == null ||
            saveStartButton == null ||
            saveBackButton == null ||
            slot1Button == null ||
            slot2Button == null ||
            slot3Button == null ||
            settingsButton == null ||
            quitButton == null)
        {
            throw new InvalidOperationException("MainMenuUI UI binding failed.");
        }

        settingsPresenter = new GameSettingsUiPresenter(root);
    }

    private void OnEnable()
    {
        Time.timeScale = 1f;
        CursorPolicyService.AcquireUiCursor(CursorOwner);
        startButton.clicked += HandleStartGameClicked;
        saveStartButton.clicked += HandleSaveStartClicked;
        saveBackButton.clicked += HandleSaveBackClicked;
        slot1Button.clicked += HandleSlot1Clicked;
        slot2Button.clicked += HandleSlot2Clicked;
        slot3Button.clicked += HandleSlot3Clicked;
        settingsButton.clicked += HandleOpenSettingsClicked;
        quitButton.clicked += HandleQuitClicked;
        settingsPresenter.Hide(saveAudioSettings: false);
        CloseSaveSelectModal();
    }

    private void OnDisable()
    {
        CursorPolicyService.ReleaseUiCursor(CursorOwner);
        startButton.clicked -= HandleStartGameClicked;
        saveStartButton.clicked -= HandleSaveStartClicked;
        saveBackButton.clicked -= HandleSaveBackClicked;
        slot1Button.clicked -= HandleSlot1Clicked;
        slot2Button.clicked -= HandleSlot2Clicked;
        slot3Button.clicked -= HandleSlot3Clicked;
        settingsButton.clicked -= HandleOpenSettingsClicked;
        quitButton.clicked -= HandleQuitClicked;
        settingsPresenter.Hide(saveAudioSettings: true);
    }

    private void OnDestroy()
    {
        settingsPresenter?.Dispose();
        settingsPresenter = null;
    }

    private void HandleStartGameClicked()
    {
        OpenSaveSelectModal();
    }

    private void HandleSaveStartClicked()
    {
        SaveSlotService.SelectSlot(selectedSlotId);
        CloseSaveSelectModal();
        flowOrchestrator.EnterBase();
    }

    private void HandleSaveBackClicked()
    {
        CloseSaveSelectModal();
    }

    private void HandleSlot1Clicked()
    {
        SelectSlot(1);
    }

    private void HandleSlot2Clicked()
    {
        SelectSlot(2);
    }

    private void HandleSlot3Clicked()
    {
        SelectSlot(3);
    }

    private void HandleOpenSettingsClicked()
    {
        settingsPresenter.Open();
    }

    private static void HandleQuitClicked()
    {
        Application.Quit();
    }

    private void OpenSaveSelectModal()
    {
        selectedSlotId = 0;
        RefreshSlotTexts();
        RefreshSlotSelectionVisual();
        saveStartButton.SetEnabled(false);
        saveSelectRoot.style.display = DisplayStyle.Flex;
    }

    private void CloseSaveSelectModal()
    {
        selectedSlotId = 0;
        RefreshSlotSelectionVisual();
        saveStartButton.SetEnabled(false);
        saveSelectRoot.style.display = DisplayStyle.None;
    }

    private void SelectSlot(int slotId)
    {
        selectedSlotId = slotId;
        RefreshSlotSelectionVisual();
        saveStartButton.SetEnabled(true);
    }

    private void RefreshSlotSelectionVisual()
    {
        ApplySlotSelectedClass(slot1Button, selectedSlotId == 1);
        ApplySlotSelectedClass(slot2Button, selectedSlotId == 2);
        ApplySlotSelectedClass(slot3Button, selectedSlotId == 3);
    }

    private void RefreshSlotTexts()
    {
        slot1Button.text = BuildSlotText(SaveSlotService.GetSlotSummary(1));
        slot2Button.text = BuildSlotText(SaveSlotService.GetSlotSummary(2));
        slot3Button.text = BuildSlotText(SaveSlotService.GetSlotSummary(3));
    }

    private static string BuildSlotText(SaveSlotService.SaveSlotSummary summary)
    {
        if (!summary.HasData)
        {
            return $"存档 {summary.SlotId}\n空";
        }

        return $"存档 {summary.SlotId}\n最近关卡：{ToLocalizedLevelName(summary.LastSelectedLevelId)}\n已解锁：{summary.UnlockedLevelCount}\n已通关：{summary.CompletedRunCount}";
    }

    private static string ToLocalizedLevelName(string runId)
    {
        switch (runId)
        {
            case "body_1":
                return "肉体回响一";
            case "body_2":
                return "肉体回响二";
            case "soul_1":
                return "灵魂回响一";
            case "memory_1":
                return "记忆回响一";
            default:
                return "未选择";
        }
    }

    private static void ApplySlotSelectedClass(Button slotButton, bool selected)
    {
        if (selected)
        {
            slotButton.AddToClassList("save-slot-btn--selected");
            return;
        }

        slotButton.RemoveFromClassList("save-slot-btn--selected");
    }
}
