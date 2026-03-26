using UnityEngine;
using UnityEngine.UI;

public class GameplayNavSettings : MonoBehaviour
{
    public Slider Speed;
    public static float speed;
    
    
    public void Start()
    {
        float animSpeed = PlayerPrefs.GetFloat("speed", 1.0f);

        Speed.value = animSpeed;


        UpdateSettings();
    }


    public void UpdateSettings()
    {
        float animSpeed = PlayerPrefs.GetFloat("speed", 1.0f);
        speed = animSpeed;
    }

    public void ChangeSliderSpeed()
    {
        PlayerPrefs.SetFloat("speed", Speed.value);
        UpdateSettings();
    }
}
