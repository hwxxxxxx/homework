using System;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StatBlock statBlock;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private string damageStatId = StatIds.EnemyAttackDamage;
    [SerializeField] private string attackIntervalStatId = StatIds.EnemyAttackInterval;

    private Transform target;
    private bool canAttack;
    private bool isDead;
    private float lastAttackTime = float.NegativeInfinity;
    private float attackRange;
    private bool requireLineOfSight;
    private LayerMask lineOfSightMask;
    private float attackOriginHeightOffset;
    private float targetHeightOffset;
    private bool isBoss;

    public event Action OnAttack;

    public float AttackRange => attackRange;

    private void Awake()
    {
        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }

        if (statBlock == null)
        {
            statBlock = GetComponent<StatBlock>();
        }

        isBoss = GetComponent<BossEnemyController>() != null;
    }

    private void Update()
    {
        if (!canAttack || isDead || target == null)
        {
            return;
        }

        float currentAttackInterval = GetAttackInterval();
        if (Time.time < lastAttackTime + currentAttackInterval)
        {
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > attackRange)
        {
            return;
        }

        if (requireLineOfSight && !HasLineOfSight(target))
        {
            return;
        }

        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = target.GetComponentInParent<IDamageable>();
        }

        if (damageable == null)
        {
            return;
        }

        damageable.TakeDamage(GetDamageValue());
        lastAttackTime = Time.time;
        EventBus.Publish(new EnemyAttackEvent(gameObject, isBoss, transform.position));
        OnAttack?.Invoke();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetCanAttack(bool value)
    {
        canAttack = value;
    }

    public void SetDead()
    {
        isDead = true;
        canAttack = false;
        target = null;
    }

    public void ResetCombat()
    {
        isDead = false;
        canAttack = false;
        target = null;
        lastAttackTime = float.NegativeInfinity;
    }

    public void ApplyConfig(EnemyConfigAsset config)
    {
        attackRange = config.AttackRange;
        requireLineOfSight = config.RequireLineOfSight;
        lineOfSightMask = config.LineOfSightMask;
        attackOriginHeightOffset = config.AttackOriginHeightOffset;
        targetHeightOffset = config.TargetHeightOffset;
    }

    private bool HasLineOfSight(Transform potentialTarget)
    {
        Vector3 origin = attackOrigin.position + Vector3.up * attackOriginHeightOffset;
        Vector3 targetPosition = potentialTarget.position + Vector3.up * targetHeightOffset;
        Vector3 direction = targetPosition - origin;

        if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, lineOfSightMask))
        {
            return true;
        }

        return hit.transform == potentialTarget || hit.transform.IsChildOf(potentialTarget);
    }

    private int GetDamageValue()
    {
        return Mathf.RoundToInt(statBlock.GetStatValue(damageStatId));
    }

    private float GetAttackInterval()
    {
        return statBlock.GetStatValue(attackIntervalStatId);
    }
}
