using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Flow/Run Catalog", fileName = "RunCatalog")]
public class RunCatalogAsset : ScriptableObject
{
    [Serializable]
    public class RunEntry
    {
        [SerializeField] private string runId;
        [SerializeField] private string displayName;
        [SerializeField] private string sceneId;
        [SerializeField] private FragmentType unlockCostType;
        [SerializeField] private int unlockCostAmount;
        [SerializeField] private List<string> prerequisiteRunIds = new List<string>();

        public string RunId => runId;
        public string DisplayName => displayName;
        public string SceneId => sceneId;
        public FragmentType UnlockCostType => unlockCostType;
        public int UnlockCostAmount => unlockCostAmount;
        public IReadOnlyList<string> PrerequisiteRunIds => prerequisiteRunIds;
    }

    [SerializeField] private List<RunEntry> runs = new List<RunEntry>();

    public IReadOnlyList<RunEntry> Runs => runs;

    public bool TryGetRun(string runId, out RunEntry runEntry)
    {
        runEntry = null;
        if (string.IsNullOrWhiteSpace(runId))
        {
            return false;
        }

        for (int i = 0; i < runs.Count; i++)
        {
            RunEntry entry = runs[i];
            if (!string.Equals(entry.RunId, runId, StringComparison.Ordinal))
            {
                continue;
            }

            runEntry = entry;
            return true;
        }

        return false;
    }
}
