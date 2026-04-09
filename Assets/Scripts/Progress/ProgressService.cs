using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class ProgressService : MonoBehaviour
{
    private ProgressSaveData data;

    public event Action OnProgressChanged;

    public ProgressSaveData Data => data;

    private void Awake()
    {
        LoadOrCreate();
    }

    public int GetFragment(FragmentType fragmentType)
    {
        switch (fragmentType)
        {
            case FragmentType.Body:
                return data.bodyFragments;
            case FragmentType.Soul:
                return data.soulFragments;
            case FragmentType.Memory:
                return data.memoryFragments;
            default:
                return 0;
        }
    }

    public void AddFragment(FragmentType fragmentType, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        switch (fragmentType)
        {
            case FragmentType.Body:
                data.bodyFragments += amount;
                break;
            case FragmentType.Soul:
                data.soulFragments += amount;
                break;
            case FragmentType.Memory:
                data.memoryFragments += amount;
                break;
        }

        Save();
    }

    public bool TryConsumeFragment(FragmentType fragmentType, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        int current = GetFragment(fragmentType);
        if (current < amount)
        {
            return false;
        }

        switch (fragmentType)
        {
            case FragmentType.Body:
                data.bodyFragments -= amount;
                break;
            case FragmentType.Soul:
                data.soulFragments -= amount;
                break;
            case FragmentType.Memory:
                data.memoryFragments -= amount;
                break;
        }

        Save();
        return true;
    }

    public bool IsLevelUnlocked(string levelId)
    {
        if (string.IsNullOrWhiteSpace(levelId))
        {
            return false;
        }

        return data.unlockedLevelIds.Contains(levelId);
    }

    public bool TryUnlockLevel(string levelId)
    {
        if (string.IsNullOrWhiteSpace(levelId) || data.unlockedLevelIds.Contains(levelId))
        {
            return false;
        }

        data.unlockedLevelIds.Add(levelId);
        Save();
        EventBus.Publish(new LevelUnlockedEvent(levelId));
        return true;
    }

    public bool TryUnlockLevelWithCost(RunCatalogAsset.RunEntry runEntry)
    {
        if (runEntry == null)
        {
            return false;
        }

        if (!ArePrerequisitesMet(runEntry.PrerequisiteRunIds))
        {
            return false;
        }

        int costAmount = Mathf.Max(0, runEntry.UnlockCostAmount);
        if (costAmount > 0 && GetFragment(runEntry.UnlockCostType) < costAmount)
        {
            return false;
        }

        if (IsLevelUnlocked(runEntry.RunId))
        {
            return false;
        }

        if (costAmount > 0)
        {
            ConsumeFragmentWithoutSave(runEntry.UnlockCostType, costAmount);
        }

        data.unlockedLevelIds.Add(runEntry.RunId);
        Save();
        EventBus.Publish(new LevelUnlockedEvent(runEntry.RunId));
        return true;
    }

    public bool IsAchievementUnlocked(AchievementId achievementId)
    {
        if (data.unlockedAchievementIds == null)
        {
            return false;
        }

        return data.unlockedAchievementIds.Contains(ToAchievementKey(achievementId));
    }

    public bool TryUnlockAchievement(AchievementId achievementId)
    {
        string achievementKey = ToAchievementKey(achievementId);
        if (data.unlockedAchievementIds.Contains(achievementKey))
        {
            return false;
        }

        data.unlockedAchievementIds.Add(achievementKey);
        Save();
        return true;
    }

    public void SetLastSelectedLevel(string levelId)
    {
        data.lastSelectedLevelId = levelId ?? string.Empty;
        Save();
    }

    public string GetLastSelectedLevelId()
    {
        return data.lastSelectedLevelId;
    }

    private void LoadOrCreate()
    {
        ProgressConfigAsset config = ProgressConfigProvider.Config;
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            data = CreateDefaultData(config);
            Save();
            return;
        }

        string json = File.ReadAllText(path);
        data = JsonUtility.FromJson<ProgressSaveData>(json);
        if (data == null)
        {
            data = CreateDefaultData(config);
            Save();
            return;
        }

        if (data.unlockedLevelIds == null)
        {
            data.unlockedLevelIds = new System.Collections.Generic.List<string>();
        }

        if (data.unlockedAchievementIds == null)
        {
            data.unlockedAchievementIds = new System.Collections.Generic.List<string>();
        }

        if (data.unlockedLevelIds.Count == 0 && !string.IsNullOrWhiteSpace(config.InitialUnlockedLevelId))
        {
            data.unlockedLevelIds.Add(config.InitialUnlockedLevelId);
        }

        if (string.IsNullOrWhiteSpace(data.lastSelectedLevelId))
        {
            data.lastSelectedLevelId = data.unlockedLevelIds.Count > 0 ? data.unlockedLevelIds[0] : config.InitialUnlockedLevelId;
        }

        if (!string.IsNullOrWhiteSpace(data.lastSelectedLevelId) && !data.unlockedLevelIds.Contains(data.lastSelectedLevelId))
        {
            data.unlockedLevelIds.Add(data.lastSelectedLevelId);
        }
    }

    private ProgressSaveData CreateDefaultData(ProgressConfigAsset config)
    {
        var defaultData = new ProgressSaveData();
        if (!string.IsNullOrWhiteSpace(config.InitialUnlockedLevelId))
        {
            defaultData.unlockedLevelIds.Add(config.InitialUnlockedLevelId);
            defaultData.lastSelectedLevelId = config.InitialUnlockedLevelId;
        }

        return defaultData;
    }

    private void Save()
    {
        string path = GetSavePath();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        OnProgressChanged?.Invoke();
    }

    private bool ArePrerequisitesMet(IReadOnlyList<string> prerequisiteLevelIds)
    {
        if (prerequisiteLevelIds == null || prerequisiteLevelIds.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < prerequisiteLevelIds.Count; i++)
        {
            string prerequisiteLevelId = prerequisiteLevelIds[i];
            if (string.IsNullOrWhiteSpace(prerequisiteLevelId))
            {
                continue;
            }

            if (!IsLevelUnlocked(prerequisiteLevelId))
            {
                return false;
            }
        }

        return true;
    }

    private void ConsumeFragmentWithoutSave(FragmentType fragmentType, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        switch (fragmentType)
        {
            case FragmentType.Body:
                data.bodyFragments -= amount;
                break;
            case FragmentType.Soul:
                data.soulFragments -= amount;
                break;
            case FragmentType.Memory:
                data.memoryFragments -= amount;
                break;
        }
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, ProgressConfigProvider.Config.SaveFileName);
    }

    private static string ToAchievementKey(AchievementId achievementId)
    {
        return achievementId.ToString();
    }
}
