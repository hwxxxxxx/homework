using UnityEngine;

public class LevelSceneBinding : MonoBehaviour
{
    [SerializeField] private EnemyDeathDropListenerBase[] enemyDropListeners;
    [SerializeField] private LevelEncounterController encounterController;
    [SerializeField] private Transform playerSpawnPoint;

    public EnemyDeathDropListenerBase[] EnemyDropListeners => enemyDropListeners;
    public Transform PlayerSpawnPoint => playerSpawnPoint;

    public void StartEncounter(RuntimeShell shell, string runId)
    {
        if (encounterController == null)
        {
            Debug.LogError($"LevelSceneBinding on '{name}' is missing encounterController.");
            return;
        }

        encounterController.StartEncounter(shell);
    }

    public void StopEncounter()
    {
        if (encounterController == null)
        {
            return;
        }

        encounterController.StopEncounter();
    }
}
