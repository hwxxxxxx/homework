public static class CombatConfigProvider
{
    private static CombatConfigAsset cached;

    public static void Configure(CombatConfigAsset config)
    {
        if (config == null)
        {
            throw new System.ArgumentNullException(nameof(config));
        }

        cached = config;
    }

    public static CombatConfigAsset Config =>
        cached != null
            ? cached
            : throw new System.InvalidOperationException("CombatConfigProvider is not configured. Install GlobalRuntimeConfigAsset during boot.");

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        cached = null;
    }
}
