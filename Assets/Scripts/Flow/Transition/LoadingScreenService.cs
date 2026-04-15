using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public static class LoadingScreenService
{
    private static Font runtimeFont;

    private static LoadingScreenRunner runner;
    private static UIDocument document;
    private static PanelSettings panelSettings;
    private static VisualElement root;
    private static Label statusText;
    private static VisualElement progressFill;
    private static bool isTransitionInProgress;

    public static bool IsTransitionInProgress => isTransitionInProgress;

    public static void BeginTransition(string message)
    {
        isTransitionInProgress = true;
        Show(message);
    }

    public static void EndTransition()
    {
        SetProgress(1f);
        Hide();
    }

    public static void Show(string message)
    {
        EnsureUi();
        SetStatus(message);
        SetProgress(0f);
        SetVisible(true);
    }

    public static void SetProgress(float normalizedProgress)
    {
        float value = Mathf.Clamp01(normalizedProgress);
        progressFill.style.width = Length.Percent(value * 100f);
    }

    public static void Hide()
    {
        runner.StartCoroutine(HideAtEndOfFrameRoutine());
    }

    private static IEnumerator HideAtEndOfFrameRoutine()
    {
        yield return null;
        HideImmediate();
    }

    private static void HideImmediate()
    {
        root.style.display = DisplayStyle.None;
        SetStatus(string.Empty);
        SetProgress(0f);
        isTransitionInProgress = false;
        Object.Destroy(runner.gameObject);
        runner = null;
        document = null;
        panelSettings = null;
        root = null;
        progressFill = null;
        statusText = null;
    }

    private static void SetVisible(bool visible)
    {
        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static void SetStatus(string message)
    {
        statusText.text = message;
    }

    private static void EnsureUi()
    {
        if (runner != null)
        {
            return;
        }

        runtimeFont = RuntimeUiConfigProvider.Config.RuntimeFont;

        GameObject root = new GameObject(RuntimeNodeConfigProvider.Config.LoadingScreenRootName);
        Object.DontDestroyOnLoad(root);
        runner = root.AddComponent<LoadingScreenRunner>();

        panelSettings = Object.Instantiate(RuntimeUiConfigProvider.Config.LoadingScreenPanelSettings);
        panelSettings.sortingOrder = 6000;

        document = root.AddComponent<UIDocument>();
        document.panelSettings = panelSettings;

        VisualElement visualRoot = document.rootVisualElement;
        visualRoot.style.flexGrow = 1f;
        visualRoot.style.position = Position.Absolute;
        visualRoot.style.left = 0f;
        visualRoot.style.top = 0f;
        visualRoot.style.right = 0f;
        visualRoot.style.bottom = 0f;

        LoadingScreenService.root = new VisualElement();
        LoadingScreenService.root.style.position = Position.Absolute;
        LoadingScreenService.root.style.left = 0f;
        LoadingScreenService.root.style.top = 0f;
        LoadingScreenService.root.style.right = 0f;
        LoadingScreenService.root.style.bottom = 0f;
        LoadingScreenService.root.style.backgroundColor = new Color(0f, 0f, 0f, 0.92f);
        LoadingScreenService.root.style.alignItems = Align.Center;
        LoadingScreenService.root.style.justifyContent = Justify.Center;
        LoadingScreenService.root.style.display = DisplayStyle.None;

        VisualElement content = new VisualElement();
        content.style.width = 760f;
        content.style.alignItems = Align.Stretch;

        statusText = new Label(FlowConfigProvider.Config.LoadingDefaultMessage);
        statusText.style.unityFont = runtimeFont;
        statusText.style.color = Color.white;
        statusText.style.fontSize = 34;
        statusText.style.unityTextAlign = TextAnchor.MiddleCenter;
        statusText.style.marginBottom = 20f;
        content.Add(statusText);

        VisualElement progressBg = new VisualElement();
        progressBg.style.height = 30f;
        progressBg.style.backgroundColor = new Color(1f, 1f, 1f, 0.14f);
        progressBg.style.paddingLeft = 4f;
        progressBg.style.paddingRight = 4f;
        progressBg.style.paddingTop = 4f;
        progressBg.style.paddingBottom = 4f;

        progressFill = new VisualElement();
        progressFill.style.height = Length.Percent(100f);
        progressFill.style.width = Length.Percent(0f);
        progressFill.style.backgroundColor = new Color(0.2f, 0.78f, 0.37f, 1f);
        progressBg.Add(progressFill);

        content.Add(progressBg);
        LoadingScreenService.root.Add(content);
        visualRoot.Add(LoadingScreenService.root);
    }

    private sealed class LoadingScreenRunner : MonoBehaviour
    {
    }
}
