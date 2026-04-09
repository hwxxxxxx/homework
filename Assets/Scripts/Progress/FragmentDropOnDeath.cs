using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyBase))]
public class FragmentDropOnDeath : EnemyDeathDropListenerBase
{
    [SerializeField] private DropTableAsset dropTable;

    protected override void HandleEnemyDiedInRun(EnemyBase enemy)
    {
        Dictionary<FragmentType, int> drops = dropTable.Roll();
        foreach (KeyValuePair<FragmentType, int> pair in drops)
        {
            runContextService.RecordDrop(pair.Key, pair.Value);
        }
    }
}
