using UnityEngine;

public class GamePauseController : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }
}

public static class CursorPolicyService
{
    private static readonly System.Collections.Generic.HashSet<string> UiOwners =
        new System.Collections.Generic.HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnLoad()
    {
        UiOwners.Clear();
        ApplyCursor(false);
    }

    public static void AcquireUiCursor(string owner)
    {
        if (string.IsNullOrWhiteSpace(owner))
        {
            return;
        }

        if (UiOwners.Add(owner))
        {
            ApplyCursor(true);
        }
    }

    public static void ReleaseUiCursor(string owner)
    {
        if (string.IsNullOrWhiteSpace(owner))
        {
            return;
        }

        if (UiOwners.Remove(owner) && UiOwners.Count == 0)
        {
            ApplyCursor(false);
        }
    }

    public static void ForceGameplayCursor()
    {
        UiOwners.Clear();
        ApplyCursor(false);
    }

    private static void ApplyCursor(bool uiVisible)
    {
        Cursor.visible = uiVisible;
        Cursor.lockState = uiVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
