using System;
using UnityEngine.SceneManagement;
using UnityEngine;

public static class ShellSceneInitializer
{
    private static BaseInteractionController activeBaseInteractionController;

    public static void InitializeMainMenu(Scene scene, RuntimeShell runtimeShell)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            throw new InvalidOperationException("MainMenu scene is not loaded.");
        }

        ExitBaseMode(runtimeShell);
        if (runtimeShell.PersistentRoot != null && runtimeShell.PersistentRoot.BattleSharedRoot != null)
        {
            runtimeShell.PersistentRoot.BattleSharedRoot.SetActive(false);
        }

        MainMenuUI menuUi = ResolveBinding<MainMenuUI>(scene);
        menuUi.ConfigureRuntimeServices(runtimeShell.GameFlowOrchestrator);

        runtimeShell.RunContextService.ResetRunState();
        runtimeShell.PauseController.SetPaused(false);
        CursorPolicyService.AcquireUiCursor("MainMenuState");
    }

    public static void InitializeBase(Scene scene, RuntimeShell runtimeShell)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            throw new InvalidOperationException("Base scene is not loaded.");
        }

        ExitBaseMode(runtimeShell);
        if (runtimeShell.PersistentRoot != null && runtimeShell.PersistentRoot.BattleSharedRoot != null)
        {
            runtimeShell.PersistentRoot.BattleSharedRoot.SetActive(true);
        }

        runtimeShell.RunContextService.ResetRunState();
        runtimeShell.PauseController.SetPaused(false);
        CursorPolicyService.ForceGameplayCursor();

        BaseSceneBinding binding = ResolveBinding<BaseSceneBinding>(scene);
        if (binding.BaseSceneUiController == null)
        {
            throw new InvalidOperationException("BaseSceneBinding.BaseSceneUiController is required.");
        }

        binding.BaseSceneUiController.ConfigureRuntimeServices(
            runtimeShell.GameFlowOrchestrator,
            runtimeShell.RunCatalog,
            runtimeShell.ProgressService
        );

        if (binding.PlayerSpawnPoint == null || binding.InteractionRoot == null)
        {
            throw new InvalidOperationException("BaseSceneBinding requires PlayerSpawnPoint and InteractionRoot.");
        }

        PlayerController persistentPlayer = runtimeShell.PersistentRoot.PlayerStats.GetComponent<PlayerController>();
        if (persistentPlayer == null)
        {
            throw new InvalidOperationException("Persistent player is missing PlayerController.");
        }

        Transform playerTransform = persistentPlayer.transform;
        Transform spawn = binding.PlayerSpawnPoint;
        persistentPlayer.TeleportTo(spawn.position, spawn.rotation);
        ConfigureBillboardLabels(binding, runtimeShell);

        BaseInteractionController interactionController = playerTransform.GetComponent<BaseInteractionController>();
        if (interactionController == null)
        {
            interactionController = playerTransform.gameObject.AddComponent<BaseInteractionController>();
        }

        GameInput playerInput = persistentPlayer.GameInput;
        if (playerInput == null)
        {
            throw new InvalidOperationException("Persistent player is missing GameInput.");
        }

        interactionController.ConfigureRuntime(playerInput, binding.BaseSceneUiController, binding.InteractionRoot);
        interactionController.enabled = true;
        activeBaseInteractionController = interactionController;

        if (runtimeShell.PersistentRoot.PlayerCombat != null)
        {
            runtimeShell.PersistentRoot.PlayerCombat.ForceSetAimState(false);
            runtimeShell.PersistentRoot.PlayerCombat.enabled = false;
        }
    }

    public static void ExitBaseMode(RuntimeShell runtimeShell)
    {
        if (runtimeShell != null && runtimeShell.PersistentRoot != null && runtimeShell.PersistentRoot.PlayerCombat != null)
        {
            runtimeShell.PersistentRoot.PlayerCombat.ForceSetAimState(false);
            runtimeShell.PersistentRoot.PlayerCombat.enabled = true;
        }

        if (activeBaseInteractionController != null)
        {
            activeBaseInteractionController.enabled = false;
            activeBaseInteractionController = null;
        }
    }

    private static T ResolveBinding<T>(Scene scene) where T : class
    {
        UnityEngine.GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            T binding = roots[i].GetComponentInChildren(typeof(T), true) as T;
            if (binding != null)
            {
                return binding;
            }
        }

        throw new InvalidOperationException($"Required scene binding '{typeof(T).Name}' not found in scene '{scene.name}'.");
    }

    private static void ConfigureBillboardLabels(BaseSceneBinding binding, RuntimeShell runtimeShell)
    {
        PlayerCameraAimController cameraController = null;
        if (runtimeShell != null &&
            runtimeShell.PersistentRoot != null &&
            runtimeShell.PersistentRoot.BattleSharedRoot != null)
        {
            cameraController =
                runtimeShell.PersistentRoot.BattleSharedRoot.GetComponentInChildren<PlayerCameraAimController>(true);
        }

        Camera cameraRef = cameraController != null ? cameraController.MainCamera : Camera.main;
        WorldBillboardLabel[] labels = binding.GetComponentsInChildren<WorldBillboardLabel>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            labels[i].SetCamera(cameraRef);
        }
    }
}
