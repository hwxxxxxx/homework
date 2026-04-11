using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Global Runtime Config", fileName = "GlobalRuntimeConfig")]
public class GlobalRuntimeConfigAsset : ScriptableObject
{
    [Header("Core")]
    [SerializeField] private CombatConfigAsset combatConfig;
    [SerializeField] private RuntimeNodeConfigAsset runtimeNodeConfig;

    [Header("Flow")]
    [SerializeField] private FlowConfigAsset flowConfig;

    [Header("Progress")]
    [SerializeField] private ProgressConfigAsset progressConfig;

    [Header("UI")]
    [SerializeField] private RuntimeUiConfigAsset runtimeUiConfig;
    [SerializeField] private UiTextConfigAsset uiTextConfig;

    [Header("Audio")]
    [SerializeField] private AudioConfigAsset audioConfig;

    public void Install()
    {
        ValidateReferences();
        CombatConfigProvider.Configure(combatConfig);
        RuntimeNodeConfigProvider.Configure(runtimeNodeConfig);
        FlowConfigProvider.Configure(flowConfig);
        ProgressConfigProvider.Configure(progressConfig);
        RuntimeUiConfigProvider.Configure(runtimeUiConfig);
        UiTextConfigProvider.Configure(uiTextConfig);
        AudioConfigProvider.Configure(audioConfig);
    }

    private void ValidateReferences()
    {
        if (combatConfig == null ||
            runtimeNodeConfig == null ||
            flowConfig == null ||
            progressConfig == null ||
            runtimeUiConfig == null ||
            uiTextConfig == null ||
            audioConfig == null)
        {
            throw new InvalidOperationException("GlobalRuntimeConfigAsset has unassigned required config references.");
        }
    }
}
