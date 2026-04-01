using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private string baseSceneName = "BaseScene_Main";

    private void OnEnable()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnStartGame()
    {
        if (!string.IsNullOrWhiteSpace(baseSceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(baseSceneName, LoadSceneMode.Single);
            return;
        }

        if (gameStateService != null)
        {
            gameStateService.TrySetState(GameStateId.Base);
            return;
        }

        Debug.LogError("MainMenuUI: missing Base scene name and GameStateMachineService reference.", this);
    }

    public void OnQuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
