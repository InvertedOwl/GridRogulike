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

        StaticResetManager.ResetRunStatics();
    }

    public void UpdateMainMenu()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "save1.json");
        bool saveExists = File.Exists(savePath);

        SetMenuGroupVisible(SaveExists, saveExists);
        SetMenuGroupVisible(SaveDoesntExist, !saveExists);
    }

    private void SetMenuGroupVisible(GameObject group, bool isVisible)
    {
        if (group == null)
            return;

        group.SetActive(true);

        CanvasGroup canvasGroup = group.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = group.AddComponent<CanvasGroup>();

        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }
    
    public void StartRun()
    {
        StaticResetManager.ResetRunStatics();
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
