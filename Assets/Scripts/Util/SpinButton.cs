using UnityEngine;
using Util;

public class SpinButton : MonoBehaviour
{
    public void Rotate()
    {
        GetComponent<EaseRotation>().SendToRotation(-359);
    }
}
