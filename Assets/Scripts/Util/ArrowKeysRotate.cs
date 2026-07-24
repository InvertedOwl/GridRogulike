using UnityEngine;
using Util;

public class ArrowKeysRotate : MonoBehaviour
{
    [SerializeField] private EaseRotation easeRotation;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            HoverStateInvalidator.ReleaseAll();
            easeRotation.SendToRotation(easeRotation.TargetEulerLocal + new Vector3(0f, 0f, -60f));
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            HoverStateInvalidator.ReleaseAll();
            easeRotation.SendToRotation(easeRotation.TargetEulerLocal + new Vector3(0f, 0f, 60f));
        }
    }
}
