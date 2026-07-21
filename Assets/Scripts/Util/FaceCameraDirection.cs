using UnityEngine;
using Util;

[DisallowMultipleComponent]
public class FaceCameraDirection : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool keepLerpPositionInSync = true;

    private LerpPosition _lerpPosition;

    private void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        // Directional billboard: every object shares the camera's orientation instead
        // of individually turning toward the camera's position.
        transform.rotation = targetCamera.transform.rotation;

        if (!keepLerpPositionInSync)
            return;

        if (_lerpPosition == null)
            _lerpPosition = GetComponent<LerpPosition>();

        if (_lerpPosition != null)
        {
            _lerpPosition.targetRotation = _lerpPosition.isLocal
                ? transform.localRotation
                : transform.rotation;
        }
    }
}
