using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuFlowController : MonoBehaviour
{
    [SerializeField] private string baseSceneName = "BaseScene_Main";

    public void EnterBase()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(baseSceneName, LoadSceneMode.Single);
    }
}
