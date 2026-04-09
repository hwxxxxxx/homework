using UnityEngine;

public static class RuntimeUiConfigProvider
{
    private const string ResourcePath = "UI/RuntimeUiConfig";
    private static RuntimeUiConfigAsset cached;

    public static RuntimeUiConfigAsset Config
    {
        get
        {
            if (cached != null)
            {
                return cached;
            }

            cached = Resources.Load<RuntimeUiConfigAsset>(ResourcePath);
            if (cached == null)
            {
                Debug.LogError($"Missing RuntimeUiConfigAsset at Resources/{ResourcePath}.asset");
            }

            return cached;
        }
    }
}
