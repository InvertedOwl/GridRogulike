using System;
using UnityEngine;
using Util;

public class PauseScreen : MonoBehaviour
{
    public bool isPaused = false;

    public EaseColor screenColor;
    public GameObject pauseContent;
    
    public SettingsMenu settingsMenu;
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !settingsMenu.settingsOpen)
        {
            if (isPaused)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
            
        }
    }

    public void Pause()
    {
        screenColor.targetColor = new Color(0f, 0f, 0f, 0.8f);
        pauseContent.SetActive(true);
        isPaused = true;
    }

    public void Unpause()
    {
        screenColor.targetColor = new Color(0f, 0f, 0f, 0.0f);
        pauseContent.SetActive(false);
        isPaused = false;
    }
}
