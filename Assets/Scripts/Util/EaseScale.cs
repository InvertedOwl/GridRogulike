using System;
using UnityEngine;

public class EaseScale : MonoBehaviour
{
    public float durationSeconds = 1;
    public float _elapsedTime;
    
    private Vector3 _lastScale;
    private Vector3 _targetScale;
    private Action _onComplete;
    private bool _hasExplicitTarget;
    private bool _initialized;

    public Vector3 scale
    {
        set { SetScale(value); }
    }

    public static double EaseInOutCubic(double x)
    {
        return x < 0.5 ? 4 * x * x * x : 1 - ((-2 * x + 2) * (-2 * x + 2) * (-2 * x + 2)) / 2;
    }

    public void Awake()
    {
        Initialize();
    }

    public void Start()
    {
        Initialize();

        if (_elapsedTime >= durationSeconds)
            enabled = false;
    }

    private void Initialize()
    {
        if (_initialized || _hasExplicitTarget)
            return;

        _lastScale = transform.localScale;
        _targetScale = transform.localScale;
        _elapsedTime = durationSeconds;
        _initialized = true;
    }

    public void SetScale(Vector3 scale, Action onComplete = null)
    {
        Initialize();

        // CHANGED: Set _lastScale to the current actual scale instead of the target
        // This prevents jumping when the animation is interrupted
        _lastScale = transform.localScale;
        _targetScale = scale;
        _elapsedTime = 0f;
        _hasExplicitTarget = true;
        _initialized = true;

        _onComplete = onComplete;

        if (durationSeconds <= 0f)
        {
            transform.localScale = _targetScale;

            _onComplete?.Invoke();
            _onComplete = null;
            _elapsedTime = durationSeconds;
            enabled = false;
            return;
        }

        enabled = true;
    }

    public void Update()
    {
        if (_elapsedTime >= durationSeconds)
            return;

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
            enabled = false;
        }
    }
}
