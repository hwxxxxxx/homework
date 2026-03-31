using UnityEngine;

public class RunSelectorService : MonoBehaviour
{
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private RunContextService runContextService;
    [SerializeField] private ProgressService progressService;

    public bool TrySelectLevel(LevelDefinitionAsset levelDefinition)
    {
        if (levelDefinition == null || progressService == null)
        {
            return false;
        }

        if (!progressService.IsLevelUnlocked(levelDefinition.LevelId))
        {
            return false;
        }

        if (runContextService != null)
        {
            runContextService.SetSelectedLevel(levelDefinition);
        }

        return true;
    }

    public bool TryStartSelectedRun()
    {
        if (gameStateService == null || runContextService == null || runContextService.SelectedLevel == null)
        {
            return false;
        }

        GameStateId current = gameStateService.CurrentState;
        if (current != GameStateId.RunSelect && current != GameStateId.Base)
        {
            return false;
        }

        if (current == GameStateId.Base && !gameStateService.TrySetState(GameStateId.RunSelect))
        {
            return false;
        }

        if (gameStateService.CurrentState != GameStateId.RunSelect || !gameStateService.TrySetState(GameStateId.LoadingRun))
        {
            return false;
        }

        return gameStateService.TrySetState(GameStateId.InRun);
    }
}
