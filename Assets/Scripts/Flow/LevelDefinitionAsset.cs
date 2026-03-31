using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Flow/Level Definition", fileName = "LevelDefinition")]
public class LevelDefinitionAsset : ScriptableObject
{
    [SerializeField] private string levelId = "body_1";
    [SerializeField] private string displayName = "Level";
    [SerializeField] private RunConfigAsset runConfig;
    [SerializeField] private List<string> prerequisiteLevelIds = new List<string>();

    public string LevelId => levelId;
    public string DisplayName => displayName;
    public RunConfigAsset RunConfig => runConfig;
    public IReadOnlyList<string> PrerequisiteLevelIds => prerequisiteLevelIds;
}
