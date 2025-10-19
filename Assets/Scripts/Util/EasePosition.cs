using System;
using UnityEngine;

namespace Util
{
    public class EasePosition : MonoBehaviour
    {
        public float durationSeconds = 1;
        public bool isLocal;
        public float _elapsedTime;
        
        private Vector3 _lastPosition;
        private Vector3 _targetPosition;
        private Action _onComplete;

        public Vector3 targetLocation
        {
            set { SendToLocation(value); }
            get { return _targetPosition; }
        }

        public static double EaseInOutCubic(double x)
        {
            return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
        }

        public void Start()
        {
            _lastPosition = isLocal ? transform.localPosition : transform.position;
            _targetPosition = isLocal ? transform.localPosition : transform.position;
        }

        public void InstantSend(Vector3 position)
        {
            _lastPosition = position;
            _targetPosition = position;
            _elapsedTime = 0;

            // Clear or capture the callback BEFORE invoking it to prevent re-entrancy loops
            if (isLocal)
                transform.localPosition = position;
            else
                transform.position = position;
        }

        public void SendToLocation(Vector3 location, Action onComplete = null)
        {

            // CHANGED: Set _lastPosition to current position to prevent jumping
            _lastPosition = isLocal ? transform.localPosition : transform.position;
            _targetPosition = location;
            _elapsedTime = 0f;

            _onComplete = onComplete;

            if (durationSeconds <= 0f)
            {
                if (isLocal)
                    transform.localPosition = _targetPosition;
                else
                    transform.position = _targetPosition;

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

                Vector3 pos = Vector3.Lerp(_lastPosition, _targetPosition, eased);

                if (isLocal)
                    transform.localPosition = pos;
                else
                    transform.position = pos;

                if (progress >= 1f)
                {
                    if (isLocal)
                        transform.localPosition = _targetPosition;
                    else
                        transform.position = _targetPosition;

                    _onComplete?.Invoke();
                    _onComplete = null;
                }
            }
        }
    }
}