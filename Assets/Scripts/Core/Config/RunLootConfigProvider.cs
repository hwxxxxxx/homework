public static class RunLootConfigProvider
{
    private static RunLootConfigAsset cached;

    public static void Configure(RunLootConfigAsset config)
    {
        if (config == null)
        {
            throw new System.ArgumentNullException(nameof(config));
        }

        cached = config;
    }

    public static RunLootConfigAsset Config =>
        cached != null
            ? cached
            : throw new System.InvalidOperationException("RunLootConfigProvider is not configured. Install GlobalRuntimeConfigAsset during boot.");

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        cached = null;
    }
}
