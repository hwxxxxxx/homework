using System;
using System.Collections.Generic;
using UnityEngine;

public class RunContextService : MonoBehaviour
{
    [SerializeField] private ProgressService progressService;
    [SerializeField] private LevelDefinitionAsset defaultLevel;

    private readonly Dictionary<FragmentType, int> runtimeDrops = new Dictionary<FragmentType, int>();

    public LevelDefinitionAsset SelectedLevel { get; private set; }
    public RunConfigAsset ActiveRunConfig => SelectedLevel != null ? SelectedLevel.RunConfig : null;
    public bool IsRunActive { get; private set; }
    public bool? LastRunWon { get; private set; }
    public IReadOnlyDictionary<FragmentType, int> RuntimeDrops => runtimeDrops;

    public event Action OnRunContextChanged;

    private void Awake()
    {
        if (defaultLevel != null)
        {
            SetSelectedLevel(defaultLevel);
        }
    }

    public void SetSelectedLevel(LevelDefinitionAsset levelDefinition)
    {
        SelectedLevel = levelDefinition;
        if (progressService != null && SelectedLevel != null)
        {
            progressService.SetLastSelectedLevel(SelectedLevel.LevelId);
        }

        OnRunContextChanged?.Invoke();
    }

    public void BeginRun()
    {
        runtimeDrops.Clear();
        IsRunActive = true;
        LastRunWon = null;
        OnRunContextChanged?.Invoke();
    }

    public void RecordDrop(FragmentType fragmentType, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (!runtimeDrops.ContainsKey(fragmentType))
        {
            runtimeDrops[fragmentType] = 0;
        }

        runtimeDrops[fragmentType] += amount;
        OnRunContextChanged?.Invoke();
    }

    public void CompleteRun(bool won)
    {
        IsRunActive = false;
        LastRunWon = won;

        if (progressService != null)
        {
            foreach (KeyValuePair<FragmentType, int> pair in runtimeDrops)
            {
                progressService.AddFragment(pair.Key, pair.Value);
            }
        }

        OnRunContextChanged?.Invoke();
    }
}
