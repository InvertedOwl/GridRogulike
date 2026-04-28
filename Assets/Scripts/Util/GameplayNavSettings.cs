using UnityEngine;
using UnityEngine.UI;

public class GameplayNavSettings : MonoBehaviour
{
    public Slider Speed;
    public Toggle autoendturn;
    public static float speed;
    public static bool endturn;
    
    
    public void Start()
    {
        float animSpeed = PlayerPrefs.GetFloat("speed", 1.0f);
        bool automaticallyendturn = PlayerPrefs.GetInt("endturn", 0) == 1 ? true : false;

        Speed.value = animSpeed;
        autoendturn.isOn = automaticallyendturn;


        UpdateSettings();
    }


    public void UpdateSettings()
    {
        float animSpeed = PlayerPrefs.GetFloat("speed", 1.0f);
        speed = animSpeed;
        
        endturn = PlayerPrefs.GetInt("endturn", 0) == 1 ? true : false;
    }

    public void ChangeSliderSpeed()
    {
        PlayerPrefs.SetFloat("speed", Speed.value);
        UpdateSettings();
    }
    
    public void ChangeAutoEndTurn()
    {
        PlayerPrefs.SetInt("endturn", autoendturn.isOn ? 1 : 0);
        UpdateSettings();
    }
}
