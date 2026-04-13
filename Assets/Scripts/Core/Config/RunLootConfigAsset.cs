using UnityEngine;

[CreateAssetMenu(menuName = "Game/Config/Run Loot Config", fileName = "RunLootConfig")]
public class RunLootConfigAsset : ScriptableObject
{
    [SerializeField] private BuffPickupItem buffPickupPrefab;
    [SerializeField] private float buffPickupSpawnHeightOffset = 0.6f;
    [SerializeField] private GameObject damageBuffVisualPrefab;
    [SerializeField] private GameObject fireRateBuffVisualPrefab;
    [SerializeField] private GameObject reloadBuffVisualPrefab;

    public BuffPickupItem BuffPickupPrefab => buffPickupPrefab;
    public float BuffPickupSpawnHeightOffset => buffPickupSpawnHeightOffset;
    public GameObject DamageBuffVisualPrefab => damageBuffVisualPrefab;
    public GameObject FireRateBuffVisualPrefab => fireRateBuffVisualPrefab;
    public GameObject ReloadBuffVisualPrefab => reloadBuffVisualPrefab;
}
