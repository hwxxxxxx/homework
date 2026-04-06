using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GlobalRuntimeBootstrap
{
    private const string RootName = "GlobalRuntimeRoot";
    private const string SystemsName = "GlobalSystems";
    private const string UiName = "GlobalUI";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureRuntimeRoot();
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GamePauseController controller = EnsureRuntimeRoot();
        if (controller == null)
        {
            return;
        }

        bool isMainMenu = scene.name == "MainMenu";
        bool isGameplayScene = IsGameplayScene(scene.name);

        if (isMainMenu)
        {
            controller.enabled = false;
            return;
        }

        controller.enabled = true;
        controller.ConfigurePauseOnlyWhenInRun(isGameplayScene);
        controller.ConfigureReturnScene(isGameplayScene ? "BaseScene_Main" : "MainMenu");
        SyncGlobalAchievementProgress();
        RemoveSceneLocalAchievementServices();
    }

    private static bool IsGameplayScene(string sceneName)
    {
        return sceneName == "GameplayCommon" ||
               sceneName == "GameScene" ||
               sceneName == "Level_Body_2" ||
               sceneName == "Level_Soul_1" ||
               sceneName == "Level_Memory_1";
    }

    private static GamePauseController EnsureRuntimeRoot()
    {
        GamePauseController existingController = Object.FindObjectOfType<GamePauseController>();
        if (existingController != null)
        {
            return existingController;
        }

        GameObject root = new GameObject(RootName);
        Object.DontDestroyOnLoad(root);

        GameObject systems = new GameObject(SystemsName);
        systems.transform.SetParent(root.transform, false);
        GameInput input = systems.AddComponent<GameInput>();
        GamePauseController controller = systems.AddComponent<GamePauseController>();
        systems.AddComponent<AchievementService>();

        GameObject uiRoot = new GameObject(UiName);
        uiRoot.transform.SetParent(root.transform, false);

        GameObject canvasObject = new GameObject("GlobalCanvas");
        canvasObject.transform.SetParent(uiRoot.transform, false);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject pausePanel = new GameObject("PausePanel",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pausePanel.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = pausePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(420f, 220f);
        panelRect.anchoredPosition = Vector2.zero;
        Image panelImage = pausePanel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.75f);

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject titleObject = new GameObject("PauseTitle",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        titleObject.transform.SetParent(pausePanel.transform, false);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(300f, 40f);
        titleRect.anchoredPosition = new Vector2(0f, -30f);
        Text titleText = titleObject.GetComponent<Text>();
        titleText.text = "Paused";
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.font = defaultFont;
        titleText.fontSize = 28;
        titleText.color = Color.white;

        GameObject buttonObject = new GameObject("ReturnButton",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(pausePanel.transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(240f, 54f);
        buttonRect.anchoredPosition = new Vector2(0f, -20f);
        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.18f, 0.45f, 0.22f, 1f);

        GameObject labelObject = new GameObject("Label",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        Text labelText = labelObject.GetComponent<Text>();
        labelText.text = "Return To Base";
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.font = defaultFont;
        labelText.fontSize = 24;
        labelText.color = Color.white;

        pausePanel.SetActive(false);

        controller.ConfigureDependencies(input, pausePanel, buttonObject.GetComponent<Button>());
        controller.ConfigurePauseOnlyWhenInRun(false);
        controller.ConfigureReturnScene("MainMenu");
        return controller;
    }

    private static void SyncGlobalAchievementProgress()
    {
        AchievementService globalAchievement = GetGlobalAchievementService();
        if (globalAchievement == null)
        {
            return;
        }

        ProgressService progress = Object.FindObjectOfType<ProgressService>();
        if (progress != null)
        {
            globalAchievement.ConfigureProgressService(progress);
        }
    }

    private static void RemoveSceneLocalAchievementServices()
    {
        AchievementService globalAchievement = GetGlobalAchievementService();
        if (globalAchievement == null)
        {
            return;
        }

        AchievementService[] all = Object.FindObjectsOfType<AchievementService>(true);
        for (int i = 0; i < all.Length; i++)
        {
            AchievementService candidate = all[i];
            if (candidate == null || candidate == globalAchievement)
            {
                continue;
            }

            Object.Destroy(candidate);
        }
    }

    private static AchievementService GetGlobalAchievementService()
    {
        AchievementService[] all = Object.FindObjectsOfType<AchievementService>(true);
        for (int i = 0; i < all.Length; i++)
        {
            AchievementService service = all[i];
            if (service == null)
            {
                continue;
            }

            Transform parent = service.transform.parent;
            if (parent != null && parent.name == SystemsName)
            {
                return service;
            }
        }

        return null;
    }
}
