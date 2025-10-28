using System;
using UnityEngine;

namespace Util
{
    public class EaseRotation : MonoBehaviour
    {
        public float durationSeconds = 1;
        public float _elapsedTime;
        
        private float _lastRotation;
        private float _targetRotation;
        private Action _onComplete;

        public float targetRotation
        {
            set { SendToRotation(value); }
            get { return _targetRotation; }
        }

        public static double EaseInOutCubic(double x)
        {
            return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
        }

        public void Start()
        {
            _lastRotation = transform.eulerAngles.z;
            _targetRotation = transform.eulerAngles.z;
        }

        public void InstantSend(float rotation)
        {
            _lastRotation = rotation;
            _targetRotation = rotation;
            _elapsedTime = 0;

            transform.rotation = Quaternion.Euler(0, 0, _lastRotation);
        }

        public void SendToRotation(float rotation, Action onComplete = null)
        {

            _lastRotation = transform.eulerAngles.z;
            _targetRotation = rotation;
            _elapsedTime = 0f;

            _onComplete = onComplete;

            if (durationSeconds <= 0f)
            {
                transform.rotation = Quaternion.Euler(0, 0, _targetRotation);

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

                float pos = Mathf.Lerp(_lastRotation, _targetRotation, eased);

                transform.eulerAngles = new Vector3(0, 0, pos);

                if (progress >= 1f)
                {
                    transform.rotation = Quaternion.Euler(0, 0, _targetRotation);

                    _onComplete?.Invoke();
                    _onComplete = null;
                }
            }
        }
    }
}