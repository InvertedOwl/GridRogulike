using System.Collections;
using UnityEngine;

public class DieAfterSeconds : MonoBehaviour
{
    public float secondsToWait = 2;
    void Start()
    {
        StartCoroutine(WaitSeconds());
    }

    IEnumerator WaitSeconds()
    {
        yield return new WaitForSeconds(secondsToWait);
        Destroy(gameObject);
    }
}
