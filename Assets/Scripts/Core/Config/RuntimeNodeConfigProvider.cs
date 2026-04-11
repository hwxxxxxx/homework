public static class RuntimeNodeConfigProvider
{
    private static RuntimeNodeConfigAsset cached;

    public static void Configure(RuntimeNodeConfigAsset config)
    {
        if (config == null)
        {
            throw new System.ArgumentNullException(nameof(config));
        }

        cached = config;
    }

    public static RuntimeNodeConfigAsset Config =>
        cached != null
            ? cached
            : throw new System.InvalidOperationException("RuntimeNodeConfigProvider is not configured. Install GlobalRuntimeConfigAsset during boot.");

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        cached = null;
    }
}
