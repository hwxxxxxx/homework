using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    private const string CursorOwner = "MainMenuUI";

    [SerializeField] private GameFlowOrchestrator flowOrchestrator;

    public void ConfigureRuntimeServices(GameFlowOrchestrator runtimeFlowOrchestrator)
    {
        flowOrchestrator = runtimeFlowOrchestrator;
    }

    private void OnEnable()
    {
        Time.timeScale = 1f;
        CursorPolicyService.AcquireUiCursor(CursorOwner);
    }

    private void OnDisable()
    {
        CursorPolicyService.ReleaseUiCursor(CursorOwner);
    }

    public void OnStartGame()
    {
        flowOrchestrator.EnterBase();
    }

    public void OnQuitGame()
    {
        Application.Quit();
    }
}
