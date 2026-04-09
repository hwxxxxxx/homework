using UnityEngine;

[RequireComponent(typeof(EnemyBase))]
public class BuffDropOnDeath : EnemyDeathDropListenerBase
{
    [SerializeField] private BuffDropTableAsset dropTable;
    [SerializeField] private EffectController targetEffectController;

    public void SetTargetEffectController(EffectController controller)
    {
        targetEffectController = controller;
    }

    protected override void HandleEnemyDiedInRun(EnemyBase enemy)
    {
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
