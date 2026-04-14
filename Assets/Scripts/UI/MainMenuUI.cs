using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    private const string CursorOwner = "MainMenuUI";

    [SerializeField] private GameFlowOrchestrator flowOrchestrator;

    [Header("Main Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;

    [Header("Resolution Controls")]
    [SerializeField] private Button resolutionPrevButton;
    [SerializeField] private Button resolutionNextButton;
    [SerializeField] private TMP_Text resolutionValueText;

    [Header("Display Mode Controls")]
    [SerializeField] private Button displayModePrevButton;
    [SerializeField] private Button displayModeNextButton;
    [SerializeField] private TMP_Text displayModeValueText;

    [Header("Audio Controls")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TMP_Text musicValueText;

    private readonly List<Vector2Int> availableResolutions = new List<Vector2Int>();
    private int selectedResolutionIndex;
    private FullScreenMode selectedDisplayMode;

    public void ConfigureRuntimeServices(GameFlowOrchestrator runtimeFlowOrchestrator)
    {
        flowOrchestrator = runtimeFlowOrchestrator;
    }

    private void Awake()
    {
        AutoWireReferences();
        ValidateBindings();
        BindButtonActions();
        InitializeSettingsValues();
    }

    private void OnEnable()
    {
        Time.timeScale = 1f;
        CursorPolicyService.AcquireUiCursor(CursorOwner);
    }

    private void OnDisable()
    {
        CursorPolicyService.ReleaseUiCursor(CursorOwner);
        AudioRuntimeService audioService = ResolveAudioService();
        if (audioService != null)
        {
            audioService.SaveVolumeSettings();
        }
    }

    private void BindButtonActions()
    {
        startButton.onClick.AddListener(HandleStartGameClicked);
        settingsButton.onClick.AddListener(HandleOpenSettingsClicked);
        quitButton.onClick.AddListener(HandleQuitClicked);
        closeSettingsButton.onClick.AddListener(HandleCloseSettingsClicked);
        resolutionPrevButton.onClick.AddListener(HandleResolutionPrevClicked);
        resolutionNextButton.onClick.AddListener(HandleResolutionNextClicked);
        displayModePrevButton.onClick.AddListener(HandleDisplayModePrevClicked);
        displayModeNextButton.onClick.AddListener(HandleDisplayModeNextClicked);
        musicVolumeSlider.onValueChanged.AddListener(HandleMusicVolumeChanged);
    }

    private void InitializeSettingsValues()
    {
        LoadResolutionOptions();
        selectedDisplayMode = Screen.fullScreenMode;
        UpdateResolutionLabel();
        UpdateDisplayModeLabel();

        AudioRuntimeService audioService = ResolveAudioService();
        float currentVolume = audioService != null ? audioService.GetMasterVolume() : 1f;
        musicVolumeSlider.SetValueWithoutNotify(currentVolume);
        UpdateMusicValueLabel(currentVolume);
        settingsPanel.SetActive(false);
    }

    private void LoadResolutionOptions()
    {
        Resolution[] resolutions = Screen.resolutions;
        availableResolutions.Clear();

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution candidate = resolutions[i];
            bool exists = false;
            for (int j = 0; j < availableResolutions.Count; j++)
            {
                if (availableResolutions[j].x == candidate.width && availableResolutions[j].y == candidate.height)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                availableResolutions.Add(new Vector2Int(candidate.width, candidate.height));
            }
        }

        selectedResolutionIndex = 0;
        int currentWidth = Screen.currentResolution.width;
        int currentHeight = Screen.currentResolution.height;
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].x == currentWidth && availableResolutions[i].y == currentHeight)
            {
                selectedResolutionIndex = i;
                break;
            }
        }
    }

    private void HandleStartGameClicked()
    {
        flowOrchestrator.EnterBase();
    }

    public void OnStartGame()
    {
        HandleStartGameClicked();
    }

    private void HandleOpenSettingsClicked()
    {
        settingsPanel.SetActive(true);
    }

    private void HandleCloseSettingsClicked()
    {
        AudioRuntimeService audioService = ResolveAudioService();
        if (audioService != null)
        {
            audioService.SaveVolumeSettings();
        }

        settingsPanel.SetActive(false);
    }

    private void HandleQuitClicked()
    {
        Application.Quit();
    }

    public void OnQuitGame()
    {
        HandleQuitClicked();
    }

    private void HandleResolutionPrevClicked()
    {
        if (availableResolutions.Count == 0)
        {
            return;
        }

        selectedResolutionIndex = (selectedResolutionIndex - 1 + availableResolutions.Count) % availableResolutions.Count;
        ApplyDisplaySettings();
    }

    private void HandleResolutionNextClicked()
    {
        if (availableResolutions.Count == 0)
        {
            return;
        }

        selectedResolutionIndex = (selectedResolutionIndex + 1) % availableResolutions.Count;
        ApplyDisplaySettings();
    }

    private void HandleDisplayModePrevClicked()
    {
        selectedDisplayMode = GetRelativeDisplayMode(-1);
        ApplyDisplaySettings();
    }

    private void HandleDisplayModeNextClicked()
    {
        selectedDisplayMode = GetRelativeDisplayMode(1);
        ApplyDisplaySettings();
    }

    private void HandleMusicVolumeChanged(float value)
    {
        UpdateMusicValueLabel(value);
        AudioRuntimeService audioService = ResolveAudioService();
        if (audioService != null)
        {
            audioService.SetMasterVolume(value);
        }
    }

    private void ApplyDisplaySettings()
    {
        if (availableResolutions.Count == 0)
        {
            return;
        }

        Vector2Int target = availableResolutions[selectedResolutionIndex];
        Screen.SetResolution(target.x, target.y, selectedDisplayMode);
        UpdateResolutionLabel();
        UpdateDisplayModeLabel();
    }

    private void UpdateResolutionLabel()
    {
        if (availableResolutions.Count == 0)
        {
            resolutionValueText.text = "-";
            return;
        }

        Vector2Int value = availableResolutions[selectedResolutionIndex];
        resolutionValueText.text = value.x + " x " + value.y;
    }

    private void UpdateDisplayModeLabel()
    {
        displayModeValueText.text = GetDisplayModeLabel(selectedDisplayMode);
    }

    private void UpdateMusicValueLabel(float value)
    {
        int percent = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f);
        musicValueText.text = percent + "%";
    }

    private FullScreenMode GetRelativeDisplayMode(int delta)
    {
        FullScreenMode[] modes = { FullScreenMode.ExclusiveFullScreen, FullScreenMode.Windowed, FullScreenMode.FullScreenWindow };
        int index = 0;
        for (int i = 0; i < modes.Length; i++)
        {
            if (modes[i] == selectedDisplayMode)
            {
                index = i;
                break;
            }
        }

        index = (index + delta + modes.Length) % modes.Length;
        return modes[index];
    }

    private static string GetDisplayModeLabel(FullScreenMode mode)
    {
        switch (mode)
        {
            case FullScreenMode.Windowed:
                return "Windowed";
            case FullScreenMode.FullScreenWindow:
                return "Borderless";
            default:
                return "Fullscreen";
        }
    }

    private AudioRuntimeService ResolveAudioService()
    {
        RuntimeShell shell = RuntimeShell.Instance;
        return shell != null ? shell.AudioRuntimeService : null;
    }

    private void ValidateBindings()
    {
        if (startButton == null ||
            settingsButton == null ||
            quitButton == null ||
            settingsPanel == null ||
            closeSettingsButton == null ||
            resolutionPrevButton == null ||
            resolutionNextButton == null ||
            resolutionValueText == null ||
            displayModePrevButton == null ||
            displayModeNextButton == null ||
            displayModeValueText == null ||
            musicVolumeSlider == null ||
            musicValueText == null)
        {
            throw new System.InvalidOperationException("MainMenuUI has unassigned UI references.");
        }
    }

    private void AutoWireReferences()
    {
        if (startButton == null) startButton = FindButton("StartButton");
        if (settingsButton == null) settingsButton = FindButton("SettingsButton");
        if (quitButton == null) quitButton = FindButton("QuitButton");
        if (settingsPanel == null)
        {
            Transform panel = transform.Find("SettingsPanel");
            settingsPanel = panel != null ? panel.gameObject : null;
        }

        if (closeSettingsButton == null) closeSettingsButton = FindButtonIn("SettingsPanel/CloseSettingsButton");
        if (resolutionPrevButton == null) resolutionPrevButton = FindButtonIn("SettingsPanel/ResolutionPrevButton");
        if (resolutionNextButton == null) resolutionNextButton = FindButtonIn("SettingsPanel/ResolutionNextButton");
        if (displayModePrevButton == null) displayModePrevButton = FindButtonIn("SettingsPanel/DisplayModePrevButton");
        if (displayModeNextButton == null) displayModeNextButton = FindButtonIn("SettingsPanel/DisplayModeNextButton");
        if (resolutionValueText == null) resolutionValueText = FindText("SettingsPanel/ResolutionValueText");
        if (displayModeValueText == null) displayModeValueText = FindText("SettingsPanel/DisplayModeValueText");
        if (musicValueText == null) musicValueText = FindText("SettingsPanel/MusicValueText");
        if (musicVolumeSlider == null)
        {
            Transform slider = transform.Find("SettingsPanel/MusicSlider");
            musicVolumeSlider = slider != null ? slider.GetComponent<Slider>() : null;
        }
    }

    private Button FindButton(string objectName)
    {
        Transform child = transform.Find(objectName);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private Button FindButtonIn(string relativePath)
    {
        Transform child = transform.Find(relativePath);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private TMP_Text FindText(string relativePath)
    {
        Transform child = transform.Find(relativePath);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }
}
