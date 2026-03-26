using System;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackRange = 2f;
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
    }

    private void Update()
    {
        if (!canAttack || isDead || target == null)
        {
            return;
        }

        if (Time.time < lastAttackTime + attackInterval)
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

        damageable.TakeDamage(damage);
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
}
