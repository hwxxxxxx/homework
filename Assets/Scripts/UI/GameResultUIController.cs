using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameResultUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private RunContextService runContextService;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject failPanel;
    [SerializeField] private Button victoryBackToBaseButton;
    [SerializeField] private Button failBackToBaseButton;
    [SerializeField] private string baseSceneName = "BaseScene_Main";

    private void OnEnable()
    {
        if (gameStateService != null)
        {
            gameStateService.OnStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(gameStateService.CurrentState, gameStateService.CurrentState);
        }

        if (victoryBackToBaseButton != null)
        {
            victoryBackToBaseButton.onClick.AddListener(HandleBackToBaseClicked);
        }

        if (failBackToBaseButton != null)
        {
            failBackToBaseButton.onClick.AddListener(HandleBackToBaseClicked);
        }
    }

    private void OnDisable()
    {
        if (gameStateService != null)
        {
            gameStateService.OnStateChanged -= HandleGameStateChanged;
        }

        if (victoryBackToBaseButton != null)
        {
            victoryBackToBaseButton.onClick.RemoveListener(HandleBackToBaseClicked);
        }

        if (failBackToBaseButton != null)
        {
            failBackToBaseButton.onClick.RemoveListener(HandleBackToBaseClicked);
        }
    }

    private void HandleGameStateChanged(GameStateId previous, GameStateId current)
    {
        if (current != GameStateId.RunResult)
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }

            if (failPanel != null)
            {
                failPanel.SetActive(false);
            }

            return;
        }

        bool won = runContextService != null && runContextService.LastRunWon.HasValue && runContextService.LastRunWon.Value;
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(won);
        }

        if (failPanel != null)
        {
            failPanel.SetActive(!won);
        }
    }

    private void HandleBackToBaseClicked()
    {
        if (string.IsNullOrWhiteSpace(baseSceneName))
        {
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(baseSceneName);
    }
}
