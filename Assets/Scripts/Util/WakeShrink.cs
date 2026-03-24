using UnityEngine;

public class WakeShrink : MonoBehaviour
{
    private bool hasSet = false;
    void Update()
    {
        if (!hasSet)
        {
            this.GetComponent<EaseScale>().scale = Vector3.zero;
            hasSet = true;
        }
    }
}
