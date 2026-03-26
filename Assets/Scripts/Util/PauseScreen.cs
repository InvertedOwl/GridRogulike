using System;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public void GoToMainMenu()
    {
        RunInfo.ResetRandoms();
        SceneManager.LoadScene("Scenes/MainMenu");
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
