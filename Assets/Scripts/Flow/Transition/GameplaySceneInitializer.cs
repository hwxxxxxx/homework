using System;
using System.Collections;
using UnityEngine.SceneManagement;

public static class GameplaySceneInitializer
{
    private static LevelSceneBinding activeLevelBinding;

    public static IEnumerator InitializeGameplay(Scene scene, RuntimeShell runtimeShell, string runId)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            throw new InvalidOperationException("Gameplay scene is not loaded.");
        }

        LevelSceneBinding binding = ResolveBinding<LevelSceneBinding>(scene);
        PersistentRuntimeRoot persistentRoot = runtimeShell.PersistentRoot;
        persistentRoot.BattleSharedRoot.SetActive(true);

        if (persistentRoot.PlayerCombat == null ||
            persistentRoot.PlayerStats == null ||
            persistentRoot.PlayerSkillSystem == null ||
            persistentRoot.PlayerEffectController == null)
        {
            throw new InvalidOperationException("PersistentRuntimeRoot has unassigned player runtime references.");
        }

        while (!runtimeShell.PlayerHud.EnsurePresentationReady())
        {
            yield return null;
        }

        runtimeShell.PlayerHud.ConfigureSceneReferences(
            persistentRoot.PlayerCombat,
            persistentRoot.PlayerStats,
            persistentRoot.PlayerSkillSystem,
            persistentRoot.PlayerEffectController
        );

        if (binding.EnemyDropListeners == null)
        {
            throw new InvalidOperationException($"LevelSceneBinding in scene '{scene.name}' has null EnemyDropListeners.");
        }

        EnemyDeathDropListenerBase[] dropListeners = binding.EnemyDropListeners;
        for (int i = 0; i < dropListeners.Length; i++)
        {
            if (dropListeners[i] == null)
            {
                throw new InvalidOperationException($"LevelSceneBinding in scene '{scene.name}' has null EnemyDropListeners element at index {i}.");
            }

            dropListeners[i].SetRunContextService(runtimeShell.RunContextService);
        }

        runtimeShell.PlayerHud.SetGameplayActive(true);
        runtimeShell.PauseController.SetPaused(false);
        CursorPolicyService.ForceGameplayCursor();
        runtimeShell.RunContextService.BeginRun(runId);
        activeLevelBinding = binding;
        binding.StartEncounter(runtimeShell, runId);
        yield return null;
    }

    public static void DeactivateGameplayScope(RuntimeShell runtimeShell)
    {
        if (activeLevelBinding != null)
        {
            activeLevelBinding.StopEncounter();
            activeLevelBinding = null;
        }

        runtimeShell.PlayerHud.SetGameplayActive(false);
        runtimeShell.PersistentRoot.BattleSharedRoot.SetActive(false);
        runtimeShell.RunContextService.ResetRunState();
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
