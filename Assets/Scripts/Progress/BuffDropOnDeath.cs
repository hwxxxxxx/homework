using UnityEngine;

[RequireComponent(typeof(EnemyBase))]
public class BuffDropOnDeath : MonoBehaviour
{
    [SerializeField] private BuffDropTableAsset dropTable;
    [SerializeField] private RunContextService runContextService;
    [SerializeField] private EffectController targetEffectController;

    private EnemyBase enemyBase;

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
    }

    public void SetRunContextService(RunContextService service)
    {
        runContextService = service;
    }

    public void SetTargetEffectController(EffectController controller)
    {
        targetEffectController = controller;
    }

    private void OnEnable()
    {
        if (enemyBase != null)
        {
            enemyBase.OnEnemyDied += HandleEnemyDied;
        }
    }

    private void OnDisable()
    {
        if (enemyBase != null)
        {
            enemyBase.OnEnemyDied -= HandleEnemyDied;
        }
    }

    private void HandleEnemyDied(EnemyBase enemy)
    {
        if (dropTable == null || targetEffectController == null)
        {
            return;
        }

        if (runContextService != null && !runContextService.IsRunActive)
        {
            return;
        }

        if (!dropTable.TryRoll(out EffectAsset effect, out int stackCount))
        {
            return;
        }

        for (int i = 0; i < stackCount; i++)
        {
            targetEffectController.ApplyEffect(effect, enemy != null ? enemy.gameObject : null);
        }
    }
}
