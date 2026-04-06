using UnityEngine;

public class LevelUnlockService : MonoBehaviour
{
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private ProgressService progressService;

    private void Awake()
    {
        if (gameStateService == null)
        {
            gameStateService = FindObjectOfType<GameStateMachineService>(true);
        }

        if (progressService == null)
        {
            progressService = FindObjectOfType<ProgressService>(true);
        }
    }

    public bool TryUnlock(LevelDefinitionAsset levelDefinition)
    {
        if (gameStateService == null || progressService == null || levelDefinition == null)
        {
            return false;
        }

        if (gameStateService.CurrentState != GameStateId.Base)
        {
            return false;
        }

        return progressService.TryUnlockLevelWithCost(levelDefinition);
    }
}
