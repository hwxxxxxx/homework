using UnityEngine;

public static class ProgressConfigProvider
{
    private const string ResourcePath = "Progress/ProgressConfig";
    private static ProgressConfigAsset cached;

    public static ProgressConfigAsset Config
    {
        get
        {
            if (cached != null)
            {
                return cached;
            }

            cached = Resources.Load<ProgressConfigAsset>(ResourcePath);
            if (cached == null)
            {
                Debug.LogError($"Missing ProgressConfigAsset at Resources/{ResourcePath}.asset");
            }

            return cached;
        }
    }
}
