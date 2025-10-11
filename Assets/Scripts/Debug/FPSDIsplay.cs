using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    private TextMeshProUGUI fpsText;

    private int frameCount = 0;
    private float elapsedTime = 0f;
    private float updateInterval = 0.1f;

    void Start()
    {
        fpsText = GetComponent<TextMeshProUGUI>();
    }
    
    void Update()
    {
        frameCount++;
        elapsedTime += Time.unscaledDeltaTime;

        if (elapsedTime >= updateInterval)
        {
            float fps = frameCount / elapsedTime;
            fpsText.text = $"FPS: {fps:F1}";

            frameCount = 0;
            elapsedTime = 0f;
        }
    }
}