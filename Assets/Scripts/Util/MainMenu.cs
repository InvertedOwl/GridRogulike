using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public EaseScale fadeScale;
    
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
