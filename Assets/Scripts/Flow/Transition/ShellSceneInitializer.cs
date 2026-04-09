using System;
using UnityEngine.SceneManagement;

public static class ShellSceneInitializer
{
    public static void InitializeMainMenu(Scene scene, RuntimeShell runtimeShell)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            throw new InvalidOperationException("MainMenu scene is not loaded.");
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
            runtimeShell.LevelUnlockService,
            runtimeShell.ProgressService
        );
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
}
