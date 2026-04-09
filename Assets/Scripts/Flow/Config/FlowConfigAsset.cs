using UnityEngine;

[CreateAssetMenu(menuName = "Game/Flow/Flow Config", fileName = "FlowConfig")]
public class FlowConfigAsset : ScriptableObject
{
    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneId;
    [SerializeField] private string baseSceneId;
    [SerializeField] private string mainMenuSceneName;
    [SerializeField] private string persistentSceneName;
    [SerializeField] private string baseSceneName;

    [Header("Loading Messages")]
    [SerializeField] private string loadingDefaultMessage;
    [SerializeField] private string enteringBaseMessage;
    [SerializeField] private string preparingRunMessage;
    [SerializeField] private string returnToBaseMessage;
    [SerializeField] private string returnToMainMenuMessage;

    public string MainMenuSceneId => mainMenuSceneId;
    public string BaseSceneId => baseSceneId;
    public string MainMenuSceneName => mainMenuSceneName;
    public string PersistentSceneName => persistentSceneName;
    public string BaseSceneName => baseSceneName;
    public string LoadingDefaultMessage => loadingDefaultMessage;
    public string EnteringBaseMessage => enteringBaseMessage;
    public string PreparingRunMessage => preparingRunMessage;
    public string ReturnToBaseMessage => returnToBaseMessage;
    public string ReturnToMainMenuMessage => returnToMainMenuMessage;
}
