using UnityEngine;

public class ShotgunWeapon : BallisticWeaponBase
{
    protected override void Fire()
    {
        bool isAiming = IsAiming();
        Vector3 baseDirection = GetBaseDirection(isAiming);
        float spread = isAiming ? weaponConfig.AimSpreadAngle : weaponConfig.HipFireSpreadAngle;

        SpawnMuzzleEffect(baseDirection);

        for (int i = 0; i < weaponConfig.PelletCount; i++)
        {
            Vector3 direction = ApplySpread(baseDirection, spread);
            if (TrySpawnProjectile(direction))
            {
                continue;
            }

            TryGetFireRay(direction, out Ray _, out RaycastHit hit, out bool hasHit);
            if (hasHit)
            {
                ApplyDamage(hit);
            }
        }
    }

    protected override Color GetShotDebugColor()
    {
        return Color.yellow;
    }
}
