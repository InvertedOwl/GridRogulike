using System;
using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    public class PulseTransparency : MonoBehaviour
    {
        [Header("Target Type")]
        public bool useMaterialColor = true;

        [Header("Alpha Range")]
        [Range(0f, 1f)] public float minAlpha = 0.25f;
        [Range(0f, 1f)] public float maxAlpha = 1f;

        [Header("Pulse Settings")]
        public float pulseSpeed = 1f;
        public bool playOnStart = true;

        private Renderer _renderer;
        private Graphic _graphic;

        private bool _isPlaying;
        private float _startTimeOffset;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _graphic = GetComponent<Graphic>();

            minAlpha = Mathf.Clamp01(minAlpha);
            maxAlpha = Mathf.Clamp01(maxAlpha);

            _isPlaying = playOnStart;
            _startTimeOffset = 0f;

            ApplyAlphaImmediate(_isPlaying ? EvaluatePulseAlpha() : minAlpha);
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            ApplyAlphaImmediate(EvaluatePulseAlpha());
        }

        public void Play()
        {
            _isPlaying = true;
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        public void ResetPulse()
        {
            _startTimeOffset = Time.time;
            ApplyAlphaImmediate(_isPlaying ? EvaluatePulseAlpha() : minAlpha);
        }

        public void SetPulseSpeed(float speed)
        {
            pulseSpeed = speed;
        }

        public void SetAlphaRange(float newMinAlpha, float newMaxAlpha)
        {
            minAlpha = Mathf.Clamp01(newMinAlpha);
            maxAlpha = Mathf.Clamp01(newMaxAlpha);
        }

        public static float EaseInOutCubic(float x)
        {
            return x < 0.5f
                ? 4f * x * x * x
                : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
        }

        private float EvaluatePulseAlpha()
        {
            if (pulseSpeed <= 0f)
                return minAlpha;

            float elapsed = (Time.time - _startTimeOffset) * pulseSpeed;

            // 0 -> 1 -> 0 repeating
            float rawPhase = Mathf.PingPong(elapsed, 1f);

            // Apply easing to the phase
            float easedPhase = EaseInOutCubic(rawPhase);

            // Map eased phase into alpha range
            return minAlpha + (maxAlpha - minAlpha) * easedPhase;
        }

        private void ApplyAlphaImmediate(float alpha)
        {
            if (useMaterialColor && _renderer != null)
            {
                Color c = _renderer.material.color;
                c.a = alpha;
                _renderer.material.color = c;
            }
            else if (_graphic != null)
            {
                Color c = _graphic.color;
                c.a = alpha;
                _graphic.color = c;
            }
        }
    }
}