public static class FlowConfigProvider
{
    private static FlowConfigAsset cached;

    public static void Configure(FlowConfigAsset config)
    {
        if (config == null)
        {
            throw new System.ArgumentNullException(nameof(config));
        }

        cached = config;
    }

    public static FlowConfigAsset Config =>
        cached != null
            ? cached
            : throw new System.InvalidOperationException("FlowConfigProvider is not configured. Install GlobalRuntimeConfigAsset during boot.");

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        cached = null;
    }
}
