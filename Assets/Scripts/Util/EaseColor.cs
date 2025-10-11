using System;
using UnityEngine;
using UnityEngine.UI; // Only needed if using UI Graphic components

namespace Util
{
    public class EaseColor : MonoBehaviour
    {
        public float durationSeconds = 1f;
        public bool useMaterialColor = true; // true = Renderer.material.color, false = UI Graphic.color
        public float _elapsedTime;

        private Color _lastColor;
        private Color _targetColor;
        private Action _onComplete;

        private Renderer _renderer;
        private Graphic _graphic;

        public Color targetColor
        {
            set { SendToColor(value); }
            get { return _targetColor; }
        }

        public static double EaseInOutCubic(double x)
        {
            return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
        }

        public void Start()
        {
            _renderer = GetComponent<Renderer>();
            _graphic = GetComponent<Graphic>();

            if (useMaterialColor && _renderer != null)
            {
                _lastColor = _renderer.material.color;
                _targetColor = _renderer.material.color;
            }
            else if (_graphic != null)
            {
                _lastColor = _graphic.color;
                _targetColor = _graphic.color;
            }
        }

        public void InstantSend(Color color)
        {
            _lastColor = color;
            _targetColor = color;
            _elapsedTime = 0;
            _onComplete = null;

            ApplyColor(color);
        }

        public void SendToColor(Color color, Action onComplete = null)
        {
            _lastColor = _targetColor;
            _targetColor = color;
            _elapsedTime = 0f;

            _onComplete = onComplete;

            if (durationSeconds <= 0f)
            {
                ApplyColor(_targetColor);
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

                Color c = Color.Lerp(_lastColor, _targetColor, eased);
                ApplyColor(c);

                if (progress >= 1f)
                {
                    ApplyColor(_targetColor);
                    _onComplete?.Invoke();
                    _onComplete = null;
                }
            }
        }

        private void ApplyColor(Color c)
        {
            if (useMaterialColor && _renderer != null)
                _renderer.material.color = c;
            else if (_graphic != null)
                _graphic.color = c;
        }
    }
}
