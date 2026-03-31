using UnityEngine;

public class ShotgunWeapon : WeaponBase, IWeaponAimPointProvider
{
    [Header("Shotgun Settings")]
    [SerializeField] private int pelletCount = 8;
    [SerializeField] private float range = 40f;
    [SerializeField] private float projectileSpeed = 120f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform hipFireForwardReference;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private string ignoreTag = "Player";
    [SerializeField] private ProjectileBullet projectilePrefab;
    [SerializeField] private ParticleSystem muzzleFlashPrefab;
    [SerializeField] private ParticleSystem impactEffectPrefab;

    [Header("Spread (Degrees)")]
    [SerializeField] private float aimSpreadAngle = 3f;
    [SerializeField] private float hipFireSpreadAngle = 8f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay;

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
        if (firePoint == null)
        {
            Debug.LogWarning($"{name} missing firePoint reference.");
            return;
        }

        bool isAiming = ownerCombat != null && ownerCombat.IsAiming;
        Vector3 baseDirection = GetBaseDirection(isAiming);
        float spread = isAiming ? aimSpreadAngle : hipFireSpreadAngle;

        if (muzzleFlashPrefab != null)
        {
            SpawnPooledEffect(
                muzzleFlashPrefab,
                firePoint.position,
                Quaternion.LookRotation(baseDirection),
                1.2f
            );
        }

        for (int i = 0; i < Mathf.Max(1, pelletCount); i++)
        {
            Vector3 direction = ApplySpread(baseDirection, spread);
            if (projectilePrefab != null)
            {
                ProjectileBullet bullet = PoolService.Spawn(
                    projectilePrefab,
                    firePoint.position,
                    Quaternion.LookRotation(direction)
                );
                bullet.Initialize(
                    direction,
                    projectileSpeed,
                    range,
                    GetDamageValue(),
                    hitMask,
                    ignoreTag,
                    impactEffectPrefab
                );
                continue;
            }

            Ray ray = new Ray(firePoint.position, direction);
            if (drawDebugRay)
            {
                Debug.DrawRay(firePoint.position, direction * range, Color.yellow, 0.8f);
            }

            if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
            {
                ApplyDamage(hit);
            }
        }
    }

    public bool TryGetShotHitPoint(out Vector3 hitPoint)
    {
        if (firePoint == null)
        {
            hitPoint = Vector3.zero;
            return false;
        }

        bool isAiming = ownerCombat != null && ownerCombat.IsAiming;
        Vector3 direction = GetBaseDirection(isAiming);
        Ray ray = new Ray(firePoint.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            hitPoint = hit.point;
            return true;
        }

        hitPoint = firePoint.position + direction * range;
        return true;
    }

    private Vector3 GetBaseDirection(bool isAiming)
    {
        if (isAiming && mainCamera != null)
        {
            Ray cameraRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(cameraRay, out RaycastHit hit, range, hitMask))
            {
                return (hit.point - firePoint.position).normalized;
            }

            return cameraRay.direction.normalized;
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

    private void ApplyDamage(RaycastHit hit)
    {
        if (TryGetDamageableFromCollider(hit.collider, out IDamageable damageable))
        {
            damageable.TakeDamage(GetDamageValue());
        }

        SpawnPooledEffect(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal), 2f);
    }

    private void Reset()
    {
        weaponDisplayName = "Shotgun";
        weaponKind = WeaponKind.Shotgun;
        damage = 9;
        fireRate = 1.1f;
        magazineSize = 8;
        reserveAmmo = 40;
        reloadTime = 2.1f;
        pelletCount = 8;
        range = 45f;
        projectileSpeed = 130f;
        aimSpreadAngle = 2.8f;
        hipFireSpreadAngle = 8.5f;
    }
}
