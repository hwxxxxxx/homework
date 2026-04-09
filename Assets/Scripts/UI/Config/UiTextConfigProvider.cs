using UnityEngine;

public static class UiTextConfigProvider
{
    private const string ResourcePath = "UI/UiTextConfig";
    private static UiTextConfigAsset cached;

    public static UiTextConfigAsset Config
    {
        get
        {
            if (cached != null)
            {
                return cached;
            }

            cached = Resources.Load<UiTextConfigAsset>(ResourcePath);
            if (cached == null)
            {
                Debug.LogError($"Missing UiTextConfigAsset at Resources/{ResourcePath}.asset");
            }

            return cached;
        }
    }
}
