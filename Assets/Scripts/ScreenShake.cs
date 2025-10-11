using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance;

    private Vector3 initialPosition;
    private float currentShakeMagnitude;
    private float targetShakeMagnitude;
    private float dampingSpeed;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        initialPosition = transform.localPosition;
    }

    public void Shake(float magnitude, float damping = 1f)
    {
        currentShakeMagnitude = magnitude;
        targetShakeMagnitude = 0;
        initialPosition = transform.localPosition;
        dampingSpeed = damping;
    }

    private void Update()
    {
        currentShakeMagnitude = Mathf.Lerp(currentShakeMagnitude, targetShakeMagnitude, Time.deltaTime * dampingSpeed);
        Vector3 randomPoint = initialPosition + Random.insideUnitSphere * currentShakeMagnitude;
        transform.localPosition = randomPoint;
    }
}