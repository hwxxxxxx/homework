using UnityEngine;

public class HitscanWeapon : BallisticWeaponBase
{
    protected override void Fire()
    {
        Vector3 shotDirection = GetFireDirection();
        TryGetFireRay(shotDirection, out Ray _, out RaycastHit hit, out bool hasHit);

        SpawnMuzzleEffect(shotDirection);

        if (TrySpawnProjectile(shotDirection))
        {
            return;
        }

        if (hasHit)
        {
            ApplyDamage(hit);
        }
    }
}
