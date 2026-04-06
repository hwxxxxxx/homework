using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Flow/Level Definition", fileName = "LevelDefinition")]
public class LevelDefinitionAsset : ScriptableObject
{
    [SerializeField] private string levelId = "body_1";
    [SerializeField] private string displayName = "Level";
    [SerializeField] private string sceneName = "GameScene";
    [SerializeField] private RunConfigAsset runConfig;
    [SerializeField] private FragmentType unlockCostType = FragmentType.Body;
    [SerializeField] private int unlockCostAmount;
    [SerializeField] private List<string> prerequisiteLevelIds = new List<string>();

    public string LevelId => levelId;
    public string DisplayName => displayName;
    public string SceneName => sceneName;
    public RunConfigAsset RunConfig => runConfig;
    public FragmentType UnlockCostType => unlockCostType;
    public int UnlockCostAmount => unlockCostAmount;
    public IReadOnlyList<string> PrerequisiteLevelIds => prerequisiteLevelIds;
}
