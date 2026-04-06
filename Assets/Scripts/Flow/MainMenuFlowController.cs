using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuFlowController : MonoBehaviour
{
    [SerializeField] private string persistentSceneName = "Persistent";
    [SerializeField] private string baseSceneName = "BaseScene_Main";
    [SerializeField] private string loadingMessage = "Entering base...";

    public void EnterBase()
    {
        Time.timeScale = 1f;
        StartCoroutine(EnterBaseRoutine());
    }

    private IEnumerator EnterBaseRoutine()
    {
        Scene persistentScene = SceneManager.GetSceneByName(persistentSceneName);
        if (!persistentScene.IsValid() || !persistentScene.isLoaded)
        {
            AsyncOperation loadPersistent = SceneManager.LoadSceneAsync(persistentSceneName, LoadSceneMode.Additive);
            while (loadPersistent != null && !loadPersistent.isDone)
            {
                yield return null;
            }
        }

        LoadingScreenService.TryLoadSceneSingle(
            baseSceneName,
            loadingMessage,
            keepVisibleAfterSceneLoad: false);
    }
}
