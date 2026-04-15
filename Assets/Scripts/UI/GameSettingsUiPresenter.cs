using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class GameSettingsUiPresenter : IDisposable
{
    private const string DisplayWidthPrefKey = "Settings.Display.Width";
    private const string DisplayHeightPrefKey = "Settings.Display.Height";
    private const string DisplayModePrefKey = "Settings.Display.Mode";
    private const int DefaultWidth = 1920;
    private const int DefaultHeight = 1080;
    private const FullScreenMode DefaultDisplayMode = FullScreenMode.Windowed;

    private readonly VisualElement settingsRoot;
    private readonly VisualElement settingsCard;
    private readonly VisualElement settingsDragHandle;
    private readonly Button closeButton;
    private readonly Button resolutionPrevButton;
    private readonly Button resolutionNextButton;
    private readonly Label resolutionValueLabel;
    private readonly Button displayModePrevButton;
    private readonly Button displayModeNextButton;
    private readonly Label displayModeValueLabel;
    private readonly Slider masterVolumeSlider;
    private readonly Label masterVolumeValueLabel;
    private readonly List<Vector2Int> availableResolutions = new List<Vector2Int>();

    private int selectedResolutionIndex;
    private FullScreenMode selectedDisplayMode;
    private bool disposed;

    public bool IsVisible => settingsRoot.style.display != DisplayStyle.None;

    public GameSettingsUiPresenter(VisualElement root)
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root));
        }

        settingsRoot = root.Q<VisualElement>("settings-root");
        settingsCard = root.Q<VisualElement>("settings-card");
        settingsDragHandle = root.Q<VisualElement>("settings-drag-handle");
        closeButton = root.Q<Button>("settings-close-btn");
        resolutionPrevButton = root.Q<Button>("settings-resolution-prev-btn");
        resolutionNextButton = root.Q<Button>("settings-resolution-next-btn");
        resolutionValueLabel = root.Q<Label>("settings-resolution-value");
        displayModePrevButton = root.Q<Button>("settings-displaymode-prev-btn");
        displayModeNextButton = root.Q<Button>("settings-displaymode-next-btn");
        displayModeValueLabel = root.Q<Label>("settings-displaymode-value");
        masterVolumeSlider = root.Q<Slider>("settings-master-volume-slider");
        masterVolumeValueLabel = root.Q<Label>("settings-master-volume-value");

        if (settingsRoot == null ||
            settingsCard == null ||
            settingsDragHandle == null ||
            closeButton == null ||
            resolutionPrevButton == null ||
            resolutionNextButton == null ||
            resolutionValueLabel == null ||
            displayModePrevButton == null ||
            displayModeNextButton == null ||
            displayModeValueLabel == null ||
            masterVolumeSlider == null ||
            masterVolumeValueLabel == null)
        {
            throw new InvalidOperationException("Settings panel UI binding failed.");
        }

        closeButton.clicked += HandleCloseClicked;
        resolutionPrevButton.clicked += HandleResolutionPrevClicked;
        resolutionNextButton.clicked += HandleResolutionNextClicked;
        displayModePrevButton.clicked += HandleDisplayModePrevClicked;
        displayModeNextButton.clicked += HandleDisplayModeNextClicked;
        masterVolumeSlider.RegisterValueChangedCallback(HandleMasterVolumeChanged);

        LoadResolutionOptions();
        LoadOrApplyDisplaySettings();

        AudioRuntimeService audioService = ResolveAudioService();
        float currentMasterVolume = audioService != null ? audioService.GetMasterVolume() : 1f;
        masterVolumeSlider.SetValueWithoutNotify(currentMasterVolume);
        UpdateMasterVolumeLabel(currentMasterVolume);

        UpdateResolutionLabel();
        UpdateDisplayModeLabel();
        Hide(saveAudioSettings: false);
    }

    public void Open()
    {
        settingsCard.style.position = Position.Relative;
        settingsCard.style.left = StyleKeyword.Auto;
        settingsCard.style.top = StyleKeyword.Auto;
        settingsCard.style.right = StyleKeyword.Auto;
        settingsCard.style.bottom = StyleKeyword.Auto;
        settingsRoot.style.display = DisplayStyle.Flex;
        settingsRoot.pickingMode = PickingMode.Position;
    }

    public void Hide(bool saveAudioSettings)
    {
        settingsRoot.style.display = DisplayStyle.None;
        settingsRoot.pickingMode = PickingMode.Ignore;
        if (!saveAudioSettings)
        {
            return;
        }

        AudioRuntimeService audioService = ResolveAudioService();
        if (audioService != null)
        {
            audioService.SaveVolumeSettings();
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        closeButton.clicked -= HandleCloseClicked;
        resolutionPrevButton.clicked -= HandleResolutionPrevClicked;
        resolutionNextButton.clicked -= HandleResolutionNextClicked;
        displayModePrevButton.clicked -= HandleDisplayModePrevClicked;
        displayModeNextButton.clicked -= HandleDisplayModeNextClicked;
        masterVolumeSlider.UnregisterValueChangedCallback(HandleMasterVolumeChanged);
    }

    private void HandleCloseClicked()
    {
        Hide(saveAudioSettings: true);
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

    private void HandleMasterVolumeChanged(ChangeEvent<float> evt)
    {
        float value = Mathf.Clamp01(evt.newValue);
        UpdateMasterVolumeLabel(value);

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

        Vector2Int selected = availableResolutions[selectedResolutionIndex];
        Screen.SetResolution(selected.x, selected.y, selectedDisplayMode);
        SaveDisplaySettings();
        UpdateResolutionLabel();
        UpdateDisplayModeLabel();
    }

    private void LoadResolutionOptions()
    {
        Resolution[] resolutions = Screen.resolutions;
        availableResolutions.Clear();

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution resolution = resolutions[i];
            bool exists = false;
            for (int j = 0; j < availableResolutions.Count; j++)
            {
                Vector2Int current = availableResolutions[j];
                if (current.x == resolution.width && current.y == resolution.height)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                availableResolutions.Add(new Vector2Int(resolution.width, resolution.height));
            }
        }

        selectedResolutionIndex = 0;
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            if (availableResolutions[i].x == currentWidth && availableResolutions[i].y == currentHeight)
            {
                selectedResolutionIndex = i;
                break;
            }
        }
    }

    private void UpdateResolutionLabel()
    {
        if (availableResolutions.Count == 0)
        {
            resolutionValueLabel.text = "-";
            return;
        }

        Vector2Int resolution = availableResolutions[selectedResolutionIndex];
        resolutionValueLabel.text = resolution.x + " x " + resolution.y;
    }

    private void UpdateDisplayModeLabel()
    {
        displayModeValueLabel.text = GetDisplayModeLabel(selectedDisplayMode);
    }

    private void UpdateMasterVolumeLabel(float value)
    {
        int percentage = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f);
        masterVolumeValueLabel.text = percentage + "%";
    }

    private FullScreenMode GetRelativeDisplayMode(int delta)
    {
        FullScreenMode[] modes =
        {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.Windowed,
            FullScreenMode.FullScreenWindow
        };

        int currentIndex = 0;
        for (int i = 0; i < modes.Length; i++)
        {
            if (modes[i] == selectedDisplayMode)
            {
                currentIndex = i;
                break;
            }
        }

        currentIndex = (currentIndex + delta + modes.Length) % modes.Length;
        return modes[currentIndex];
    }

    private void LoadOrApplyDisplaySettings()
    {
        if (PlayerPrefs.HasKey(DisplayWidthPrefKey) &&
            PlayerPrefs.HasKey(DisplayHeightPrefKey) &&
            PlayerPrefs.HasKey(DisplayModePrefKey))
        {
            int width = PlayerPrefs.GetInt(DisplayWidthPrefKey);
            int height = PlayerPrefs.GetInt(DisplayHeightPrefKey);
            selectedDisplayMode = (FullScreenMode)PlayerPrefs.GetInt(DisplayModePrefKey);
            selectedResolutionIndex = FindClosestResolutionIndex(width, height);
            ApplyDisplaySettings();
            return;
        }

        selectedDisplayMode = DefaultDisplayMode;
        selectedResolutionIndex = FindClosestResolutionIndex(DefaultWidth, DefaultHeight);
        ApplyDisplaySettings();
    }

    private int FindClosestResolutionIndex(int width, int height)
    {
        int bestIndex = 0;
        int bestDistance = int.MaxValue;

        for (int i = 0; i < availableResolutions.Count; i++)
        {
            int dx = availableResolutions[i].x - width;
            int dy = availableResolutions[i].y - height;
            int distance = dx * dx + dy * dy;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void SaveDisplaySettings()
    {
        Vector2Int selected = availableResolutions[selectedResolutionIndex];
        PlayerPrefs.SetInt(DisplayWidthPrefKey, selected.x);
        PlayerPrefs.SetInt(DisplayHeightPrefKey, selected.y);
        PlayerPrefs.SetInt(DisplayModePrefKey, (int)selectedDisplayMode);
        PlayerPrefs.Save();
    }

    private static string GetDisplayModeLabel(FullScreenMode mode)
    {
        switch (mode)
        {
            case FullScreenMode.Windowed:
                return "窗口";
            case FullScreenMode.FullScreenWindow:
                return "无边框";
            default:
                return "全屏";
        }
    }

    private static AudioRuntimeService ResolveAudioService()
    {
        RuntimeShell shell = RuntimeShell.Instance;
        return shell != null ? shell.AudioRuntimeService : null;
    }

}
