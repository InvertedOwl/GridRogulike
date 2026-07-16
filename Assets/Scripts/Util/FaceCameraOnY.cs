using UnityEngine;

public class FaceCameraOnY : MonoBehaviour
{
    private Camera mainCamera;

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return;
        }

        Vector3 directionToCamera = mainCamera.transform.position - transform.position;
        directionToCamera.y = 0f;

        if (directionToCamera.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        float yRotation = Quaternion.LookRotation(directionToCamera).eulerAngles.y;
        Vector3 currentRotation = transform.eulerAngles;
        transform.eulerAngles = new Vector3(currentRotation.x, yRotation + 180, currentRotation.z);
    }
}
