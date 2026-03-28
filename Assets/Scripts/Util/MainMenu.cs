using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public EaseScale fadeScale;

    public GameObject SaveExists;
    public GameObject SaveDoesntExist;
    
    public void Start()
    {
        UpdateMainMenu();
    }

    public void AbandonRun()
    {
        DeleteSave();
        UpdateMainMenu();
    }
    
    public void DeleteSave()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "save1.json");

        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save file deleted: " + savePath);
        }
        else
        {
            Debug.Log("No save file to delete.");
        }
    }

    public void UpdateMainMenu()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "save1.json");
        if (File.Exists(savePath))
        {
            SaveExists.SetActive(true);
            SaveDoesntExist.SetActive(false);
        }
        else
        {
            SaveExists.SetActive(false);
            SaveDoesntExist.SetActive(true);
        }
    }
    
    public void StartRun()
    {
        // TEMP just send to run
        fadeScale.SetScale(new Vector3(1, 1, 1), () =>
        {
            SceneManager.LoadScene("Scenes/Run");
        });
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
