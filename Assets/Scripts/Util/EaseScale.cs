using System;
using UnityEngine;

public class EaseScale : MonoBehaviour
{
    public float durationSeconds = 1;
    public float _elapsedTime;
    
    private Vector3 _lastScale;
    private Vector3 _targetScale;
    private Action _onComplete;

    public Vector3 scale
    {
        set { SetScale(value); }
    }

    public static double EaseInOutCubic(double x)
    {
        return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
    }

    public void Start()
    {
        _lastScale = transform.localScale;
        _targetScale = transform.localScale;
    }

    public void SetScale(Vector3 scale, Action onComplete = null)
    {

        _lastScale = new Vector3(_targetScale.x, _targetScale.y, _targetScale.z);
        _targetScale = scale;
        _elapsedTime = 0f;

        _onComplete = onComplete;

        if (durationSeconds <= 0f)
        {
            transform.localScale = _targetScale;

            _onComplete?.Invoke();
            _onComplete = null;
        }
    }

    public void Update()
    {
        if (_elapsedTime < durationSeconds)
        {
            _elapsedTime += Time.deltaTime;
            float progress = durationSeconds > 0f
                ? Mathf.Clamp01(_elapsedTime / durationSeconds)
                : 1f;

            float eased = (float)EaseInOutCubic(progress);

            Vector3 scale = Vector3.Lerp(_lastScale, _targetScale, eased);

            transform.localScale = scale;

            if (progress >= 1f)
            {
                transform.localScale = _targetScale;

                _onComplete?.Invoke();
                _onComplete = null;
            }
        }
    }
}
