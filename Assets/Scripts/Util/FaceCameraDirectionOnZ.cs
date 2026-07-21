using UnityEngine;

public class FaceCameraDirectionOnZ : MonoBehaviour
{
    private Camera _mainCamera;

    private void LateUpdate()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_mainCamera == null)
            return;

        Vector3 cameraUp = _mainCamera.transform.up;
        Vector2 projectedCameraUp = new Vector2(cameraUp.x, cameraUp.y);
        if (projectedCameraUp.sqrMagnitude <= Mathf.Epsilon)
            return;

        float cameraZRotation = Mathf.Atan2(
            -projectedCameraUp.x,
            projectedCameraUp.y
        ) * Mathf.Rad2Deg;

        Vector3 worldEulerAngles = transform.eulerAngles;
        worldEulerAngles.z = cameraZRotation;
        transform.eulerAngles = worldEulerAngles;
    }
}
