using UnityEngine;

public class ProjectileBullet : MonoBehaviour, IPoolable
{
    private Vector3 direction;
    private float speed;
    private float maxDistance;
    private int damage;
    private LayerMask hitMask;
    private string ignoreTag;
    private ParticleSystem impactEffectPrefab;
    private float impactEffectLifetime;
    private float explosionRadius;
    private float lifeSeconds;
    private float traveledDistance;
    private float lifeTimer;
    private bool initialized;

    public void Initialize(
        Vector3 shotDirection,
        float shotSpeed,
        float shotRange,
        int shotDamage,
        LayerMask shotHitMask,
        string shotIgnoreTag,
        ParticleSystem impactPrefab,
        float shotLifeSeconds,
        float shotImpactEffectLifetime,
        float shotExplosionRadius
    )
    {
        direction = shotDirection.normalized;
        speed = shotSpeed;
        maxDistance = shotRange;
        damage = shotDamage;
        hitMask = shotHitMask;
        ignoreTag = shotIgnoreTag ?? string.Empty;
        impactEffectPrefab = impactPrefab;
        lifeSeconds = shotLifeSeconds;
        impactEffectLifetime = shotImpactEffectLifetime;
        explosionRadius = shotExplosionRadius;
        traveledDistance = 0f;
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
        if (delta <= 0f)
        {
            return;
        }

        lifeTimer += delta;
        if (lifeTimer >= lifeSeconds)
        {
            DespawnSelf();
            return;
        }

        float step = speed * delta;
        Vector3 origin = transform.position;

        if (TryHitInStep(origin, step, out RaycastHit hit))
        {
            HandleHit(hit);
            return;
        }

        transform.position = origin + direction * step;
        traveledDistance += step;
        if (traveledDistance >= maxDistance)
        {
            DespawnSelf();
        }
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
            if (!string.IsNullOrEmpty(ignoreTag) && candidate.collider != null && candidate.collider.CompareTag(ignoreTag))
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

    private void HandleHit(RaycastHit hit)
    {
        transform.position = hit.point;

        if (explosionRadius > 0f)
        {
            ApplyExplosionDamage(hit.point);
        }
        else if (CombatTargetResolver.TryResolveDamageable(hit.collider, out IDamageable damageable))
        {
            ApplyDamageAndPublishHitEvent(hit.collider, damageable, hit.point);
        }

        if (impactEffectPrefab != null)
        {
            ParticleSystem fx = PoolService.Spawn(
                impactEffectPrefab,
                hit.point,
                Quaternion.LookRotation(hit.normal)
            );
            if (fx != null)
            {
                fx.Clear(true);
                fx.Play(true);
                PoolService.DespawnAfterDelay(fx.gameObject, impactEffectLifetime);
            }
        }

        DespawnSelf();
    }

    private void ApplyExplosionDamage(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, explosionRadius, hitMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            return;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            Collider targetCollider = hits[i];
            if (targetCollider == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(ignoreTag) && targetCollider.CompareTag(ignoreTag))
            {
                continue;
            }

            if (!CombatTargetResolver.TryResolveDamageable(targetCollider, out IDamageable damageable))
            {
                continue;
            }

            ApplyDamageAndPublishHitEvent(targetCollider, damageable, center);
        }
    }

    private void ApplyDamageAndPublishHitEvent(Collider hitCollider, IDamageable damageable, Vector3 hitPoint)
    {
        damageable.TakeDamage(damage);
        EnemyBase enemyBase = hitCollider.GetComponentInParent<EnemyBase>();
        if (enemyBase == null)
        {
            return;
        }

        bool isBoss = enemyBase.GetComponent<BossEnemyController>() != null;
        EventBus.Publish(new PlayerHitEnemyEvent(enemyBase.gameObject, isBoss, hitPoint));
    }

    public void OnSpawnedFromPool()
    {
        traveledDistance = 0f;
        lifeTimer = 0f;
        initialized = false;
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
