public static class UiTextConfigProvider
{
    private static UiTextConfigAsset cached;

    public static void Configure(UiTextConfigAsset config)
    {
        if (config == null)
        {
            throw new System.ArgumentNullException(nameof(config));
        }

        cached = config;
    }

    public static UiTextConfigAsset Config =>
        cached != null
            ? cached
            : throw new System.InvalidOperationException("UiTextConfigProvider is not configured. Install GlobalRuntimeConfigAsset during boot.");

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        cached = null;
    }
}
