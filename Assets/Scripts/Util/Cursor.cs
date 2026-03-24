using UnityEngine;

public class Cursor : MonoBehaviour
{
    void Update()
    {
        transform.position = Input.mousePosition;
        UnityEngine.Cursor.visible = !Application.isFocused;
        
    }
}
