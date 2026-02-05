using System;
using UnityEngine;

namespace Util
{
    public class EaseRotation : MonoBehaviour
    {
        public float durationSeconds = 0.25f;

        private float _elapsed;
        private Quaternion _from;
        private Quaternion _to;
        private Action _onComplete;

        private bool _hasInitialized;

        public Quaternion TargetRotation
        {
            get => _to;
            set => SendToRotation(value);
        }

        public Vector3 TargetEulerLocal
        {
            get => _to.eulerAngles;
            set => SendToRotation(Quaternion.Euler(value));
        }

        private void Awake()
        {
            InitializeIfNeeded();
        }

        private void OnEnable()
        {
            InitializeIfNeeded(force: true);
        }

        private void InitializeIfNeeded(bool force = false)
        {
            if (_hasInitialized && !force) return;

            var current = transform.localRotation;
            _from = current;
            _to = current;
            _elapsed = durationSeconds;
            _onComplete = null;

            _hasInitialized = true;
        }

        public static float EaseInOutCubic(float x)
        {
            return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
        }

        public void InstantSend(Quaternion localRotation, Action onComplete = null)
        {
            InitializeIfNeeded();

            _from = localRotation;
            _to = localRotation;
            _elapsed = durationSeconds;

            transform.localRotation = localRotation;

            _onComplete = null;
            onComplete?.Invoke();
        }

        public void InstantSend(Vector3 localEuler, Action onComplete = null)
            => InstantSend(Quaternion.Euler(localEuler), onComplete);

        public void SendToRotation(Quaternion localRotation, Action onComplete = null)
        {
            InitializeIfNeeded();

            _from = transform.localRotation;
            _to = localRotation;
            _elapsed = 0f;
            _onComplete = onComplete;

            if (durationSeconds <= 0f)
            {
                transform.localRotation = _to;
                _onComplete?.Invoke();
                _onComplete = null;
                _elapsed = durationSeconds;
            }
        }

        public void SendToRotation(Vector3 localEuler, Action onComplete = null)
            => SendToRotation(Quaternion.Euler(localEuler), onComplete);

        private void Update()
        {
            if (!_hasInitialized) InitializeIfNeeded();
            if (durationSeconds <= 0f) return;
            if (_elapsed >= durationSeconds) return;

            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / durationSeconds);
            float eased = EaseInOutCubic(t);

            transform.localRotation = Quaternion.Slerp(_from, _to, eased);

            if (t >= 1f)
            {
                transform.localRotation = _to;
                _onComplete?.Invoke();
                _onComplete = null;
                _elapsed = durationSeconds;
            }
        }
    }
}
