public static class RunSceneRequest
{
    private static string pendingLevelSceneName;

    public static string PendingLevelSceneName => pendingLevelSceneName;

    public static void SetPendingLevelScene(string sceneName)
    {
        pendingLevelSceneName = sceneName;
    }

    public static void Clear()
    {
        pendingLevelSceneName = null;
    }
}
