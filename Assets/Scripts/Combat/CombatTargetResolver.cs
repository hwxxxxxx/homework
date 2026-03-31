using UnityEngine;

public static class CombatTargetResolver
{
    public static bool TryResolveDamageable(Collider collider, out IDamageable damageable)
    {
        damageable = null;
        if (collider == null)
        {
            return false;
        }

        damageable = collider.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = collider.GetComponentInParent<IDamageable>();
        }

        return damageable != null;
    }
}
