using UnityEngine;

public class WorldBillboardLabel : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    public void SetCamera(Camera cameraRef)
    {
        targetCamera = cameraRef;
    }

    private void LateUpdate()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null)
        {
            return;
        }

        Vector3 toCamera = transform.position - cam.transform.position;
        if (toCamera.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(toCamera, Vector3.up);
    }
}
