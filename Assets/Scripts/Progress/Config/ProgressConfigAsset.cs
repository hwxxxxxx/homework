using UnityEngine;

[CreateAssetMenu(menuName = "Game/Progress/Progress Config", fileName = "ProgressConfig")]
public class ProgressConfigAsset : ScriptableObject
{
    [SerializeField] private string saveFileName;
    [SerializeField] private string initialUnlockedLevelId;

    public string SaveFileName => saveFileName;
    public string InitialUnlockedLevelId => initialUnlockedLevelId;
}
