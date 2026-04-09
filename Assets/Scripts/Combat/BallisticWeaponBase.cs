using UnityEngine;

public abstract class BallisticWeaponBase : WeaponBase, IWeaponAimPointProvider
{
    [Header("References")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected Transform hipFireForwardReference;
    [SerializeField] protected Camera mainCamera;
    [SerializeField] protected ProjectileBullet projectilePrefab;
    [SerializeField] protected ParticleSystem muzzleFlashPrefab;
    [SerializeField] protected ParticleSystem impactEffectPrefab;

    [Header("Debug")]
    [SerializeField] protected bool drawDebugRay = true;

    protected virtual void Start()
    {
        PrepareParticleTemplate(muzzleFlashPrefab);
        PrepareParticleTemplate(impactEffectPrefab);
    }

    public bool TryGetShotHitPoint(out Vector3 hitPoint)
    {
        if (!TryGetFireRay(GetBaseDirection(IsAiming()), out Ray fireRay, out RaycastHit hit, out bool hasHit))
        {
            hitPoint = Vector3.zero;
            return false;
        }

        hitPoint = hasHit ? hit.point : fireRay.origin + fireRay.direction * weaponConfig.Range;
        return true;
    }

    protected bool IsAiming()
    {
        return ownerCombat != null && ownerCombat.IsAiming;
    }

    protected Vector3 GetFireDirection()
    {
        float spread = IsAiming() ? weaponConfig.AimSpreadAngle : weaponConfig.HipFireSpreadAngle;
        Vector3 baseDirection = GetBaseDirection(IsAiming());
        return ApplySpread(baseDirection, spread);
    }

    protected Vector3 GetBaseDirection(bool isAiming)
    {
        if (!isAiming)
        {
            return firePoint.forward;
        }

        Vector3 aimPoint = GetCameraAimPoint();
        return (aimPoint - firePoint.position).normalized;
    }

    protected Vector3 ApplySpread(Vector3 direction, float spreadAngle)
    {
        if (spreadAngle <= 0.001f)
        {
            return direction.normalized;
        }

        Vector2 spread = Random.insideUnitCircle * Mathf.Tan(spreadAngle * Mathf.Deg2Rad);
        Vector3 spreadDir = direction + transform.right * spread.x + transform.up * spread.y;
        return spreadDir.normalized;
    }

    protected Vector3 GetCameraAimPoint()
    {
        Ray cameraRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (drawDebugRay)
        {
            Debug.DrawRay(cameraRay.origin, cameraRay.direction * weaponConfig.Range, Color.green, 1f);
        }

        if (Physics.Raycast(cameraRay, out RaycastHit hit, weaponConfig.Range, weaponConfig.HitMask))
        {
            return hit.point;
        }

        return cameraRay.origin + cameraRay.direction * weaponConfig.Range;
    }

    protected bool TryGetFireRay(
        Vector3 direction,
        out Ray fireRay,
        out RaycastHit hit,
        out bool hasHit
    )
    {
        fireRay = new Ray(firePoint.position, direction);
        hasHit = Physics.Raycast(fireRay, out hit, weaponConfig.Range, weaponConfig.HitMask);

        if (drawDebugRay)
        {
            Debug.DrawRay(firePoint.position, direction * weaponConfig.Range, GetShotDebugColor(), 1f);
        }

        return true;
    }

    protected void SpawnMuzzleEffect(Vector3 shotDirection)
    {
        SpawnPooledEffect(
            muzzleFlashPrefab,
            firePoint.position,
            Quaternion.LookRotation(shotDirection),
            weaponConfig.MuzzleEffectLifetime
        );
    }

    protected bool TrySpawnProjectile(Vector3 shotDirection)
    {
        if (projectilePrefab == null)
        {
            return false;
        }

        ProjectileBullet bullet = PoolService.Spawn(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(shotDirection)
        );
        bullet.Initialize(
            shotDirection,
            weaponConfig.ProjectileSpeed,
            weaponConfig.Range,
            GetDamageValue(),
            weaponConfig.HitMask,
            CombatConfigProvider.Config.PlayerTag,
            impactEffectPrefab,
            weaponConfig.ProjectileLifetime,
            weaponConfig.ImpactEffectLifetime
        );
        return true;
    }

    protected void ApplyDamage(RaycastHit hit)
    {
        if (TryGetDamageableFromCollider(hit.collider, out IDamageable damageable))
        {
            damageable.TakeDamage(GetDamageValue());
        }

        SpawnPooledEffect(
            impactEffectPrefab,
            hit.point,
            Quaternion.LookRotation(hit.normal),
            weaponConfig.ImpactEffectLifetime
        );
    }

    protected virtual Color GetShotDebugColor()
    {
        return Color.red;
    }
}
