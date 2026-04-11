using UnityEngine;

public class BaseSceneBinding : MonoBehaviour
{
    [SerializeField] private BaseSceneUIController baseSceneUiController;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform interactionRoot;

    public BaseSceneUIController BaseSceneUiController => baseSceneUiController;
    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public Transform InteractionRoot => interactionRoot;
}
