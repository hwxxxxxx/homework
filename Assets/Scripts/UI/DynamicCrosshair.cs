using UnityEngine;

public class DynamicCrosshair : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform crosshairRect;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private HitscanWeapon hitscanWeapon;
    [SerializeField] private Camera mainCamera;

    [Header("Smoothing")]
    [SerializeField] private float followSmoothTime = 0.03f;

    private Vector2 crosshairVelocity;

    private void LateUpdate()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (crosshairRect == null || canvasRect == null || hitscanWeapon == null || mainCamera == null)
        {
            return;
        }

        if (!hitscanWeapon.TryGetShotHitPoint(out Vector3 worldHitPoint))
        {
            return;
        }

        Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldHitPoint);
        if (screenPoint.z <= 0f)
        {
            return;
        }

        Camera uiCamera = null;
        Canvas canvas = canvasRect.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                uiCamera,
                out Vector2 localPoint
            ))
        {
            return;
        }

        crosshairRect.anchoredPosition = Vector2.SmoothDamp(
            crosshairRect.anchoredPosition,
            localPoint,
            ref crosshairVelocity,
            followSmoothTime
        );
    }
}
