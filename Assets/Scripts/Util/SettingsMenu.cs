using System;
using UnityEngine;
using Util;

public class SettingsMenu : MonoBehaviour
{
    public GameObject Video;
    public GameObject Audio;
    public GameObject Gameplay;
    public GameObject Input;
    public GameObject Accessibility;

    public GameObject SettingsContent;
    public EaseColor screenColor;
    public bool settingsOpen = false;
    
    public void CloseSettings()
    {
        SettingsContent.SetActive(false);
        screenColor.targetColor = new Color(0f, 0f, 0f, 0.0f);
        settingsOpen = false;
    }

    public void OpenSettings()
    {
        SettingsContent.SetActive(true);
        screenColor.targetColor = new Color(0f, 0f, 0f, 0.8f);
        settingsOpen = true;
    }

    public void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsOpen)
            {
                CloseSettings();
            }
        }
    }


    public void NavTo(GameObject nav)
    {
        Video.SetActive(false);
        Audio.SetActive(false);
        Gameplay.SetActive(false);
        Input.SetActive(false);
        Accessibility.SetActive(false);
        
        nav.SetActive(true);
    }
}
