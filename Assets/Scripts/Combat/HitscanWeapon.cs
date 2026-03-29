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
        if (!TryGetShotData(out Ray fireRay, out RaycastHit hit, out bool hasHit))
        {
            return;
        }

        if (hasHit)
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

    public bool TryGetShotHitPoint(out Vector3 hitPoint)
    {
        if (!TryGetShotData(out Ray fireRay, out RaycastHit hit, out bool hasHit))
        {
            hitPoint = Vector3.zero;
            return false;
        }

        hitPoint = hasHit ? hit.point : fireRay.origin + fireRay.direction * range;
        return true;
    }

    private bool TryGetShotData(out Ray fireRay, out RaycastHit hit, out bool hasHit)
    {
        fireRay = default;
        hit = default;
        hasHit = false;

        if (mainCamera == null || firePoint == null)
        {
            Debug.LogWarning("HitscanWeapon is missing mainCamera or firePoint reference.");
            return false;
        }

        Vector3 targetPoint = GetAimPoint();
        Vector3 fireDirection = (targetPoint - firePoint.position).normalized;
        fireRay = new Ray(firePoint.position, fireDirection);

        if (drawDebugRay)
        {
            Debug.DrawRay(firePoint.position, fireDirection * range, Color.red, 1f);
        }

        hasHit = Physics.Raycast(fireRay, out hit, range, hitMask);
        return true;
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
