using System;
using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyAIController))]
[RequireComponent(typeof(EnemyCombat))]
public class EnemyBase : MonoBehaviour, IDamageable, IPoolable
{
    [Header("References")]
    [SerializeField] private EnemyStats enemyStats;
    [SerializeField] private EnemyAIController enemyAIController;
    [SerializeField] private EnemyCombat enemyCombat;
    [SerializeField] private Collider[] collidersToDisableOnDeath;
    [SerializeField] private float despawnDelayOnDeath = 2f;

    public event Action<EnemyBase> OnEnemyDied;
    private bool hasDied;

    private void Awake()
    {
        if (enemyStats == null)
        {
            enemyStats = GetComponent<EnemyStats>();
        }

        if (enemyAIController == null)
        {
            enemyAIController = GetComponent<EnemyAIController>();
        }

        if (enemyCombat == null)
        {
            enemyCombat = GetComponent<EnemyCombat>();
        }

        if (collidersToDisableOnDeath == null || collidersToDisableOnDeath.Length == 0)
        {
            collidersToDisableOnDeath = GetComponentsInChildren<Collider>();
        }
    }

    private void OnEnable()
    {
        if (enemyStats != null)
        {
            enemyStats.OnDied += OnDied;
        }
    }

    private void OnDisable()
    {
        if (enemyStats != null)
        {
            enemyStats.OnDied -= OnDied;
        }
    }

    public void TakeDamage(int damage)
    {
        if (hasDied)
        {
            return;
        }

        if (enemyStats == null)
        {
            return;
        }

        enemyStats.TakeDamage(damage);
    }

    private void OnDied()
    {
        if (hasDied)
        {
            return;
        }

        hasDied = true;
        if (TryGetComponent<BossEnemyController>(out BossEnemyController _))
        {
            EventBus.Publish(new BossDefeatedEvent(gameObject));
        }

        OnEnemyDied?.Invoke(this);

        if (enemyAIController != null)
        {
            enemyAIController.SetDead();
        }

        if (enemyCombat != null)
        {
            enemyCombat.SetDead();
        }

        DisableColliders();
        PoolService.DespawnAfterDelay(gameObject, despawnDelayOnDeath);
    }

    private void DisableColliders()
    {
        if (collidersToDisableOnDeath == null)
        {
            return;
        }

        for (int i = 0; i < collidersToDisableOnDeath.Length; i++)
        {
            Collider enemyCollider = collidersToDisableOnDeath[i];
            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }
        }
    }

    private void EnableColliders()
    {
        if (collidersToDisableOnDeath == null)
        {
            return;
        }

        for (int i = 0; i < collidersToDisableOnDeath.Length; i++)
        {
            Collider enemyCollider = collidersToDisableOnDeath[i];
            if (enemyCollider != null)
            {
                enemyCollider.enabled = true;
            }
        }
    }

    public void OnSpawnedFromPool()
    {
        hasDied = false;
        EnableColliders();

        if (enemyStats != null)
        {
            enemyStats.ResetStats();
        }

        if (enemyCombat != null)
        {
            enemyCombat.ResetCombat();
        }

        if (enemyAIController != null)
        {
            enemyAIController.ResetAI();
        }
    }

    public void OnDespawnedToPool()
    {
        hasDied = false;
    }
}
