using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyAIController))]
[RequireComponent(typeof(EnemyCombat))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private EnemyStats enemyStats;
    [SerializeField] private EnemyAIController enemyAIController;
    [SerializeField] private EnemyCombat enemyCombat;
    [SerializeField] private Collider[] collidersToDisableOnDeath;

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
        if (enemyStats == null)
        {
            return;
        }

        enemyStats.TakeDamage(damage);
    }

    private void OnDied()
    {
        if (enemyAIController != null)
        {
            enemyAIController.SetDead();
        }

        if (enemyCombat != null)
        {
            enemyCombat.SetDead();
        }

        DisableColliders();
        Destroy(gameObject, 2f);
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
}
