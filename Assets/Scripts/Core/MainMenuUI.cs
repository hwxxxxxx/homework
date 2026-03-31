using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameStateMachineService gameStateService;

    public void OnStartGame()
    {
        if (gameStateService != null)
        {
            if (gameStateService.CurrentState == GameStateId.Boot)
            {
                gameStateService.TrySetState(GameStateId.Base);
            }

            if (gameStateService.CurrentState == GameStateId.Base)
            {
                gameStateService.TrySetState(GameStateId.RunSelect);
            }

            return;
        }

        Debug.LogError("MainMenuUI: missing GameStateMachineService reference.", this);
    }

    public void OnQuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
