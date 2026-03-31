using UnityEngine;

public class HitscanWeapon : WeaponBase, IWeaponAimPointProvider
{
    [Header("Ballistic Settings")]
    [SerializeField] private float range = 100f;
    [SerializeField] private float projectileSpeed = 220f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform hipFireForwardReference;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private string ignoreTag = "Player";
    [SerializeField] private ProjectileBullet projectilePrefab;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    [SerializeField] private ParticleSystem impactEffectPrefab;

    [Header("Spread (Degrees)")]
    [SerializeField] private float aimSpreadAngle = 0.1f;
    [SerializeField] private float hipFireSpreadAngle = 2.4f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;

    public override void BindOwner(PlayerCombat owner)
    {
        base.BindOwner(owner);
    }

    private void Start()
    {
        if (firePoint == null || mainCamera == null)
        {
            Debug.LogError($"{name} missing firePoint/mainCamera reference.", this);
            enabled = false;
            return;
        }

        PrepareParticleTemplate(muzzleFlashPrefab);
        PrepareParticleTemplate(impactEffectPrefab);
    }

    protected override void Fire()
    {
        if (!TryGetShotData(out Ray fireRay, out RaycastHit hit, out bool hasHit))
        {
            return;
        }

        if (muzzleFlashPrefab != null && firePoint != null)
        {
            SpawnPooledEffect(
                muzzleFlashPrefab,
                firePoint.position,
                Quaternion.LookRotation(fireRay.direction),
                1.2f
            );
        }

        if (projectilePrefab != null && firePoint != null)
        {
            ProjectileBullet bullet = PoolService.Spawn(
                projectilePrefab,
                firePoint.position,
                Quaternion.LookRotation(fireRay.direction)
            );
            bullet.Initialize(
                fireRay.direction,
                projectileSpeed,
                range,
                GetDamageValue(),
                hitMask,
                ignoreTag,
                impactEffectPrefab
            );
            return;
        }

        if (hasHit)
        {
            ApplyDamage(hit);
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

    private void ApplyDamage(RaycastHit hit)
    {
        if (TryGetDamageableFromCollider(hit.collider, out IDamageable damageable))
        {
            damageable.TakeDamage(GetDamageValue());
        }

        SpawnPooledEffect(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal), 2f);
    }

    private bool TryGetShotData(out Ray fireRay, out RaycastHit hit, out bool hasHit)
    {
        fireRay = default;
        hit = default;
        hasHit = false;

        if (firePoint == null)
        {
            Debug.LogWarning($"{name} missing firePoint reference.");
            return false;
        }

        Vector3 fireDirection = GetFireDirection();
        fireRay = new Ray(firePoint.position, fireDirection);

        if (drawDebugRay)
        {
            Debug.DrawRay(firePoint.position, fireDirection * range, Color.red, 1f);
        }

        hasHit = Physics.Raycast(fireRay, out hit, range, hitMask);
        return true;
    }

    private Vector3 GetFireDirection()
    {
        bool isAiming = ownerCombat != null && ownerCombat.IsAiming;
        float spread = isAiming ? aimSpreadAngle : hipFireSpreadAngle;
        Vector3 baseDirection = GetBaseDirection(isAiming);
        return ApplySpread(baseDirection, spread);
    }

    private Vector3 GetBaseDirection(bool isAiming)
    {
        if (isAiming && mainCamera != null)
        {
            Vector3 targetPoint = GetCameraAimPoint();
            return (targetPoint - firePoint.position).normalized;
        }

        return firePoint.forward;
    }

    private Vector3 ApplySpread(Vector3 direction, float spreadAngle)
    {
        if (spreadAngle <= 0.001f)
        {
            return direction.normalized;
        }

        Vector2 spread = Random.insideUnitCircle * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);
        Vector3 spreadDir = direction + transform.right * spread.x + transform.up * spread.y;
        return spreadDir.normalized;
    }

    private Vector3 GetCameraAimPoint()
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

    private void Reset()
    {
        weaponDisplayName = "Rifle";
        weaponKind = WeaponKind.Rifle;
        damage = 10;
        fireRate = 8f;
        magazineSize = 30;
        reserveAmmo = 120;
        reloadTime = 1.4f;
        range = 120f;
        projectileSpeed = 260f;
        aimSpreadAngle = 0.08f;
        hipFireSpreadAngle = 2.2f;
    }
}
