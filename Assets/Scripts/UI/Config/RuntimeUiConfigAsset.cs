using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Game/UI/Runtime UI Config", fileName = "RuntimeUiConfig")]
public class RuntimeUiConfigAsset : ScriptableObject
{
    [SerializeField] private PanelSettings loadingScreenPanelSettings;
    [SerializeField] private Font runtimeFont;

    public PanelSettings LoadingScreenPanelSettings => loadingScreenPanelSettings;
    public Font RuntimeFont => runtimeFont;
}
