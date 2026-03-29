using UnityEngine;

public class GameResultUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameFlowManager gameFlowManager;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject failPanel;

    private void OnEnable()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.OnStateChanged += HandleStateChanged;
            HandleStateChanged(gameFlowManager.CurrentState);
        }
    }

    private void OnDisable()
    {
        if (gameFlowManager != null)
        {
            gameFlowManager.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameFlowManager.GameFlowState state)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(state == GameFlowManager.GameFlowState.Victory);
        }

        if (failPanel != null)
        {
            failPanel.SetActive(state == GameFlowManager.GameFlowState.Fail);
        }
    }
}
