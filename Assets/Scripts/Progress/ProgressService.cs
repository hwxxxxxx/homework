using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class ProgressService : MonoBehaviour
{
    [SerializeField] private string saveFileName = "progress_save.json";
    [SerializeField] private string initialUnlockedLevelId = "body_1";

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

    public bool TryUnlockLevelWithCost(LevelDefinitionAsset levelDefinition)
    {
        if (levelDefinition == null)
        {
            return false;
        }

        if (!ArePrerequisitesMet(levelDefinition.PrerequisiteLevelIds))
        {
            return false;
        }

        int costAmount = Mathf.Max(0, levelDefinition.UnlockCostAmount);
        if (costAmount > 0 && GetFragment(levelDefinition.UnlockCostType) < costAmount)
        {
            return false;
        }

        if (IsLevelUnlocked(levelDefinition.LevelId))
        {
            return false;
        }

        if (costAmount > 0)
        {
            ConsumeFragmentWithoutSave(levelDefinition.UnlockCostType, costAmount);
        }

        data.unlockedLevelIds.Add(levelDefinition.LevelId);
        Save();
        EventBus.Publish(new LevelUnlockedEvent(levelDefinition.LevelId));
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

    private void LoadOrCreate()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            data = CreateDefaultData();
            Save();
            return;
        }

        string json = File.ReadAllText(path);
        data = JsonUtility.FromJson<ProgressSaveData>(json);
        if (data == null)
        {
            data = CreateDefaultData();
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

        if (data.unlockedLevelIds.Count == 0 && !string.IsNullOrWhiteSpace(initialUnlockedLevelId))
        {
            data.unlockedLevelIds.Add(initialUnlockedLevelId);
        }
    }

    private ProgressSaveData CreateDefaultData()
    {
        var defaultData = new ProgressSaveData();
        if (!string.IsNullOrWhiteSpace(initialUnlockedLevelId))
        {
            defaultData.unlockedLevelIds.Add(initialUnlockedLevelId);
            defaultData.lastSelectedLevelId = initialUnlockedLevelId;
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
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }

    private static string ToAchievementKey(AchievementId achievementId)
    {
        return achievementId.ToString();
    }
}
