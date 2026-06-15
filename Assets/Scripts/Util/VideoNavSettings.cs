using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VideoNavSettings : MonoBehaviour
{
    public TMP_Dropdown Screen;
    public TMP_Dropdown Resolution;
    public TMP_Dropdown Display;
    public TMP_Dropdown FPSLimit;
    public TMP_Dropdown Antialiasing;
    public Toggle VSync;

    private readonly List<DisplayInfo> displays = new List<DisplayInfo>();
    private int _lastAppliedDisplay = -1;

    public void Start()
    {
        PopulateDisplayDropdown();

        string screen = PlayerPrefs.GetString("screen", "Fullscreen");
        string resolution = PlayerPrefs.GetString("resolution", "1920x1080");
        int display = PlayerPrefs.GetInt("display", 0);
        string fps = PlayerPrefs.GetString("fps", "60");
        string antialiasing = PlayerPrefs.GetString("antialiasing", "None");
        bool vSync = PlayerPrefs.GetInt("vsync", 1) == 1;

        SetDropdownString(Screen, screen);
        SetDropdownString(Resolution, resolution);
        SetDropdownInt(Display, display);
        SetDropdownString(FPSLimit, fps);
        SetDropdownString(Antialiasing, antialiasing);
        VSync.SetIsOnWithoutNotify(vSync);

        UpdateSettings();
    }

    private void PopulateDisplayDropdown()
    {
        displays.Clear();
        UnityEngine.Device.Screen.GetDisplayLayout(displays);

        Display.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < displays.Count; i++)
        {
            DisplayInfo display = displays[i];

            string name = string.IsNullOrEmpty(display.name)
                ? $"Monitor {i + 1}"
                : display.name;

            options.Add($"{name} - {display.width}x{display.height}");
        }

        Display.AddOptions(options);
        Display.RefreshShownValue();
    }

    public void UpdateSettings()
    {
        string screen = PlayerPrefs.GetString("screen", "Fullscreen");
        string resolution = PlayerPrefs.GetString("resolution", "1920x1080");
        int display = PlayerPrefs.GetInt("display", 0);
        string fps = PlayerPrefs.GetString("fps", "60");
        string antialiasing = PlayerPrefs.GetString("antialiasing", "None");
        bool vSync = PlayerPrefs.GetInt("vsync", 1) == 1;

        FullScreenMode fullScreenMode = FullScreenMode.Windowed;

        if (screen == "Fullscreen")
        {
            fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else if (screen == "Borderless Fullscreen")
        {
            fullScreenMode = FullScreenMode.FullScreenWindow;
        }

        bool isBorderlessFullscreen = fullScreenMode == FullScreenMode.FullScreenWindow;
        if (Resolution != null)
            Resolution.interactable = !isBorderlessFullscreen;
        if (FPSLimit != null)
            FPSLimit.interactable = !vSync;

        display = Mathf.Clamp(display, 0, Mathf.Max(0, displays.Count - 1));

        if (display >= 0 && display < displays.Count && display != _lastAppliedDisplay)
        {
            UnityEngine.Device.Screen.MoveMainWindowTo(
                displays[display],
                Vector2Int.zero
            );
            _lastAppliedDisplay = display;
        }

        if (TryGetTargetResolution(resolution, display, isBorderlessFullscreen, out int width, out int height))
        {
            UnityEngine.Device.Screen.SetResolution(width, height, fullScreenMode);
        }

        if (!vSync && int.TryParse(fps, out int fpsLimit))
        {
            Application.targetFrameRate = fpsLimit;
        }
        else if (vSync)
        {
            Application.targetFrameRate = -1;
        }

        switch (antialiasing)
        {
            case "None":
                QualitySettings.antiAliasing = 0;
                break;

            case "2x":
                QualitySettings.antiAliasing = 2;
                break;

            case "4x":
                QualitySettings.antiAliasing = 4;
                break;

            case "8x":
                QualitySettings.antiAliasing = 8;
                break;
        }

        QualitySettings.vSyncCount = vSync ? 1 : 0;
    }

    private bool TryGetTargetResolution(
        string resolution,
        int display,
        bool isBorderlessFullscreen,
        out int width,
        out int height)
    {
        width = 0;
        height = 0;

        if (isBorderlessFullscreen && display >= 0 && display < displays.Count)
        {
            width = displays[display].width;
            height = displays[display].height;
            return true;
        }

        string[] resolutionSplit = resolution.Split('x');
        if (resolutionSplit.Length != 2 ||
            !int.TryParse(resolutionSplit[0], out width) ||
            !int.TryParse(resolutionSplit[1], out height))
        {
            return false;
        }

        return width > 0 && height > 0;
    }

    public void SetDropdownString(TMP_Dropdown dropdown, string str)
    {
        List<string> optionTexts = new List<string>();

        foreach (var option in dropdown.options)
        {
            optionTexts.Add(option.text);
        }

        int index = optionTexts.IndexOf(str);

        if (index >= 0)
        {
            dropdown.SetValueWithoutNotify(index);
        }
        else
        {
            dropdown.SetValueWithoutNotify(0);
        }

        dropdown.RefreshShownValue();
    }

    public void SetDropdownInt(TMP_Dropdown dropdown, int index)
    {
        if (index >= 0 && index < dropdown.options.Count)
        {
            dropdown.SetValueWithoutNotify(index);
        }
        else
        {
            dropdown.SetValueWithoutNotify(0);
        }

        dropdown.RefreshShownValue();
    }

    public void SetScreen()
    {
        PlayerPrefs.SetString("screen", Screen.options[Screen.value].text);
        PlayerPrefs.Save();

        UpdateSettings();
    }

    public void SetResolution()
    {
        PlayerPrefs.SetString("resolution", Resolution.options[Resolution.value].text);
        PlayerPrefs.Save();

        UpdateSettings();
    }

    public void SetDisplay()
    {
        PlayerPrefs.SetInt("display", Display.value);
        PlayerPrefs.Save();

        UpdateSettings();
    }

    public void SetFPSLimit()
    {
        PlayerPrefs.SetString("fps", FPSLimit.options[FPSLimit.value].text);
        PlayerPrefs.Save();

        UpdateSettings();
    }

    public void SetAntialiasing()
    {
        PlayerPrefs.SetString("antialiasing", Antialiasing.options[Antialiasing.value].text);
        PlayerPrefs.Save();

        UpdateSettings();
    }

    public void SetVSync()
    {
        PlayerPrefs.SetInt("vsync", VSync.isOn ? 1 : 0);
        PlayerPrefs.Save();

        UpdateSettings();
    }
}
