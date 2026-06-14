using UnityEngine;

public class GravitySettings : MonoBehaviour
{
    private void Awake()
    {
        Physics.gravity = new Vector3(0f, 0f, 9.81f);
    }
}