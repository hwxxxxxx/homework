using UnityEngine;

public interface IWeaponAimPointProvider
{
    bool TryGetShotHitPoint(out Vector3 hitPoint);
}
