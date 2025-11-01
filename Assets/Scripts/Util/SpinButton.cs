using UnityEngine;
using Util;

public class SpinButton : MonoBehaviour
{
    public void Rotate()
    {
        GetComponent<EaseRotation>().SendToRotation(new Vector3(0, 0, -359));
    }
}
