using System;
using System.IO;
using UnityEngine;

public static class SaveSlotService
{
    public readonly struct SaveSlotSummary
    {
        public SaveSlotSummary(int slotId, bool hasData, string lastSelectedLevelId, int unlockedLevelCount, int completedRunCount)
        {
            SlotId = slotId;
            HasData = hasData;
            LastSelectedLevelId = lastSelectedLevelId;
            UnlockedLevelCount = unlockedLevelCount;
            CompletedRunCount = completedRunCount;
        }

        public int SlotId { get; }
        public bool HasData { get; }
        public string LastSelectedLevelId { get; }
        public int UnlockedLevelCount { get; }
        public int CompletedRunCount { get; }
    }

    public const int SlotCount = 3;

    private static int activeSlotId;

    public static event Action<int> ActiveSlotChanged;

    public static int ActiveSlotId => activeSlotId;
    public static bool HasActiveSlot => activeSlotId >= 1 && activeSlotId <= SlotCount;

    public static void SelectSlot(int slotId)
    {
        if (slotId < 1 || slotId > SlotCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slotId), $"slotId must be in [1, {SlotCount}].");
        }

        activeSlotId = slotId;
        ActiveSlotChanged?.Invoke(activeSlotId);
    }

    public static SaveSlotSummary GetSlotSummary(int slotId)
    {
        string path = GetSlotSavePath(slotId);
        if (!File.Exists(path))
        {
            return new SaveSlotSummary(slotId, false, string.Empty, 0, 0);
        }

        string json = File.ReadAllText(path);
        ProgressSaveData saveData = JsonUtility.FromJson<ProgressSaveData>(json);

        return new SaveSlotSummary(
            slotId,
            true,
            saveData.lastSelectedLevelId,
            saveData.unlockedLevelIds.Count,
            saveData.completedRunIds.Count
        );
    }

    public static string GetSlotSavePath(int slotId)
    {
        if (slotId < 1 || slotId > SlotCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slotId), $"slotId must be in [1, {SlotCount}].");
        }

        string fileName = ProgressConfigProvider.Config.SaveFileName;
        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        string slotFileName = $"{baseName}_slot{slotId}{extension}";
        return Path.Combine(Application.persistentDataPath, slotFileName);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        activeSlotId = 0;
        ActiveSlotChanged = null;
    }
}
