using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    public class PulseLightness : MonoBehaviour
    {
        [Header("Target Type")]
        public bool useMaterialColor = true;

        [Header("Lightness Range")]
        [Range(0f, 1f)] public float minLightness = 0.5f;
        [Range(0f, 1f)] public float maxLightness = 1f;

        [Header("Pulse Settings")]
        public float pulseSpeed = 1f;
        public bool playOnStart = true;

        private Renderer _renderer;
        private Graphic _graphic;
        private Color _baseColor;
        private float _baseHue;
        private float _baseSaturation;

        private bool _isPlaying;
        private float _startTimeOffset;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _graphic = GetComponent<Graphic>();

            _baseColor = GetCurrentColor();
            Color.RGBToHSV(_baseColor, out _baseHue, out _baseSaturation, out _);

            minLightness = Mathf.Clamp01(minLightness);
            maxLightness = Mathf.Clamp01(maxLightness);

            _isPlaying = playOnStart;
            _startTimeOffset = 0f;

            ApplyLightnessImmediate(_isPlaying ? EvaluatePulseLightness() : minLightness);
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            ApplyLightnessImmediate(EvaluatePulseLightness());
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
            ApplyLightnessImmediate(_isPlaying ? EvaluatePulseLightness() : minLightness);
        }

        public void SetPulseSpeed(float speed)
        {
            pulseSpeed = speed;
        }

        public void SetLightnessRange(float newMinLightness, float newMaxLightness)
        {
            minLightness = Mathf.Clamp01(newMinLightness);
            maxLightness = Mathf.Clamp01(newMaxLightness);
        }

        public static float EaseInOutCubic(float x)
        {
            return x < 0.5f
                ? 4f * x * x * x
                : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
        }

        private float EvaluatePulseLightness()
        {
            if (pulseSpeed <= 0f)
                return minLightness;

            float elapsed = (Time.time - _startTimeOffset) * pulseSpeed;
            float rawPhase = Mathf.PingPong(elapsed, 1f);
            float easedPhase = EaseInOutCubic(rawPhase);

            return minLightness + (maxLightness - minLightness) * easedPhase;
        }

        private Color GetCurrentColor()
        {
            if (useMaterialColor && _renderer != null)
                return _renderer.material.color;

            if (_graphic != null)
                return _graphic.color;

            return Color.white;
        }

        private void ApplyLightnessImmediate(float lightness)
        {
            Color c = Color.HSVToRGB(_baseHue, _baseSaturation, Mathf.Clamp01(lightness));
            c.a = _baseColor.a;

            if (useMaterialColor && _renderer != null)
            {
                _renderer.material.color = c;
            }
            else if (_graphic != null)
            {
                _graphic.color = c;
            }
        }
    }
}
