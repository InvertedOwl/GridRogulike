using System;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class SettingsMenu : MonoBehaviour
{
    public GameObject Video;
    public GameObject Audio;
    public GameObject Gameplay;
    public GameObject Input;
    public GameObject Accessibility;

    public Button VideoButton;
    public Button AudioButton;
    public Button GameplayButton;
    public Button InputButton;
    public Button AccessiblityButton;
    
    public GameObject SettingsContent;
    public EaseColor screenColor;
    public bool settingsOpen = false;
    
    public void CloseSettings()
    {
        SettingsContent.GetComponent<EasePosition>().targetLocation = new Vector3(0, 750, 0);
        screenColor.targetColor = new Color(0f, 0f, 0f, 0.0f);
        settingsOpen = false;
    }

    public void OpenSettings()
    {
        SettingsContent.GetComponent<EasePosition>().targetLocation = new Vector3(0, 75, 0);
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
        Video.transform.localPosition = new Vector3(0, -20000, 0);
        Audio.transform.localPosition = new Vector3(0, -20000, 0);
        Gameplay.transform.localPosition = new Vector3(0, -20000, 0);
        Input.transform.localPosition = new Vector3(0, -20000, 0);
        Accessibility.transform.localPosition = new Vector3(0, -20000, 0);
        
        nav.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void NavColors(Button self)
    {
        VideoButton.GetComponent<EaseColor>().targetColor = new Color(55f/255f, 70f/255f, 94f/255f);
        AudioButton.GetComponent<EaseColor>().targetColor = new Color(55f/255f, 70f/255f, 94f/255f);
        GameplayButton.GetComponent<EaseColor>().targetColor = new Color(55f/255f, 70f/255f, 94f/255f);
        InputButton.GetComponent<EaseColor>().targetColor = new Color(55f/255f, 70f/255f, 94f/255f);
        AccessiblityButton.GetComponent<EaseColor>().targetColor = new Color(55f/255f, 70f/255f, 94f/255f);
        
        self.GetComponent<EaseColor>().targetColor = new Color(18f/255f, 29f/255f, 46f/255f);
    }
}
