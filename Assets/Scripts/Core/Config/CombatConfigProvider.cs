using UnityEngine;

public static class CombatConfigProvider
{
    private const string ResourcePath = "Combat/CombatConfig";
    private static CombatConfigAsset cached;

    public static CombatConfigAsset Config
    {
        get
        {
            if (cached != null)
            {
                return cached;
            }

            cached = Resources.Load<CombatConfigAsset>(ResourcePath);
            if (cached == null)
            {
                Debug.LogError($"Missing CombatConfigAsset at Resources/{ResourcePath}.asset");
            }

            return cached;
        }
    }
}
