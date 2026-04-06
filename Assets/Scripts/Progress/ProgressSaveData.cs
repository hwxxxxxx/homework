using System;
using System.Collections.Generic;

[Serializable]
public class ProgressSaveData
{
    public int schemaVersion = 1;
    public string lastSelectedLevelId = string.Empty;
    public List<string> unlockedLevelIds = new List<string>();
    public List<string> unlockedAchievementIds = new List<string>();
    public int bodyFragments;
    public int soulFragments;
    public int memoryFragments;
}
