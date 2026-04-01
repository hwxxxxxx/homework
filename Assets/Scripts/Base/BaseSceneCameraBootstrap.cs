using Cinemachine;
using UnityEngine;

public class BaseSceneCameraBootstrap : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineVirtualCamera normalCamera;
    [SerializeField] private PlayerCameraAimController cameraController;
    [SerializeField] private string collisionIgnoreTag = "Player";

    private void Awake()
    {
        GamePauseController.EnsureBaseControllerExists();

        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        SetupBrain();
        SetupNormalCamera();
        cameraController.ConfigureBaseMode(gameInput, cameraRoot, normalCamera);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private bool ValidateReferences()
    {
        if (gameInput != null && cameraRoot != null && mainCamera != null && normalCamera != null && cameraController != null)
        {
            return true;
        }

        Debug.LogError("BaseSceneCameraBootstrap references are not fully assigned.", this);
        return false;
    }

    private void SetupBrain()
    {
        CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            Debug.LogError("Main Camera is missing CinemachineBrain.", mainCamera);
            enabled = false;
            return;
        }
    }

    private void SetupNormalCamera()
    {
        normalCamera.Priority = 30;
        normalCamera.Follow = cameraRoot;
        normalCamera.LookAt = cameraRoot;
        normalCamera.m_Lens.FieldOfView = 60f;

        Cinemachine3rdPersonFollow follow = normalCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (follow == null)
        {
            Debug.LogError("NormalCamera is missing Cinemachine3rdPersonFollow.", normalCamera);
            enabled = false;
            return;
        }

        follow.ShoulderOffset = new Vector3(0f, 0.35f, 0f);
        follow.VerticalArmLength = 0.55f;
        follow.CameraDistance = 4.2f;
        follow.Damping = new Vector3(0.18f, 0.18f, 0.18f);
        follow.CameraRadius = 0.22f;
        follow.DampingIntoCollision = 0.12f;
        follow.DampingFromCollision = 0.18f;
        follow.IgnoreTag = collisionIgnoreTag;
    }
}
