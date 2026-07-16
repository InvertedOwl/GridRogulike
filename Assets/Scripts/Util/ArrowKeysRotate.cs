using UnityEngine;
using Util;

public class ArrowKeysRotate : MonoBehaviour
{
    [SerializeField] private EaseRotation easeRotation;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            easeRotation.SendToRotation(easeRotation.TargetEulerLocal + new Vector3(0, 0, 60));
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            easeRotation.SendToRotation(easeRotation.TargetEulerLocal + new Vector3(0, 0, -60));
        }
    }
}
