using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LoadingScreenService
{
    private const string RootName = "GlobalLoadingScreenRoot";

    private static LoadingScreenRunner runner;
    private static CanvasGroup canvasGroup;
    private static Slider progressSlider;
    private static Text statusText;
    private static bool isTransitionInProgress;

    public static bool IsTransitionInProgress => isTransitionInProgress;

    public static bool TryLoadSceneSingle(string sceneName, string message, bool keepVisibleAfterSceneLoad)
    {
        EnsureUi();
        runner.StartCoroutine(LoadSceneSingleRoutine(sceneName, message, keepVisibleAfterSceneLoad));
        return true;
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
        if (progressSlider != null)
        {
            progressSlider.value = Mathf.Clamp01(normalizedProgress);
        }
    }

    public static void Hide()
    {
        runner.StartCoroutine(HideAtEndOfFrameRoutine());
    }

    private static IEnumerator LoadSceneSingleRoutine(string sceneName, string message, bool keepVisibleAfterSceneLoad)
    {
        isTransitionInProgress = true;
        Show(message);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!loadOperation.isDone)
        {
            float normalized = Mathf.Clamp01(loadOperation.progress / 0.9f);
            float maxPrimaryProgress = keepVisibleAfterSceneLoad ? 0.15f : 0.9f;
            SetProgress(normalized * maxPrimaryProgress);
            yield return null;
        }

        if (keepVisibleAfterSceneLoad)
        {
            SetProgress(0.15f);
        }
        else
        {
            SetProgress(1f);
            isTransitionInProgress = false;
            Hide();
        }

        yield return null;
    }

    private static IEnumerator HideAtEndOfFrameRoutine()
    {
        yield return null;
        HideImmediate();
    }

    private static void HideImmediate()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        SetStatus(string.Empty);
        SetProgress(0f);
        isTransitionInProgress = false;
        Object.Destroy(runner.gameObject);
        runner = null;
        canvasGroup = null;
        progressSlider = null;
        statusText = null;
    }

    private static void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
    }

    private static void SetStatus(string message)
    {
        statusText.text = string.IsNullOrWhiteSpace(message) ? "Loading..." : message;
    }

    private static void EnsureUi()
    {
        if (runner != null)
        {
            return;
        }

        GameObject root = new GameObject(RootName);
        Object.DontDestroyOnLoad(root);
        runner = root.AddComponent<LoadingScreenRunner>();

        GameObject canvasObject = new GameObject("LoadingCanvas",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasObject.transform.SetParent(root.transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 6000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGroup = canvasObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        CreateBackground(canvasObject.transform);
        statusText = CreateStatusText(canvasObject.transform);
        progressSlider = CreateProgressBar(canvasObject.transform);
    }

    private static void CreateBackground(Transform parent)
    {
        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        background.transform.SetParent(parent, false);

        RectTransform rect = background.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = background.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.92f);
    }

    private static Text CreateStatusText(Transform parent)
    {
        GameObject textObject = new GameObject("StatusText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(760f, 80f);
        rect.anchoredPosition = new Vector2(0f, 40f);

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 34;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = "Loading...";
        return text;
    }

    private static Slider CreateProgressBar(Transform parent)
    {
        GameObject sliderObject = new GameObject("ProgressBar",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
        sliderObject.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.sizeDelta = new Vector2(760f, 30f);
        sliderRect.anchoredPosition = new Vector2(0f, -40f);

        Image background = sliderObject.GetComponent<Image>();
        background.color = new Color(1f, 1f, 1f, 0.14f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.value = 0f;
        slider.transition = Selectable.Transition.None;

        GameObject fillArea = new GameObject("FillArea", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(4f, 4f);
        fillAreaRect.offsetMax = new Vector2(-4f, -4f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fill.GetComponent<Image>();
        fillImage.color = new Color(0.2f, 0.78f, 0.37f, 1f);

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.handleRect = null;
        return slider;
    }

    private sealed class LoadingScreenRunner : MonoBehaviour
    {
    }
}
