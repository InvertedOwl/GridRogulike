using System;
using StateManager;
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
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (AreYouSure.Instance != null && AreYouSure.Instance.IsOpen)
        {
            AreYouSure.Instance.Cancel();
            return;
        }

        if (settingsMenu != null && settingsMenu.settingsOpen)
            return;

        if (isPaused)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Scenes/MainMenu");
    }

    public void Pause()
    {
        GameStateManager.Instance?.PlayWindowInSound();
        screenColor.targetColor = new Color(0f, 0f, 0f, 0.8f);
        pauseContent.SetActive(true);
        isPaused = true;
    }

    public void Unpause()
    {
        GameStateManager.Instance?.PlayWindowOutSound();
        screenColor.targetColor = new Color(0f, 0f, 0f, 0.0f);
        pauseContent.SetActive(false);
        isPaused = false;
    }
}
