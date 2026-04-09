using UnityEngine;

public class LevelUnlockService : MonoBehaviour
{
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private ProgressService progressService;

    public bool TryUnlock(RunCatalogAsset.RunEntry runEntry)
    {
        if (runEntry == null)
        {
            return false;
        }

        if (gameStateService.CurrentState != GameStateId.Base)
        {
            return false;
        }

        return progressService.TryUnlockLevelWithCost(runEntry);
    }
}
