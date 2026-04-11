public static class ProgressConfigProvider
{
    private static ProgressConfigAsset cached;

    public static void Configure(ProgressConfigAsset config)
    {
        if (config == null)
        {
            throw new System.ArgumentNullException(nameof(config));
        }

        cached = config;
    }

    public static ProgressConfigAsset Config =>
        cached != null
            ? cached
            : throw new System.InvalidOperationException("ProgressConfigProvider is not configured. Install GlobalRuntimeConfigAsset during boot.");

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        cached = null;
    }
}
