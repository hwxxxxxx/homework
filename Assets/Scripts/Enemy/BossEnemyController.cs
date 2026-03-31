using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyAIController))]
[RequireComponent(typeof(StatBlock))]
public class BossEnemyController : MonoBehaviour, IPoolable
{
    [SerializeField] private BossDefinitionAsset definition;

    [Header("Skill System")]
    [SerializeField] private BossSkillController skillController;

    private const string EnrageDamageModifierId = "boss.enrage.damage";
    private const string EnrageAttackIntervalModifierId = "boss.enrage.interval";

    private EnemyStats enemyStats;
    private EnemyAIController enemyAIController;
    private StatBlock statBlock;
    private NavMeshAgent navMeshAgent;
    private EffectController effectController;

    private bool enraged;
    private bool initialized;
    private bool isSkillCasting;

    public EnemyAIController AIController => enemyAIController;
    public EnemyStats Stats => enemyStats;
    public StatBlock StatBlock => statBlock;
    public NavMeshAgent Agent => navMeshAgent;
    public EffectController EffectController => effectController;
    public bool IsSkillCasting => isSkillCasting;

    private void Awake()
    {
        enemyStats = GetComponent<EnemyStats>();
        enemyAIController = GetComponent<EnemyAIController>();
        statBlock = GetComponent<StatBlock>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        effectController = GetComponent<EffectController>();
        if (skillController == null)
        {
            skillController = GetComponent<BossSkillController>();
        }
    }

    private void OnEnable()
    {
        if (enemyStats != null)
        {
            enemyStats.OnHealthChanged += HandleHealthChanged;
        }

        initialized = false;
        isSkillCasting = false;
        if (skillController != null)
        {
            skillController.Configure(GetConfiguredSkills(), GetSkillThinkInterval());
            skillController.ResetState();
        }
    }

    private void OnDisable()
    {
        if (enemyStats != null)
        {
            enemyStats.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void Update()
    {
        if (!initialized)
        {
            InitializeBoss();
        }

        if (enemyStats == null || enemyStats.IsDead || enemyAIController == null)
        {
            return;
        }

        if (!enemyAIController.EnsureTarget())
        {
            return;
        }

        if (isSkillCasting)
        {
            enemyAIController.StopMoving();
            enemyAIController.SetAttackEnabled(false);
            return;
        }

        if (skillController != null)
        {
            skillController.Tick(this, enemyAIController.Target);
        }
    }

    public void OnSpawnedFromPool()
    {
        initialized = false;
        enraged = false;
        isSkillCasting = false;
        if (skillController != null)
        {
            skillController.Configure(GetConfiguredSkills(), GetSkillThinkInterval());
            skillController.ResetState();
        }
    }

    public void OnDespawnedToPool()
    {
        initialized = false;
        enraged = false;
        isSkillCasting = false;
    }

    public void BeginSkillCast()
    {
        isSkillCasting = true;
        if (enemyAIController != null)
        {
            enemyAIController.StopMoving();
            enemyAIController.SetAttackEnabled(false);
        }
    }

    public void EndSkillCast()
    {
        isSkillCasting = false;
    }

    private void InitializeBoss()
    {
        if (statBlock == null || enemyStats == null)
        {
            return;
        }

        if (definition == null)
        {
            Debug.LogError("BossEnemyController: missing BossDefinitionAsset.", this);
            return;
        }

        if (definition.applyBossScale)
        {
            transform.localScale = definition.bossScale;
        }

        statBlock.SetBaseValue(StatIds.MaxHealth, definition.maxHealth);
        statBlock.SetBaseValue(StatIds.EnemyAttackDamage, definition.attackDamage);
        statBlock.SetBaseValue(StatIds.EnemyAttackInterval, definition.attackInterval);

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = definition.moveSpeed;
        }

        enemyStats.ResetStats();
        initialized = true;
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (enraged || max <= 0)
        {
            return;
        }

        float ratio = (float)current / max;
        if (definition == null || ratio > definition.enrageHealthThresholdRatio)
        {
            return;
        }

        TriggerEnrage();
    }

    private void TriggerEnrage()
    {
        if (enraged || statBlock == null || definition == null)
        {
            return;
        }

        enraged = true;
        statBlock.AddModifier(
            StatIds.EnemyAttackDamage,
            new StatModifier(
                EnrageDamageModifierId,
                definition.enrageDamagePercentAdd,
                StatModifierOperation.PercentAdd,
                this,
                100
            )
        );
        statBlock.AddModifier(
            StatIds.EnemyAttackInterval,
            new StatModifier(
                EnrageAttackIntervalModifierId,
                -Mathf.Abs(definition.enrageAttackSpeedPercentAdd),
                StatModifierOperation.PercentAdd,
                this,
                100
            )
        );

        if (navMeshAgent != null)
        {
            navMeshAgent.speed *= 1f + Mathf.Max(0f, definition.enrageMoveSpeedPercentAdd);
        }
    }

    private BossSkillAsset[] GetConfiguredSkills()
    {
        return definition != null ? definition.skills : null;
    }

    private float GetSkillThinkInterval()
    {
        return definition != null ? definition.skillThinkInterval : 0.1f;
    }
}
