using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Game/UI/Runtime UI Config", fileName = "RuntimeUiConfig")]
public class RuntimeUiConfigAsset : ScriptableObject
{
    [SerializeField] private PanelSettings loadingScreenPanelSettings;

    public PanelSettings LoadingScreenPanelSettings => loadingScreenPanelSettings;
}
