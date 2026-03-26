using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnStartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnQuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}