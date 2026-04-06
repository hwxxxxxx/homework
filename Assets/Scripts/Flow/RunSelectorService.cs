using UnityEngine;
using UnityEngine.SceneManagement;

public class RunSelectorService : MonoBehaviour
{
    [SerializeField] private GameStateMachineService gameStateService;
    [SerializeField] private RunContextService runContextService;
    [SerializeField] private ProgressService progressService;
    [SerializeField] private string gameplayCommonSceneName = "GameplayCommon";

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

        string sceneName = runContextService.SelectedLevel.SceneName;
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        RunSceneRequest.SetPendingLevelScene(sceneName);
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplayCommonSceneName, LoadSceneMode.Single);
        return true;
    }
}
