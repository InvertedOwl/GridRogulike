using System;
using UnityEngine;

namespace Util
{
    public class EasePosition: MonoBehaviour
    {
        public float durationSeconds = 1;
        public bool isLocal;
        private Vector3 _lastPosition;
        private Vector3 _targetPosition;
        public float _elapsedTime;
            
        public static double EaseInOutCubic(double x)
        {
            return x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;
        }

        public void Start()
        {
            _lastPosition = isLocal ? transform.localPosition : transform.position;
            _targetPosition = isLocal ? transform.localPosition : transform.position;
        }
        
        public void SendToLocation(Vector3 location)
        {
            
            Debug.Log("Send to; " + location);
            _lastPosition = new Vector3(_targetPosition.x, _targetPosition.y, _targetPosition.z);
            _targetPosition = location;
            _elapsedTime = 0;
        }
        
        public void Update()
        {
            if (_elapsedTime < durationSeconds)
            {
                _elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(_elapsedTime / durationSeconds);

                float eased = (float) EaseInOutCubic(progress);

                Vector3 pos = Vector3.Lerp(_lastPosition, _targetPosition, eased);
                Debug.Log(_lastPosition + " Last");
                Debug.Log(_targetPosition + " Target");
                if (isLocal)
                    transform.localPosition = pos;
                else
                    transform.position = pos;
            }
            

        }

    }
}