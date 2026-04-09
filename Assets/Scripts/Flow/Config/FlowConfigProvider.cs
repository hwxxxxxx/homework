using UnityEngine;

public static class FlowConfigProvider
{
    private const string ResourcePath = "Flow/FlowConfig";
    private static FlowConfigAsset cached;

    public static FlowConfigAsset Config
    {
        get
        {
            if (cached != null)
            {
                return cached;
            }

            cached = Resources.Load<FlowConfigAsset>(ResourcePath);
            if (cached == null)
            {
                Debug.LogError($"Missing FlowConfigAsset at Resources/{ResourcePath}.asset");
            }

            return cached;
        }
    }
}
