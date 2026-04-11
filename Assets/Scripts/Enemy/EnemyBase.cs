using System;
using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyAIController))]
[RequireComponent(typeof(EnemyCombat))]
public class EnemyBase : MonoBehaviour, IDamageable, IPoolable
{
    [Header("References")]
    [SerializeField] private EnemyConfigAsset enemyConfig;
    [SerializeField] private EnemyStats enemyStats;
    [SerializeField] private EnemyAIController enemyAIController;
    [SerializeField] private EnemyCombat enemyCombat;
    [SerializeField] private StatBlock statBlock;
    [SerializeField] private Collider[] collidersToDisableOnDeath;

    private float despawnDelayOnDeath;

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

        if (statBlock == null)
        {
            statBlock = GetComponent<StatBlock>();
        }

        if (collidersToDisableOnDeath == null || collidersToDisableOnDeath.Length == 0)
        {
            collidersToDisableOnDeath = GetComponentsInChildren<Collider>();
        }

        ApplyConfig();
        enemyStats.ResetStats();
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
        bool isBoss = TryGetComponent<BossEnemyController>(out BossEnemyController _);
        if (isBoss)
        {
            EventBus.Publish(new BossDefeatedEvent(gameObject));
        }
        EventBus.Publish(new EnemyDiedEvent(gameObject, isBoss, transform.position));

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
        ApplyConfig();

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

    private void ApplyConfig()
    {
        if (enemyConfig == null)
        {
            Debug.LogError($"EnemyBase on '{name}' is missing EnemyConfigAsset.");
            return;
        }

        if (statBlock == null || enemyCombat == null || enemyAIController == null)
        {
            Debug.LogError($"EnemyBase on '{name}' is missing required runtime components.");
            return;
        }

        statBlock.SetBaseValue(StatIds.MaxHealth, enemyConfig.MaxHealth);
        statBlock.SetBaseValue(StatIds.EnemyAttackDamage, enemyConfig.AttackDamage);
        statBlock.SetBaseValue(StatIds.EnemyAttackInterval, enemyConfig.AttackInterval);

        enemyCombat.ApplyConfig(enemyConfig);
        enemyAIController.ApplyConfig(enemyConfig);
        despawnDelayOnDeath = enemyConfig.DespawnDelayOnDeath;
    }
}
