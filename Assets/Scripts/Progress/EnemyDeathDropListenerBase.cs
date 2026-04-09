using UnityEngine;

public abstract class EnemyDeathDropListenerBase : MonoBehaviour
{
    [SerializeField] protected RunContextService runContextService;

    private EnemyBase enemyBase;

    protected virtual void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
    }

    public void SetRunContextService(RunContextService service)
    {
        runContextService = service;
    }

    protected virtual void OnEnable()
    {
        enemyBase.OnEnemyDied += HandleEnemyDied;
    }

    protected virtual void OnDisable()
    {
        enemyBase.OnEnemyDied -= HandleEnemyDied;
    }

    private void HandleEnemyDied(EnemyBase enemy)
    {
        if (!runContextService.IsRunActive)
        {
            return;
        }

        HandleEnemyDiedInRun(enemy);
    }

    protected abstract void HandleEnemyDiedInRun(EnemyBase enemy);
}
