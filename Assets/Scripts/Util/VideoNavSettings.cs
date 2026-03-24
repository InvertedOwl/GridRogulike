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
    
    
    public void Start()
    {
        string screen = PlayerPrefs.GetString("screen", "Fullscreen");
        string resolution = PlayerPrefs.GetString("resolution", "1920x1080");
        string display = PlayerPrefs.GetString("display", "Monitor 1");
        string fps = PlayerPrefs.GetString("fps", "60");
        string antialiasing = PlayerPrefs.GetString("antialiasing", "None");
        bool vSync = PlayerPrefs.GetInt("vsync", 1) == 1;


        SetDropdownString(Screen, screen);
        SetDropdownString(Resolution, resolution);
        SetDropdownString(Display, display);
        SetDropdownString(FPSLimit, fps);
        SetDropdownString(Antialiasing, antialiasing);
        VSync.SetIsOnWithoutNotify(vSync);

        UpdateSettings();
    }


    public void UpdateSettings()
    {
        string screen = PlayerPrefs.GetString("screen", "Fullscreen");
        string resolution = PlayerPrefs.GetString("resolution", "1920x1080");
        string display = PlayerPrefs.GetString("display", "Monitor 1");
        string fps = PlayerPrefs.GetString("fps", "60");
        string antialiasing = PlayerPrefs.GetString("antialiasing", "None");
        bool vSync = PlayerPrefs.GetInt("vsync", 1) == 1;

        FullScreenMode fullScreenMode = FullScreenMode.Windowed;

        if (screen == "Fullscreen")
        {
            fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        } else if (screen == "Borderless Fullscreen")
        {
            fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        
        string[] resolutionSplit = resolution.Split('x');
        int width = int.Parse(resolutionSplit[0]);
        int height = int.Parse(resolutionSplit[1]);

        UnityEngine.Device.Screen.SetResolution(width, height, fullScreenMode);
        Application.targetFrameRate = int.Parse(fps);
        switch (antialiasing)
        {
            case "None": QualitySettings.antiAliasing = 0; break;
            case "2x": QualitySettings.antiAliasing = 2; break;
            case "4x": QualitySettings.antiAliasing = 4; break;
            case "8x": QualitySettings.antiAliasing = 8; break;
        }
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }

    public void SetDropdownString(TMP_Dropdown dropdown, string str)
    {
        List<string> optionTexts = new List<string>();
        foreach (var option in dropdown.options)
        {
            optionTexts.Add(option.text);
        }

        dropdown.value = optionTexts.IndexOf(str);
    }

    public void SetScreen()
    {
        PlayerPrefs.SetString("screen", Screen.options[Screen.value].text);
        UpdateSettings();
    }
    
    public void SetResolution()
    {
        PlayerPrefs.SetString("resolution", Resolution.options[Resolution.value].text);
        UpdateSettings();
    }
    
    public void SetDisplay()
    {
        PlayerPrefs.SetString("display", Display.options[Display.value].text);
        UpdateSettings();
    }
    
    public void SetFPSLimit()
    {
        PlayerPrefs.SetString("fps", FPSLimit.options[FPSLimit.value].text);
        UpdateSettings();
    }
    
    public void SetAntialiasing()
    {
        PlayerPrefs.SetString("antialiasing", Antialiasing.options[Antialiasing.value].text);
        UpdateSettings();
    }
    
    
    public void SetVSync()
    {
        PlayerPrefs.SetInt("vsync", VSync.isOn?1:0);
        UpdateSettings();
    }

}
