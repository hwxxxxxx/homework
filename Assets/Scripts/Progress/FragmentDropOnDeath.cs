using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyBase))]
public class FragmentDropOnDeath : MonoBehaviour
{
    [SerializeField] private DropTableAsset dropTable;
    [SerializeField] private RunContextService runContextService;

    private EnemyBase enemyBase;

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
    }

    public void SetRunContextService(RunContextService service)
    {
        runContextService = service;
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
        if (dropTable == null || !runContextService.IsRunActive)
        {
            return;
        }

        Dictionary<FragmentType, int> drops = dropTable.Roll();
        foreach (KeyValuePair<FragmentType, int> pair in drops)
        {
            runContextService.RecordDrop(pair.Key, pair.Value);
        }
    }
}
