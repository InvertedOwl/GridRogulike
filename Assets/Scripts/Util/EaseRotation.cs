using System;
using UnityEngine;

namespace Util
{
    public class EaseRotation : MonoBehaviour
    {
        public float durationSeconds = 1f;
        private float _elapsedTime;

        private Vector3 _lastRotation;
        private Vector3 _targetRotation;
        private Action _onComplete;

        public Vector3 targetRotation
        {
            get => _targetRotation;
            set => SendToRotation(value);
        }

        public static double EaseInOutCubic(double x)
        {
            return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
        }

        private void Start()
        {
            _lastRotation = transform.eulerAngles;
            _targetRotation = transform.eulerAngles;
        }

        public void InstantSend(Vector3 rotation)
        {
            _lastRotation = rotation;
            _targetRotation = rotation;
            _elapsedTime = 0f;

            transform.rotation = Quaternion.Euler(_lastRotation);
        }

        public void SendToRotation(Vector3 rotation, Action onComplete = null)
        {
            _lastRotation = transform.eulerAngles;
            _targetRotation = rotation;
            _elapsedTime = 0f;
            _onComplete = onComplete;

            if (durationSeconds <= 0f)
            {
                transform.rotation = Quaternion.Euler(_targetRotation);
                _onComplete?.Invoke();
                _onComplete = null;
            }
        }

        private void Update()
        {
            if (_elapsedTime < durationSeconds)
            {
                _elapsedTime += Time.deltaTime;
                float progress = durationSeconds > 0f
                    ? Mathf.Clamp01(_elapsedTime / durationSeconds)
                    : 1f;

                float eased = (float)EaseInOutCubic(progress);

                // Interpolate each Euler component smoothly
                Vector3 interpolated = new Vector3(
                    Mathf.LerpAngle(_lastRotation.x, _targetRotation.x, eased),
                    Mathf.LerpAngle(_lastRotation.y, _targetRotation.y, eased),
                    Mathf.LerpAngle(_lastRotation.z, _targetRotation.z, eased)
                );

                transform.eulerAngles = interpolated;

                if (progress >= 1f)
                {
                    transform.rotation = Quaternion.Euler(_targetRotation);
                    _onComplete?.Invoke();
                    _onComplete = null;
                }
            }
        }
    }
}
