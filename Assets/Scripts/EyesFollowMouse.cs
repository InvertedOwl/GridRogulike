using UnityEngine;

public class EyesFollowMouse : MonoBehaviour
{
    private Vector3 targetPosition;
    public bool targetMouse;
    public Vector3 setTargetPosition;
    public int speed = 15;
    
    public float distanceDivisor;
    void Update()
    {
        if (targetMouse)
            setTargetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        targetPosition = Vector3.Lerp(targetPosition, setTargetPosition, Time.deltaTime * speed);
        
        Vector3 parentPos = transform.parent.position;

        Vector2 pos = -(parentPos - targetPosition).normalized;
        transform.localPosition = pos;
        transform.localPosition = transform.localPosition.normalized / distanceDivisor;
    }
}
