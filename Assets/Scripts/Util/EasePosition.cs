using System;
using UnityEngine;

namespace Util
{
    public class EasePosition : MonoBehaviour
    {
        public float durationSeconds = 1f;
        public bool isLocal = true;
        public float _elapsedTime;

        private Vector3 _lastPosition;
        private Vector3 _targetPosition;
        private Action _onComplete;

        private RectTransform _rectTransform;
        private bool _useRectTransform;
        private bool _initialized;

        public Vector3 targetLocation
        {
            get => _targetPosition;
            set => SendToLocation(value);
        }

        public static double EaseInOutCubic(double x)
        {
            return x < 0.5
                ? 4 * x * x * x
                : 1 - Math.Pow(-2 * x + 2, 3) / 2;
        }

        void Awake()
        {
            Initialize();
        }

        void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_initialized)
                return;

            _rectTransform = GetComponent<RectTransform>();
            _useRectTransform = _rectTransform != null;
            Vector3 pos = GetCurrentPositionRaw();
            _lastPosition = pos;
            _targetPosition = pos;
            _initialized = true;
        }

        public void InstantSend(Vector3 position)
        {
            Initialize();

            _lastPosition = position;
            _targetPosition = position;
            _elapsedTime = 0f;

            SetPosition(position);
        }

        public void SendToLocation(Vector3 location, Action onComplete = null)
        {
            Initialize();

            _lastPosition = GetCurrentPosition();
            _targetPosition = location;
            _elapsedTime = 0f;
            _onComplete = onComplete;

            if ((durationSeconds / GameplayNavSettings.speed) <= 0f)
            {
                SetPosition(_targetPosition);
                _onComplete?.Invoke();
                _onComplete = null;
            }
        }

        void Update()
        {
            Initialize();

            if (_elapsedTime >= (durationSeconds / GameplayNavSettings.speed))
                return;

            _elapsedTime += Time.deltaTime;

            float progress = (durationSeconds / GameplayNavSettings.speed) > 0f
                ? Mathf.Clamp01(_elapsedTime / (durationSeconds / GameplayNavSettings.speed))
                : 1f;

            float eased = (float)EaseInOutCubic(progress);
            Vector3 pos = Vector3.Lerp(_lastPosition, _targetPosition, eased);

            SetPosition(pos);

            if (progress >= 1f)
            {
                SetPosition(_targetPosition);
                _onComplete?.Invoke();
                _onComplete = null;
            }
        }

        private Vector3 GetCurrentPosition()
        {
            Initialize();
            return GetCurrentPositionRaw();
        }

        private Vector3 GetCurrentPositionRaw()
        {
            if (_useRectTransform)
                return _rectTransform.anchoredPosition3D;

            return isLocal ? transform.localPosition : transform.position;
        }

        private void SetPosition(Vector3 position)
        {
            if (_useRectTransform)
            {
                _rectTransform.anchoredPosition3D = position;
            }
            else if (isLocal)
            {
                transform.localPosition = position;
            }
            else
            {
                transform.position = position;
            }
        }
    }
}
