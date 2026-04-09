using UnityEngine;

public class RunContextService : MonoBehaviour
{
    [SerializeField] private ProgressService progressService;

    public bool IsRunActive { get; private set; }
    public bool? LastRunWon { get; private set; }
    public string CurrentRunId { get; private set; }

    public void BeginRun(string runId)
    {
        CurrentRunId = runId ?? string.Empty;
        LastRunWon = null;
        IsRunActive = true;
    }

    public void EndRun(bool won)
    {
        LastRunWon = won;
        IsRunActive = false;
        CurrentRunId = string.Empty;
    }

    public void ResetRunState()
    {
        LastRunWon = null;
        IsRunActive = false;
        CurrentRunId = string.Empty;
    }

    public void RecordDrop(FragmentType fragmentType, int amount)
    {
        if (progressService == null)
        {
            return;
        }

        progressService.AddFragment(fragmentType, amount);
    }
}
