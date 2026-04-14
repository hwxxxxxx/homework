using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IPoolable
{
    private Transform owner;
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private int damage;
    private LayerMask hitMask;
    private float lifeTimer;
    private bool initialized;

    public void Initialize(
        Transform projectileOwner,
        Vector3 shotDirection,
        float shotSpeed,
        float shotLifetime,
        int shotDamage,
        LayerMask shotHitMask
    )
    {
        owner = projectileOwner;
        direction = shotDirection.normalized;
        speed = shotSpeed;
        lifetime = shotLifetime;
        damage = shotDamage;
        hitMask = shotHitMask;
        lifeTimer = 0f;
        initialized = true;
        transform.forward = direction;
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        float delta = Time.deltaTime;
        lifeTimer += delta;
        if (lifeTimer >= lifetime)
        {
            DespawnSelf();
            return;
        }

        float step = speed * delta;
        Vector3 origin = transform.position;

        if (TryHitInStep(origin, step, out RaycastHit hit))
        {
            transform.position = hit.point;
            if (CombatTargetResolver.TryResolveDamageable(hit.collider, out IDamageable damageable))
            {
                damageable.TakeDamage(damage);
            }

            DespawnSelf();
            return;
        }

        transform.position = origin + direction * step;
    }

    private bool TryHitInStep(Vector3 origin, float step, out RaycastHit closestValidHit)
    {
        closestValidHit = default;
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, step, hitMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            return false;
        }

        float minDistance = float.MaxValue;
        bool found = false;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit candidate = hits[i];
            if (!IsValidHit(candidate))
            {
                continue;
            }

            if (candidate.distance < minDistance)
            {
                minDistance = candidate.distance;
                closestValidHit = candidate;
                found = true;
            }
        }

        return found;
    }

    private bool IsValidHit(RaycastHit hit)
    {
        Transform hitTransform = hit.collider != null ? hit.collider.transform : null;
        if (hitTransform == null)
        {
            return false;
        }

        if (owner != null && (hitTransform == owner || hitTransform.IsChildOf(owner)))
        {
            return false;
        }

        EnemyBase enemy = hitTransform.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            return false;
        }

        PlayerStats player = hitTransform.GetComponentInParent<PlayerStats>();
        return player != null;
    }

    public void OnSpawnedFromPool()
    {
        initialized = false;
        lifeTimer = 0f;
    }

    public void OnDespawnedToPool()
    {
        initialized = false;
    }

    private void DespawnSelf()
    {
        initialized = false;
        PoolService.Despawn(gameObject);
    }
}
