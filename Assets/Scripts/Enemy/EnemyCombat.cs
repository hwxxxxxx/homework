using System;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private StatBlock statBlock;
    [SerializeField] private string damageStatId = StatIds.EnemyAttackDamage;
    [SerializeField] private string attackIntervalStatId = StatIds.EnemyAttackInterval;
    [SerializeField] private bool requireLineOfSight;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private LayerMask lineOfSightMask = ~0;

    private Transform target;
    private bool canAttack;
    private bool isDead;
    private float lastAttackTime = -999f;

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
        lastAttackTime = -999f;
    }

    private bool HasLineOfSight(Transform potentialTarget)
    {
        Vector3 origin = attackOrigin.position + Vector3.up * 0.5f;
        Vector3 targetPosition = potentialTarget.position + Vector3.up * 1f;
        Vector3 direction = targetPosition - origin;

        if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit, direction.magnitude, lineOfSightMask))
        {
            return true;
        }

        return hit.transform == potentialTarget || hit.transform.IsChildOf(potentialTarget);
    }

    private int GetDamageValue()
    {
        return Mathf.Max(1, Mathf.RoundToInt(GetStatOrDefault(damageStatId, damage)));
    }

    private float GetAttackInterval()
    {
        return Mathf.Max(0.05f, GetStatOrDefault(attackIntervalStatId, attackInterval));
    }

    private float GetStatOrDefault(string statId, float defaultValue)
    {
        if (statBlock == null || string.IsNullOrWhiteSpace(statId) || !statBlock.HasStat(statId))
        {
            return defaultValue;
        }

        return statBlock.GetStatValue(statId);
    }
}
