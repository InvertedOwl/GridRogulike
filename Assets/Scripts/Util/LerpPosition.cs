using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Util
{
    public class LerpPosition : MonoBehaviour
    {
        public float speed = 1;
        [FormerlySerializedAs("target")] public Vector3 targetLocation;
        public Quaternion targetRotation;
        public Vector3 targetScale;
        public Vector3 startPosition;
        public Quaternion startRotation;
        public Vector3 startScale;
        public bool isLocal;
        private bool _isAtLocation;
        public bool IsAtLocation => _isAtLocation;
        public UnityEvent arriveAt;

        private const float PositionThreshold = 0.01f;
        private const float RotationThreshold = 0.01f;
        private const float ScaleThreshold = 0.01f;

        public void Start()
        {
            if (isLocal)
            {
                targetLocation = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
                targetRotation = transform.localRotation;
                targetScale = transform.localScale;
            }
            else
            {
                targetLocation = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                targetRotation = transform.rotation;
                targetScale = transform.localScale;
            }

            startPosition = targetLocation;
            startRotation = targetRotation;
            startScale = targetScale;
        }

        public void Update()
        {
            if (isLocal)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocation, speed * Time.deltaTime);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, speed * Time.deltaTime);
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, speed * Time.deltaTime);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, targetLocation, speed * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, speed * Time.deltaTime);
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, speed * Time.deltaTime);
            }

            bool wasAtLocation = _isAtLocation;
            _isAtLocation = HasReachedTarget();

            if (!_isAtLocation && wasAtLocation)
            {
                // Reset if moved again
                _isAtLocation = false;
            }

            if (_isAtLocation && !wasAtLocation)
            {
                arriveAt?.Invoke();
            }
        }

        private bool HasReachedTarget()
        {
            Vector2 currentPos = isLocal ? new Vector2(transform.localPosition.x, transform.localPosition.y) : new Vector2(transform.position.x, transform.position.y);
            Quaternion currentRot = isLocal ? transform.localRotation : transform.rotation;
            Vector3 currentScale = transform.localScale;

            bool posClose = Vector2.Distance(currentPos, targetLocation) < PositionThreshold;
            bool rotClose = Quaternion.Angle(currentRot, targetRotation) < RotationThreshold;
            bool scaleClose = Vector3.Distance(currentScale, targetScale) < ScaleThreshold;

            return posClose && rotClose && scaleClose;
        }
    }
}
