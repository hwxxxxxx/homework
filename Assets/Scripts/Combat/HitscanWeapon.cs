using UnityEngine;

public class HitscanWeapon : WeaponBase
{
    [Header("Hitscan Settings")]
    [SerializeField] private float range = 100f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;

    protected override void Fire()
    {
        if (mainCamera == null || firePoint == null)
        {
            Debug.LogWarning("HitscanWeapon is missing mainCamera or firePoint reference.");
            return;
        }

        Vector3 targetPoint = GetAimPoint();
        Vector3 fireDirection = (targetPoint - firePoint.position).normalized;

        Ray fireRay = new Ray(firePoint.position, fireDirection);

        if (drawDebugRay)
        {
            Debug.DrawRay(firePoint.position, fireDirection * range, Color.red, 1f);
        }

        if (Physics.Raycast(fireRay, out RaycastHit hit, range, hitMask))
        {
            Debug.Log("Hit: " + hit.collider.name);

            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = hit.collider.GetComponentInParent<IDamageable>();
            }
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    private Vector3 GetAimPoint()
    {
        Ray cameraRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (drawDebugRay)
        {
            Debug.DrawRay(cameraRay.origin, cameraRay.direction * range, Color.green, 1f);
        }

        if (Physics.Raycast(cameraRay, out RaycastHit hit, range, hitMask))
        {
            return hit.point;
        }

        return cameraRay.origin + cameraRay.direction * range;
    }
}
