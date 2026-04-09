using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionOrchestrator : MonoBehaviour
{
    private string currentContentSceneName;

    public Scene GetCurrentContentSceneOrThrow()
    {
        if (string.IsNullOrWhiteSpace(currentContentSceneName))
        {
            throw new InvalidOperationException("Current content scene is not set.");
        }

        Scene scene = SceneManager.GetSceneByName(currentContentSceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            throw new InvalidOperationException($"Current content scene '{currentContentSceneName}' is not loaded.");
        }

        return scene;
    }

    public static IEnumerator EnsureSceneLoadedAtBoot(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            throw new ArgumentException("Boot scene name cannot be null or empty.", nameof(sceneName));
        }

        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            yield break;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOperation == null)
        {
            throw new InvalidOperationException($"Failed to load scene '{sceneName}' at boot.");
        }

        while (!loadOperation.isDone)
        {
            yield return null;
        }
    }

    public IEnumerator TransitionToContentScene(string targetSceneName, string loadingMessage)
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            throw new ArgumentException("Target scene cannot be null or empty.", nameof(targetSceneName));
        }

        LoadingScreenService.BeginTransition(loadingMessage);
        LoadingScreenService.SetProgress(0.05f);

        FlowConfigAsset config = FlowConfigProvider.Config;
        if (config == null || string.IsNullOrWhiteSpace(config.PersistentSceneName))
        {
            throw new InvalidOperationException("FlowConfig.PersistentSceneName is required.");
        }

        string persistentSceneName = config.PersistentSceneName;
        yield return EnsureSceneLoaded(persistentSceneName, 0.1f, 0.2f);
        yield return EnsureSceneLoaded(targetSceneName, 0.3f, 0.5f);

        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        if (!targetScene.IsValid() || !targetScene.isLoaded)
        {
            throw new InvalidOperationException($"Failed to load target scene '{targetSceneName}'.");
        }

        if (!SceneManager.SetActiveScene(targetScene))
        {
            throw new InvalidOperationException($"Failed to set active scene '{targetSceneName}'.");
        }

        currentContentSceneName = targetSceneName;
        LoadingScreenService.SetProgress(0.85f);
        yield return UnloadOtherContentScenes(persistentSceneName, targetSceneName);
        LoadingScreenService.SetProgress(1f);
    }

    private static IEnumerator EnsureSceneLoaded(string sceneName, float progressBase, float progressRange)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.IsValid() && scene.isLoaded)
        {
            yield break;
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOperation == null)
        {
            throw new InvalidOperationException($"Failed to start loading scene '{sceneName}'.");
        }

        while (!loadOperation.isDone)
        {
            LoadingScreenService.SetProgress(progressBase + loadOperation.progress * progressRange);
            yield return null;
        }
    }

    private static IEnumerator UnloadOtherContentScenes(string persistentSceneName, string targetSceneName)
    {
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (!loadedScene.isLoaded)
            {
                continue;
            }

            if (string.Equals(loadedScene.name, persistentSceneName, StringComparison.Ordinal) ||
                string.Equals(loadedScene.name, targetSceneName, StringComparison.Ordinal))
            {
                continue;
            }

            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(loadedScene.name);
            if (unloadOperation == null)
            {
                throw new InvalidOperationException($"Failed to unload scene '{loadedScene.name}'.");
            }

            while (!unloadOperation.isDone)
            {
                yield return null;
            }
        }
    }

}
