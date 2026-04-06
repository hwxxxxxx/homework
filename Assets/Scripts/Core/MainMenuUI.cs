using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private MainMenuFlowController flowController;

    private void OnEnable()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnStartGame()
    {
        flowController.EnterBase();
    }

    public void OnQuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
