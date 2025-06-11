using UnityEngine;
using Util;

public class MoveWithMouse : MonoBehaviour
{
    private LerpPosition _lerpPosition;

    public void Start()
    {
        _lerpPosition = GetComponent<LerpPosition>();
    }
    
    void Update()
    {
        _lerpPosition.targetLocation = GetMousePositionRelativeToCenter();

    }
    
    Vector2 GetMousePositionRelativeToCenter()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 relativePos = mousePos - screenCenter;

        return new Vector2(Mathf.Min(Mathf.Max(relativePos.x / Screen.width, -1), 1), Mathf.Min(Mathf.Max(relativePos.y / Screen.height, -1), 1));
    }

}
