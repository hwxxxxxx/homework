using UnityEngine;

[CreateAssetMenu(menuName = "Game/Flow/Run Config", fileName = "RunConfig")]
public class RunConfigAsset : ScriptableObject
{
    [Header("Core")]
    [SerializeField] private WaveConfigAsset waveConfig;
    [SerializeField] private BossDefinitionAsset bossDefinition;

    [Header("Result")]
    [SerializeField] private float resultDelaySeconds = 2f;

    public WaveConfigAsset WaveConfig => waveConfig;
    public BossDefinitionAsset BossDefinition => bossDefinition;
    public float ResultDelaySeconds => Mathf.Max(0f, resultDelaySeconds);
}
