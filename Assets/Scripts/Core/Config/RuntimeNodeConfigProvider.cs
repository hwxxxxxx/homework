using UnityEngine;

public static class RuntimeNodeConfigProvider
{
    private const string ResourcePath = "Runtime/RuntimeNodeConfig";
    private static RuntimeNodeConfigAsset cached;

    public static RuntimeNodeConfigAsset Config
    {
        get
        {
            if (cached != null)
            {
                return cached;
            }

            cached = Resources.Load<RuntimeNodeConfigAsset>(ResourcePath);
            if (cached == null)
            {
                Debug.LogError($"Missing RuntimeNodeConfigAsset at Resources/{ResourcePath}.asset");
            }

            return cached;
        }
    }
}
