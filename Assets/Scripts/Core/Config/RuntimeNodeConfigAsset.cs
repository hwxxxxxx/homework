using UnityEngine;

[CreateAssetMenu(menuName = "Game/Config/Runtime Node Config", fileName = "RuntimeNodeConfig")]
public class RuntimeNodeConfigAsset : ScriptableObject
{
    [SerializeField] private string poolRootName = "GlobalObjectPools";
    [SerializeField] private string loadingScreenRootName = "GlobalLoadingScreenRoot";
    [SerializeField] private string enemyHealthOverlayRootName = "EnemyHealthOverlay";
    [SerializeField] private string battleSharedRootName = "BattleSharedRoot";

    public string PoolRootName => poolRootName;
    public string LoadingScreenRootName => loadingScreenRootName;
    public string EnemyHealthOverlayRootName => enemyHealthOverlayRootName;
    public string BattleSharedRootName => battleSharedRootName;
}
