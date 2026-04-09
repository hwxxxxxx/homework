using System;
using System.Collections;
using UnityEngine;

public class GameFlowOrchestrator : MonoBehaviour
{
    [SerializeField] private RuntimeShell runtimeShell;

    private bool isTransitioning;
    private string pendingRunId;

    private SceneCatalogAsset SceneCatalog => runtimeShell.SceneCatalog;
    private RunCatalogAsset RunCatalog => runtimeShell.RunCatalog;

    private readonly struct TransitionPolicy
    {
        public TransitionPolicy(bool enterDeactivatingPhase, bool deactivateGameplayScope)
        {
            EnterDeactivatingPhase = enterDeactivatingPhase;
            DeactivateGameplayScope = deactivateGameplayScope;
        }

        public bool EnterDeactivatingPhase { get; }
        public bool DeactivateGameplayScope { get; }
    }

    private void Awake()
    {
        if (runtimeShell == null)
        {
            throw new InvalidOperationException("GameFlowOrchestrator requires RuntimeShell reference.");
        }
    }

    public void EnterMainMenu()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(EnterMainMenuRoutine());
    }

    public void EnterMainMenuFromBoot()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(EnterMainMenuFromBootRoutine());
    }

    public void EnterBase()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(EnterBaseRoutine());
    }

    public void EnterRun(string levelId)
    {
        if (isTransitioning)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(levelId))
        {
            throw new ArgumentException("Run levelId cannot be null or empty.", nameof(levelId));
        }

        StartCoroutine(EnterRunRoutine(levelId));
    }

    public void ReturnToBase()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(ReturnToBaseRoutine());
    }

    public void ReturnToMainMenu()
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(ReturnToMainMenuRoutine());
    }

    private IEnumerator EnterMainMenuRoutine()
    {
        isTransitioning = true;
        SceneCatalogAsset.SceneEntry targetEntry = ResolveMainMenuEntry();
        TransitionPolicy policy = ResolveTransitionPolicy(targetEntry.domain);
        ApplyPreExit(policy);
        EnterDeactivatingPhaseIfNeeded(policy);

        EnsureStateTransition(GameStateId.MainMenu);
        yield return runtimeShell.SceneTransitionOrchestrator.TransitionToContentScene(
            targetEntry.sceneName,
            FlowConfigProvider.Config.LoadingDefaultMessage
        );

        var targetScene = runtimeShell.SceneTransitionOrchestrator.GetCurrentContentSceneOrThrow();
        ShellSceneInitializer.InitializeMainMenu(targetScene, runtimeShell);
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Initializing);
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Active);
        PostEnter();
    }

    private IEnumerator EnterMainMenuFromBootRoutine()
    {
        isTransitioning = true;
        SceneCatalogAsset.SceneEntry targetEntry = ResolveMainMenuEntry();
        TransitionPolicy policy = ResolveTransitionPolicy(targetEntry.domain);
        ApplyPreExit(policy);
        EnterDeactivatingPhaseIfNeeded(policy);

        yield return runtimeShell.SceneTransitionOrchestrator.TransitionToContentScene(
            targetEntry.sceneName,
            FlowConfigProvider.Config.LoadingDefaultMessage
        );

        var targetScene = runtimeShell.SceneTransitionOrchestrator.GetCurrentContentSceneOrThrow();
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Initializing);
        ShellSceneInitializer.InitializeMainMenu(targetScene, runtimeShell);
        EnsureStateTransition(GameStateId.MainMenu);
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Active);
        PostEnter();
    }

    private IEnumerator EnterBaseRoutine()
    {
        isTransitioning = true;
        SceneCatalogAsset.SceneEntry targetEntry = ResolveBaseEntry();
        TransitionPolicy policy = ResolveTransitionPolicy(targetEntry.domain);
        ApplyPreExit(policy);
        EnterDeactivatingPhaseIfNeeded(policy);

        EnsureStateTransition(GameStateId.Base);
        yield return runtimeShell.SceneTransitionOrchestrator.TransitionToContentScene(
            targetEntry.sceneName,
            FlowConfigProvider.Config.EnteringBaseMessage
        );

        var targetScene = runtimeShell.SceneTransitionOrchestrator.GetCurrentContentSceneOrThrow();
        ShellSceneInitializer.InitializeBase(targetScene, runtimeShell);
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Initializing);
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Active);
        PostEnter();
    }

    private IEnumerator EnterRunRoutine(string levelId)
    {
        RunCatalogAsset runCatalog = RunCatalog;
        if (!runCatalog.TryGetRun(levelId, out RunCatalogAsset.RunEntry runEntry))
        {
            throw new InvalidOperationException($"RunCatalog missing run id '{levelId}'.");
        }

        SceneCatalogAsset.SceneEntry targetEntry = ResolveSceneEntry(runEntry.SceneId);
        if (targetEntry.domain != RuntimeDomain.Gameplay)
        {
            throw new InvalidOperationException($"Run '{levelId}' maps to non-gameplay domain '{targetEntry.domain}'.");
        }

        isTransitioning = true;
        pendingRunId = levelId;
        TransitionPolicy policy = ResolveTransitionPolicy(targetEntry.domain);
        ApplyPreExit(policy);
        EnterDeactivatingPhaseIfNeeded(policy);

        EnsureStateTransition(GameStateId.LoadingRun);
        yield return runtimeShell.SceneTransitionOrchestrator.TransitionToContentScene(
            targetEntry.sceneName,
            FlowConfigProvider.Config.PreparingRunMessage
        );

        var targetScene = runtimeShell.SceneTransitionOrchestrator.GetCurrentContentSceneOrThrow();
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Initializing);
        yield return GameplaySceneInitializer.InitializeGameplay(targetScene, runtimeShell, pendingRunId);
        EnsureStateTransition(GameStateId.InRun);
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Active);
        PostEnter();
    }

    private IEnumerator ReturnToBaseRoutine()
    {
        isTransitioning = true;
        SceneCatalogAsset.SceneEntry targetEntry = ResolveBaseEntry();
        TransitionPolicy policy = ResolveTransitionPolicy(targetEntry.domain);

        if (runtimeShell.GameStateService.CurrentState == GameStateId.InRun &&
            !runtimeShell.RunContextService.LastRunWon.HasValue)
        {
            runtimeShell.RunContextService.EndRun(false);
        }

        ApplyPreExit(policy);
        EnterDeactivatingPhaseIfNeeded(policy);

        EnsureStateTransition(GameStateId.LoadingBase);
        yield return runtimeShell.SceneTransitionOrchestrator.TransitionToContentScene(
            targetEntry.sceneName,
            FlowConfigProvider.Config.ReturnToBaseMessage
        );

        var targetScene = runtimeShell.SceneTransitionOrchestrator.GetCurrentContentSceneOrThrow();
        ShellSceneInitializer.InitializeBase(targetScene, runtimeShell);
        EnsureStateTransition(GameStateId.Base);
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Initializing);
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Active);
        PostEnter();
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        isTransitioning = true;
        SceneCatalogAsset.SceneEntry targetEntry = ResolveMainMenuEntry();
        TransitionPolicy policy = ResolveTransitionPolicy(targetEntry.domain);

        if (runtimeShell.GameStateService.CurrentState == GameStateId.InRun &&
            !runtimeShell.RunContextService.LastRunWon.HasValue)
        {
            runtimeShell.RunContextService.EndRun(false);
        }

        ApplyPreExit(policy);
        EnterDeactivatingPhaseIfNeeded(policy);

        EnsureStateTransition(GameStateId.MainMenu);
        yield return runtimeShell.SceneTransitionOrchestrator.TransitionToContentScene(
            targetEntry.sceneName,
            FlowConfigProvider.Config.ReturnToMainMenuMessage
        );

        var targetScene = runtimeShell.SceneTransitionOrchestrator.GetCurrentContentSceneOrThrow();
        ShellSceneInitializer.InitializeMainMenu(targetScene, runtimeShell);
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Initializing);
        runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Active);
        PostEnter();
    }

    private void ApplyPreExit(TransitionPolicy policy)
    {
        PoolService.ClearAllPools();
        if (policy.DeactivateGameplayScope)
        {
            GameplaySceneInitializer.DeactivateGameplayScope(runtimeShell);
        }

        runtimeShell.PauseController.SetPaused(false);
    }

    private void EnterDeactivatingPhaseIfNeeded(TransitionPolicy policy)
    {
        if (policy.EnterDeactivatingPhase)
        {
            runtimeShell.PhaseCoordinator.EnterPhase(RuntimePhase.Deactivating);
        }
    }

    private void PostEnter()
    {
        runtimeShell.AchievementService.ConfigureProgressService(runtimeShell.ProgressService);
        isTransitioning = false;
        pendingRunId = null;
        LoadingScreenService.EndTransition();
    }

    private SceneCatalogAsset.SceneEntry ResolveMainMenuEntry()
    {
        string sceneId = FlowConfigProvider.Config.MainMenuSceneId;
        if (string.IsNullOrWhiteSpace(sceneId))
        {
            throw new InvalidOperationException("FlowConfig.MainMenuSceneId is required.");
        }

        return ResolveSceneEntry(sceneId);
    }

    private SceneCatalogAsset.SceneEntry ResolveBaseEntry()
    {
        string sceneId = FlowConfigProvider.Config.BaseSceneId;
        if (string.IsNullOrWhiteSpace(sceneId))
        {
            throw new InvalidOperationException("FlowConfig.BaseSceneId is required.");
        }

        return ResolveSceneEntry(sceneId);
    }

    private SceneCatalogAsset.SceneEntry ResolveSceneEntry(string sceneId)
    {
        if (SceneCatalog.TryGetScene(sceneId, out SceneCatalogAsset.SceneEntry sceneEntry))
        {
            return sceneEntry;
        }

        throw new InvalidOperationException($"SceneCatalog missing required scene id '{sceneId}'.");
    }

    private TransitionPolicy ResolveTransitionPolicy(RuntimeDomain targetDomain)
    {
        RuntimeDomain sourceDomain = ResolveCurrentDomain();
        if (sourceDomain == RuntimeDomain.Bootstrap && targetDomain == RuntimeDomain.Shell)
        {
            return new TransitionPolicy(false, false);
        }

        if (sourceDomain == RuntimeDomain.Shell && targetDomain == RuntimeDomain.Shell)
        {
            return new TransitionPolicy(true, false);
        }

        if (sourceDomain == RuntimeDomain.Shell && targetDomain == RuntimeDomain.Gameplay)
        {
            return new TransitionPolicy(true, false);
        }

        if (sourceDomain == RuntimeDomain.Gameplay && targetDomain == RuntimeDomain.Shell)
        {
            return new TransitionPolicy(true, true);
        }

        if (sourceDomain == RuntimeDomain.Gameplay && targetDomain == RuntimeDomain.Gameplay)
        {
            return new TransitionPolicy(true, true);
        }

        throw new InvalidOperationException($"No transition policy from domain '{sourceDomain}' to '{targetDomain}'.");
    }

    private RuntimeDomain ResolveCurrentDomain()
    {
        switch (runtimeShell.GameStateService.CurrentState)
        {
            case GameStateId.Boot:
                return RuntimeDomain.Bootstrap;
            case GameStateId.MainMenu:
            case GameStateId.Base:
                return RuntimeDomain.Shell;
            case GameStateId.LoadingRun:
            case GameStateId.InRun:
            case GameStateId.RunResult:
            case GameStateId.LoadingBase:
                return RuntimeDomain.Gameplay;
            default:
                throw new InvalidOperationException($"Unsupported game state '{runtimeShell.GameStateService.CurrentState}'.");
        }
    }

    private void EnsureStateTransition(GameStateId targetState)
    {
        if (!runtimeShell.GameStateService.TrySetState(targetState))
        {
            throw new InvalidOperationException($"Illegal game state transition to '{targetState}'.");
        }
    }
}
