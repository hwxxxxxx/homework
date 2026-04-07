using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Unity.AI.Navigation;

public static class GlobalRuntimeBootstrap
{
    private const string PersistentSceneName = "Persistent";
    private const string SystemsName = "GlobalSystems";
    private const string GlobalEventSystemName = "GlobalEventSystem";
    private const string BattleSharedRootName = "BattleSharedRoot";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            DestroyPersistentRuntime();
            return;
        }

        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            return;
        }

        GamePauseController controller = GetPauseController();
        GameStateMachineService gameStateService = GetGameStateService();
        if (controller == null)
        {
            return;
        }

        bool isGameplayScene = IsGameplayScene(scene.name);

        controller.enabled = true;
        controller.ConfigurePauseOnlyWhenInRun(isGameplayScene);
        controller.ConfigureBaseScene("BaseScene_Main");
        controller.ConfigureMainMenuScene("MainMenu");
        controller.ConfigureBaseSceneUiController(scene.name == "BaseScene_Main"
            ? Object.FindObjectOfType<BaseSceneUIController>(true)
            : null);
        SetBattleSharedRootActive(isGameplayScene);
        SyncGlobalStateForScene(scene.name, gameStateService);
        if (isGameplayScene)
        {
            InitializeGameplayRuntime(scene.name, gameStateService);
        }

        SyncGlobalEventSystem();
        SyncGlobalAchievementProgress();
        RemoveSceneLocalAchievementServices();
    }

    private static bool IsGameplayScene(string sceneName)
    {
        return sceneName == "GameScene" ||
               sceneName == "Level_Body_2" ||
               sceneName == "Level_Soul_1" ||
               sceneName == "Level_Memory_1";
    }

    private static void SyncGlobalEventSystem()
    {
        EventSystem global = GetGlobalEventSystem();
        if (global == null)
        {
            return;
        }

        EventSystem[] all = Object.FindObjectsOfType<EventSystem>(true);
        bool hasSceneLocalEventSystem = false;
        for (int i = 0; i < all.Length; i++)
        {
            EventSystem candidate = all[i];
            if (candidate == null || candidate == global)
            {
                continue;
            }

            if (!candidate.isActiveAndEnabled)
            {
                continue;
            }

            hasSceneLocalEventSystem = true;
            break;
        }

        global.gameObject.SetActive(!hasSceneLocalEventSystem);
    }

    private static void SyncGlobalAchievementProgress()
    {
        AchievementService globalAchievement = GetGlobalAchievementService();
        if (globalAchievement == null)
        {
            return;
        }

        ProgressService progress = Object.FindObjectOfType<ProgressService>();
        if (progress != null)
        {
            globalAchievement.ConfigureProgressService(progress);
        }
    }

    private static void SyncGlobalStateForScene(string sceneName, GameStateMachineService gameStateService)
    {
        if (gameStateService == null)
        {
            return;
        }

        if (sceneName == "BaseScene_Main")
        {
            if (gameStateService.CurrentState == GameStateId.Boot)
            {
                gameStateService.TrySetState(GameStateId.Base);
            }
            else if (gameStateService.CurrentState == GameStateId.LoadingBase)
            {
                gameStateService.TrySetState(GameStateId.Base);
            }

            return;
        }

    }

    private static void InitializeGameplayRuntime(string sceneName, GameStateMachineService gameStateService)
    {
        Scene gameplayScene = SceneManager.GetSceneByName(sceneName);
        if (!gameplayScene.IsValid() || !gameplayScene.isLoaded)
        {
            return;
        }

        LevelRuntimeBinding runtimeBinding = ResolveRuntimeBinding(gameplayScene);
        if (runtimeBinding == null)
        {
            return;
        }

        BuildNavMesh(runtimeBinding);
        SpawnManager spawnManager = Object.FindObjectOfType<SpawnManager>(true);
        if (spawnManager != null)
        {
            spawnManager.ConfigureRuntimeBinding(runtimeBinding);
            spawnManager.PrewarmForCurrentRun();
        }

        if (gameStateService != null && gameStateService.CurrentState == GameStateId.LoadingRun)
        {
            gameStateService.TrySetState(GameStateId.InRun);
        }
    }

    private static void SetBattleSharedRootActive(bool active)
    {
        PersistentRuntimeRoot root = Object.FindObjectOfType<PersistentRuntimeRoot>(true);
        if (root == null)
        {
            return;
        }

        Transform battleRoot = root.transform.Find(BattleSharedRootName);
        if (battleRoot != null)
        {
            battleRoot.gameObject.SetActive(active);
        }
    }

    private static void BuildNavMesh(LevelRuntimeBinding runtimeBinding)
    {
        if (runtimeBinding.NavMeshSurfaces == null)
        {
            return;
        }

        for (int i = 0; i < runtimeBinding.NavMeshSurfaces.Length; i++)
        {
            NavMeshSurface surface = runtimeBinding.NavMeshSurfaces[i];
            if (surface != null)
            {
                surface.BuildNavMesh();
            }
        }
    }

    private static LevelRuntimeBinding ResolveRuntimeBinding(Scene levelScene)
    {
        GameObject[] roots = levelScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            LevelRuntimeBinding binding = roots[i].GetComponentInChildren<LevelRuntimeBinding>(true);
            if (binding != null)
            {
                return binding;
            }
        }

        return null;
    }

    private static void RemoveSceneLocalAchievementServices()
    {
        AchievementService globalAchievement = GetGlobalAchievementService();
        if (globalAchievement == null)
        {
            return;
        }

        AchievementService[] all = Object.FindObjectsOfType<AchievementService>(true);
        for (int i = 0; i < all.Length; i++)
        {
            AchievementService candidate = all[i];
            if (candidate == null || candidate == globalAchievement)
            {
                continue;
            }

            Object.Destroy(candidate);
        }
    }

    private static AchievementService GetGlobalAchievementService()
    {
        AchievementService[] all = Object.FindObjectsOfType<AchievementService>(true);
        for (int i = 0; i < all.Length; i++)
        {
            AchievementService service = all[i];
            if (service == null)
            {
                continue;
            }

            Transform parent = service.transform.parent;
            if (parent != null && parent.name == SystemsName)
            {
                return service;
            }
        }

        return null;
    }

    private static GamePauseController GetPauseController()
    {
        return Object.FindObjectOfType<GamePauseController>(true);
    }

    private static GameStateMachineService GetGameStateService()
    {
        return Object.FindObjectOfType<GameStateMachineService>(true);
    }

    private static void DestroyPersistentRuntime()
    {
        GamePauseController controller = GetPauseController();
        if (controller != null)
        {
            controller.ForceClearPauseUi();
        }

        PoolService.ClearAllPools();
        SetBattleSharedRootActive(false);

        PersistentRuntimeRoot[] roots = Object.FindObjectsOfType<PersistentRuntimeRoot>(true);
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i] != null)
            {
                Object.Destroy(roots[i].gameObject);
            }
        }

        Scene persistentScene = SceneManager.GetSceneByName(PersistentSceneName);
        if (persistentScene.IsValid() && persistentScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(PersistentSceneName);
        }
    }

    private static EventSystem GetGlobalEventSystem()
    {
        EventSystem[] all = Object.FindObjectsOfType<EventSystem>(true);
        for (int i = 0; i < all.Length; i++)
        {
            EventSystem eventSystem = all[i];
            if (eventSystem == null)
            {
                continue;
            }

            if (eventSystem.name == GlobalEventSystemName)
            {
                return eventSystem;
            }
        }

        return null;
    }
}
