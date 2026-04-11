using System.Collections;
using UnityEngine;

public class BootEntry : MonoBehaviour
{
    [SerializeField] private GlobalRuntimeConfigAsset globalRuntimeConfig;

    private bool started;

    private void Awake()
    {
        if (globalRuntimeConfig == null)
        {
            throw new System.InvalidOperationException("BootEntry requires GlobalRuntimeConfigAsset reference.");
        }

        globalRuntimeConfig.Install();
    }

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
