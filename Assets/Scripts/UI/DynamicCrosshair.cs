using UnityEngine;

public class DynamicCrosshair : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform crosshairRect;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private Camera mainCamera;

    [Header("Smoothing")]
    [SerializeField] private float followSmoothTime = 0.03f;

    private Vector2 crosshairVelocity;
    private IWeaponAimPointProvider currentAimPointProvider;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (playerCombat == null)
        {
            Debug.LogWarning("DynamicCrosshair: missing PlayerCombat reference.", this);
            return;
        }

        playerCombat.OnCurrentWeaponChanged += HandleCurrentWeaponChanged;
        HandleCurrentWeaponChanged(playerCombat.GetCurrentWeapon());
    }

    private void OnDestroy()
    {
        if (playerCombat != null)
        {
            playerCombat.OnCurrentWeaponChanged -= HandleCurrentWeaponChanged;
        }
    }

    private void LateUpdate()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (crosshairRect == null || canvasRect == null || currentAimPointProvider == null || mainCamera == null)
        {
            return;
        }

        if (!currentAimPointProvider.TryGetShotHitPoint(out Vector3 worldHitPoint))
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

    private void HandleCurrentWeaponChanged(WeaponBase weapon)
    {
        currentAimPointProvider = weapon as IWeaponAimPointProvider;
    }
}
