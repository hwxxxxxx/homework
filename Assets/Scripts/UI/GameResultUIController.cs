using UnityEngine;
using UnityEngine.UIElements;

public class GameResultUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private RunContextService runContextService;
    [SerializeField] private GameFlowManager gameFlowManager;

    private UIDocument resultDocument;
    private VisualElement resultRoot;
    private VisualElement victoryPanel;
    private VisualElement failPanel;
    private Button victoryBackToBaseButton;
    private Button failBackToBaseButton;

    private void Awake()
    {
        gameStateService = FindObjectOfType<GameStateMachineService>(true);
        runContextService = FindObjectOfType<RunContextService>(true);
        gameFlowManager = FindObjectOfType<GameFlowManager>(true);

        resultDocument = GetComponent<UIDocument>();
        VisualElement root = resultDocument.rootVisualElement;
        resultRoot = root.Q<VisualElement>("result-root");
        victoryPanel = root.Q<VisualElement>("victory-panel");
        failPanel = root.Q<VisualElement>("fail-panel");
        victoryBackToBaseButton = root.Q<Button>("victory-back-btn");
        failBackToBaseButton = root.Q<Button>("fail-back-btn");
    }

    private void OnEnable()
    {
        gameStateService.OnStateChanged += HandleGameStateChanged;
        HandleGameStateChanged(gameStateService.CurrentState, gameStateService.CurrentState);
        victoryBackToBaseButton.clicked += HandleBackToBaseClicked;
        failBackToBaseButton.clicked += HandleBackToBaseClicked;
    }

    private void OnDisable()
    {
        gameStateService.OnStateChanged -= HandleGameStateChanged;
        victoryBackToBaseButton.clicked -= HandleBackToBaseClicked;
        failBackToBaseButton.clicked -= HandleBackToBaseClicked;
    }

    private void HandleGameStateChanged(GameStateId previous, GameStateId current)
    {
        if (current != GameStateId.RunResult)
        {
            resultRoot.style.display = DisplayStyle.None;
            return;
        }

        bool won = runContextService.LastRunWon.HasValue && runContextService.LastRunWon.Value;
        resultRoot.style.display = DisplayStyle.Flex;
        victoryPanel.style.display = won ? DisplayStyle.Flex : DisplayStyle.None;
        failPanel.style.display = won ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void HandleBackToBaseClicked()
    {
        gameFlowManager.TryReturnToBaseFromResult();
    }
}
