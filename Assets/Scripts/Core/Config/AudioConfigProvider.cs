public static class AudioConfigProvider
{
    private static AudioConfigAsset cached;

    public static void Configure(AudioConfigAsset config)
    {
        if (config == null)
        {
            throw new System.ArgumentNullException(nameof(config));
        }

        cached = config;
    }

    public static AudioConfigAsset Config =>
        cached != null
            ? cached
            : throw new System.InvalidOperationException("AudioConfigProvider is not configured. Install GlobalRuntimeConfigAsset during boot.");

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        cached = null;
    }
}
