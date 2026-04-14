using UnityEngine;

[System.Serializable]
public struct EffectVisualEntry
{
    public string effectId;
    public GameObject visualPrefab;
}

[CreateAssetMenu(menuName = "Game/Config/Run Loot Config", fileName = "RunLootConfig")]
public class RunLootConfigAsset : ScriptableObject
{
    [SerializeField] private BuffPickupItem buffPickupPrefab;
    [SerializeField] private float buffPickupSpawnHeightOffset = 0.6f;
    [SerializeField] private GameObject damageBuffVisualPrefab;
    [SerializeField] private GameObject fireRateBuffVisualPrefab;
    [SerializeField] private GameObject reloadBuffVisualPrefab;
    [SerializeField] private EffectVisualEntry[] effectVisualMappings;

    public BuffPickupItem BuffPickupPrefab => buffPickupPrefab;
    public float BuffPickupSpawnHeightOffset => buffPickupSpawnHeightOffset;
    public GameObject DamageBuffVisualPrefab => damageBuffVisualPrefab;
    public GameObject FireRateBuffVisualPrefab => fireRateBuffVisualPrefab;
    public GameObject ReloadBuffVisualPrefab => reloadBuffVisualPrefab;

    public bool TryGetEffectVisualPrefab(string effectId, out GameObject prefab)
    {
        prefab = null;
        if (string.IsNullOrWhiteSpace(effectId) || effectVisualMappings == null)
        {
            return false;
        }

        for (int i = 0; i < effectVisualMappings.Length; i++)
        {
            EffectVisualEntry entry = effectVisualMappings[i];
            if (!string.Equals(entry.effectId, effectId))
            {
                continue;
            }

            prefab = entry.visualPrefab;
            return prefab != null;
        }

        return false;
    }
}
