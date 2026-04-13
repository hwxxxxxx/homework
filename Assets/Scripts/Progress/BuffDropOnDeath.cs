using UnityEngine;

[RequireComponent(typeof(EnemyBase))]
public class BuffDropOnDeath : EnemyDeathDropListenerBase
{
    [SerializeField] private BuffDropTableAsset dropTable;
    [SerializeField] private EffectController targetEffectController;

    public void SetTargetEffectController(EffectController controller)
    {
        if (controller == null)
        {
            throw new System.ArgumentNullException(nameof(controller));
        }

        targetEffectController = controller;
    }

    protected override void HandleEnemyDiedInRun(EnemyBase enemy)
    {
        if (dropTable == null || targetEffectController == null)
        {
            throw new System.InvalidOperationException("BuffDropOnDeath requires dropTable and targetEffectController.");
        }

        RunLootConfigAsset config = RunLootConfigProvider.Config;
        if (config.BuffPickupPrefab == null)
        {
            throw new System.InvalidOperationException("RunLootConfig.BuffPickupPrefab is required.");
        }

        if (!dropTable.TryRoll(out EffectAsset effect, out int stackCount))
        {
            return;
        }

        string buffStatId = ResolveBuffStatId(effect);
        Vector3 spawnPosition = (enemy != null ? enemy.transform.position : transform.position) + Vector3.up * config.BuffPickupSpawnHeightOffset;
        BuffPickupItem pickupItem = PoolService.Spawn(config.BuffPickupPrefab, spawnPosition, Quaternion.identity);
        pickupItem.InitializeDrop(effect, stackCount, targetEffectController, buffStatId);
    }

    private static string ResolveBuffStatId(EffectAsset effect)
    {
        if (!(effect is StatModifierEffectAsset statModifierEffect) ||
            statModifierEffect.Modifiers == null ||
            statModifierEffect.Modifiers.Length == 0)
        {
            return string.Empty;
        }

        return statModifierEffect.Modifiers[0].statId;
    }
}
