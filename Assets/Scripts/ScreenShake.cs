using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance;

    private float currentShakeMagnitude;
    private float targetShakeMagnitude;
    private float dampingSpeed;
    private Vector3 currentOffset;
    private bool hasOffset;

    private void Awake ()
    {
        Instance = this;
    }

    public void Shake(float magnitude, float damping = 1f)
    {
        RemoveCurrentOffset();

        if (magnitude >= currentShakeMagnitude)
            dampingSpeed = damping;

        currentShakeMagnitude = Mathf.Max(currentShakeMagnitude, magnitude);
        targetShakeMagnitude = 0;
        enabled = true;
    }
    
    // Another instance of allowed UnityEngine.Random because screen shake should absolutely not be seeded
    private void LateUpdate()
    {
        RemoveCurrentOffset();

        currentShakeMagnitude = Mathf.Lerp(currentShakeMagnitude, targetShakeMagnitude, Time.deltaTime * dampingSpeed);

        if (currentShakeMagnitude <= 0.001f)
        {
            currentShakeMagnitude = 0f;
            enabled = false;
            return;
        }

        currentOffset = Random.insideUnitSphere * currentShakeMagnitude;
        transform.localPosition += currentOffset;
        hasOffset = true;
    }

    private void OnDisable()
    {
        RemoveCurrentOffset();
    }

    private void RemoveCurrentOffset()
    {
        if (!hasOffset)
            return;

        transform.localPosition -= currentOffset;
        currentOffset = Vector3.zero;
        hasOffset = false;
    }
}
