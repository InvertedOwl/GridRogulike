using UnityEngine;
using Util;

public class MoveWithMouse : MonoBehaviour
{
    private LerpPosition _lerpPosition;
    public Vector2 maxLocalOffset = new Vector2(5f, 3f);
    public float sensitivity = 1f;
    public bool invertX = false;
    public bool invertY = false;

    void Start()
    {
        _lerpPosition = GetComponent<LerpPosition>();
    }

    void Update()
    {
        Vector2 norm = GetMouseNormalizedMinusOneToOne();
        if (invertX) norm.x = -norm.x;
        if (invertY) norm.y = -norm.y;

        Vector2 localTarget = Vector2.Scale(norm, maxLocalOffset) * sensitivity;

        _lerpPosition.targetLocation = localTarget; 
    }

    private Vector2 GetMouseNormalizedMinusOneToOne()
    {
        Vector3 v = Camera.main != null 
            ? Camera.main.ScreenToViewportPoint(Input.mousePosition)
            : new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height, 0f);

        return new Vector2((v.x - 0.5f) * 2f, (v.y - 0.5f) * 2f);
    }
}