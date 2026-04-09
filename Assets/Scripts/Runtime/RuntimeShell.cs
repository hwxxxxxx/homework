using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class RuntimeShell : MonoBehaviour
{
    private static RuntimeShell instance;

    [Header("Core")]
    [SerializeField] private PersistentRuntimeRoot persistentRoot;
    [SerializeField] private GamePauseController pauseController;
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private ProgressService progressService;
    [SerializeField] private LevelUnlockService levelUnlockService;
    [SerializeField] private AchievementService achievementService;
    [SerializeField] private RunContextService runContextService;
    [SerializeField] private RuntimePhaseCoordinator phaseCoordinator;
    [SerializeField] private SceneTransitionOrchestrator sceneTransitionOrchestrator;
    [SerializeField] private GameFlowOrchestrator gameFlowOrchestrator;

    [Header("Catalogs")]
    [SerializeField] private SceneCatalogAsset sceneCatalog;
    [SerializeField] private RunCatalogAsset runCatalog;

    [Header("UI")]
    [SerializeField] private EventSystem globalEventSystem;
    [SerializeField] private PlayerHUD playerHud;
    [SerializeField] private AudioListener globalAudioListener;

    public static RuntimeShell Instance => instance;
    public PersistentRuntimeRoot PersistentRoot => persistentRoot;
    public GamePauseController PauseController => pauseController;
    public GameStateMachineService GameStateService => gameStateService;
    public ProgressService ProgressService => progressService;
    public LevelUnlockService LevelUnlockService => levelUnlockService;
    public AchievementService AchievementService => achievementService;
    public RunContextService RunContextService => runContextService;
    public RuntimePhaseCoordinator PhaseCoordinator => phaseCoordinator;
    public SceneTransitionOrchestrator SceneTransitionOrchestrator => sceneTransitionOrchestrator;
    public GameFlowOrchestrator GameFlowOrchestrator => gameFlowOrchestrator;
    public SceneCatalogAsset SceneCatalog => sceneCatalog;
    public RunCatalogAsset RunCatalog => runCatalog;
    public EventSystem GlobalEventSystem => globalEventSystem;
    public PlayerHUD PlayerHud => playerHud;
    public AudioListener GlobalAudioListener => globalAudioListener;

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        ValidateRequiredReferences();
        DontDestroyOnLoad(persistentRoot.gameObject);
        pauseController.enabled = false;
        PoolService.ConfigurePersistentRoot(persistentRoot.transform);

        ServiceRegistry.Register(this);
        ServiceRegistry.Register(gameStateService);
        ServiceRegistry.Register(progressService);
        ServiceRegistry.Register(levelUnlockService);
        ServiceRegistry.Register(achievementService);
        ServiceRegistry.Register(runContextService);
        ServiceRegistry.Register(phaseCoordinator);
        ServiceRegistry.Register(sceneTransitionOrchestrator);
        ServiceRegistry.Register(gameFlowOrchestrator);
    }

    private void ValidateRequiredReferences()
    {
        if (persistentRoot == null ||
            pauseController == null ||
            gameStateService == null ||
            progressService == null ||
            levelUnlockService == null ||
            achievementService == null ||
            runContextService == null ||
            phaseCoordinator == null ||
            sceneTransitionOrchestrator == null ||
            gameFlowOrchestrator == null ||
            sceneCatalog == null ||
            runCatalog == null ||
            globalEventSystem == null ||
            playerHud == null ||
            globalAudioListener == null)
        {
            throw new InvalidOperationException("RuntimeShell has unassigned required serialized references.");
        }
    }
}
