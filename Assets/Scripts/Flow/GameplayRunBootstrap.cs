using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;

public class GameplayRunBootstrap : MonoBehaviour
{
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private RunContextService runContextService;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private LevelDefinitionAsset[] availableLevels;

    private bool hasStarted;

    private void Awake()
    {
        if (gameStateService == null)
        {
            gameStateService = FindObjectOfType<GameStateMachineService>(true);
        }

        if (runContextService == null)
        {
            runContextService = FindObjectOfType<RunContextService>(true);
        }

        if (spawnManager == null)
        {
            spawnManager = FindObjectOfType<SpawnManager>(true);
        }
    }

    private void Start()
    {
        if (hasStarted)
        {
            return;
        }

        hasStarted = true;
        StartCoroutine(LoadLevelAndEnterRunRoutine());
    }

    private IEnumerator LoadLevelAndEnterRunRoutine()
    {
        string targetLevelScene = RunSceneRequest.PendingLevelSceneName;
        if (string.IsNullOrWhiteSpace(targetLevelScene))
        {
            targetLevelScene = ResolveFallbackLevelScene();
        }

        if (string.IsNullOrWhiteSpace(targetLevelScene))
        {
            yield break;
        }

        if (runContextService != null)
        {
            LevelDefinitionAsset levelDefinition = ResolveLevelDefinitionByScene(targetLevelScene);
            if (levelDefinition != null)
            {
                runContextService.SetSelectedLevel(levelDefinition);
            }
        }

        if (!IsSceneLoaded(targetLevelScene))
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(targetLevelScene, LoadSceneMode.Additive);
            if (loadOperation != null)
            {
                while (!loadOperation.isDone)
                {
                    float normalized = Mathf.Clamp01(loadOperation.progress / 0.9f);
                    LoadingScreenService.SetProgress(0.15f + normalized * 0.55f);
                    yield return null;
                }
            }
        }
        else
        {
            // Keep behavior deterministic when the target scene was already present.
            yield return null;
        }

        Scene loadedLevelScene = SceneManager.GetSceneByName(targetLevelScene);
        if (!loadedLevelScene.IsValid() || !loadedLevelScene.isLoaded)
        {
            Debug.LogError($"GameplayRunBootstrap: scene '{targetLevelScene}' failed to load.");
            yield break;
        }

        LoadingScreenService.SetProgress(0.75f);
        SceneManager.SetActiveScene(loadedLevelScene);
        LevelRuntimeBinding runtimeBinding = ResolveRuntimeBinding(loadedLevelScene);
        if (runtimeBinding == null)
        {
            Debug.LogError($"GameplayRunBootstrap: missing LevelRuntimeBinding in scene '{targetLevelScene}'.");
            yield break;
        }

        BuildNavMesh(runtimeBinding);
        LoadingScreenService.SetProgress(0.88f);
        EnsureSpawnManagerReference();
        if (spawnManager == null)
        {
            Debug.LogError("GameplayRunBootstrap: missing SpawnManager reference.");
            yield break;
        }

        spawnManager.ConfigureRuntimeBinding(runtimeBinding);
        spawnManager.PrewarmForCurrentRun();
        LoadingScreenService.SetProgress(0.97f);

        RunSceneRequest.Clear();

        if (gameStateService != null && gameStateService.CurrentState == GameStateId.LoadingRun)
        {
            gameStateService.TrySetState(GameStateId.InRun);
        }

        LoadingScreenService.SetProgress(1f);
        LoadingScreenService.Hide();
    }

    private string ResolveFallbackLevelScene()
    {
        if (runContextService != null && runContextService.SelectedLevel != null)
        {
            return runContextService.SelectedLevel.SceneName;
        }

        if (availableLevels != null && availableLevels.Length > 0 && availableLevels[0] != null)
        {
            return availableLevels[0].SceneName;
        }

        return null;
    }

    private LevelDefinitionAsset ResolveLevelDefinitionByScene(string sceneName)
    {
        if (availableLevels == null || string.IsNullOrWhiteSpace(sceneName))
        {
            return null;
        }

        for (int i = 0; i < availableLevels.Length; i++)
        {
            LevelDefinitionAsset level = availableLevels[i];
            if (level == null || level.SceneName != sceneName)
            {
                continue;
            }

            return level;
        }

        return null;
    }

    private static bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.IsValid() && scene.isLoaded;
    }

    private void EnsureSpawnManagerReference()
    {
        if (spawnManager == null)
        {
            spawnManager = FindObjectOfType<SpawnManager>();
        }
    }

    private static void BuildNavMesh(LevelRuntimeBinding runtimeBinding)
    {
        if (runtimeBinding == null || runtimeBinding.NavMeshSurfaces == null)
        {
            return;
        }

        for (int i = 0; i < runtimeBinding.NavMeshSurfaces.Length; i++)
        {
            NavMeshSurface surface = runtimeBinding.NavMeshSurfaces[i];
            if (surface == null)
            {
                continue;
            }

            surface.BuildNavMesh();
        }
    }

    private static LevelRuntimeBinding ResolveRuntimeBinding(Scene levelScene)
    {
        if (!levelScene.IsValid() || !levelScene.isLoaded)
        {
            return null;
        }

        GameObject[] roots = levelScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (root == null)
            {
                continue;
            }

            LevelRuntimeBinding binding = root.GetComponentInChildren<LevelRuntimeBinding>(true);
            if (binding != null)
            {
                return binding;
            }
        }

        return null;
    }
}
