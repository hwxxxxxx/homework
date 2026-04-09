using System.Collections;
using UnityEngine;

public class BootEntry : MonoBehaviour
{
    private bool started;

    private void Start()
    {
        if (started)
        {
            return;
        }

        started = true;
        StartCoroutine(BootRoutine());
    }

    private IEnumerator BootRoutine()
    {
        FlowConfigAsset flowConfig = FlowConfigProvider.Config;
        yield return SceneTransitionOrchestrator.EnsureSceneLoadedAtBoot(flowConfig.PersistentSceneName);

        if (RuntimeShell.Instance == null || RuntimeShell.Instance.GameFlowOrchestrator == null)
        {
            throw new System.InvalidOperationException("RuntimeShell or GameFlowOrchestrator is not available after loading Persistent scene.");
        }

        RuntimeShell.Instance.GameFlowOrchestrator.EnterMainMenuFromBoot();
    }
}
